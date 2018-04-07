using System;
using System.Collections.Generic;
using System.Threading;
using EasyH.Net.Base;

namespace EasyH.Net.Pools
{
    internal class SocketTokenManager<T>
    {
        private readonly Queue<T> _collection;
        private LockParam _lockParam = new LockParam();

        public int Count => _collection.Count;


        public int Capacity { get; } = 4;

        public SocketTokenManager(int capacity = 32)
        {
            Capacity = capacity;
            _collection = new Queue<T>(capacity);
        }


        public T Get()
        {
            using (new LockWait(ref _lockParam))
            {
                return _collection.Count > 0 ? _collection.Dequeue() : default(T);
            }
        }


        public void Set(T item)
        {
            using (new LockWait(ref _lockParam))
            {
                _collection.Enqueue(item);
            }
        }


        public void Clear()
        {
            using (new LockWait(ref _lockParam))
            {
                _collection.Clear();
            }
        }

        public void ClearToCloseToken()
        {
            using (new LockWait(ref _lockParam))
            {
                while (_collection.Count > 0)
                {
                    var token = _collection.Dequeue() as SocketToken;
                    token?.Close();
                }
            }
        }

        public void ClearToCloseArgs()
        {
            using (new LockWait(ref _lockParam))
            {
                while (_collection.Count > 0)
                {
                    var token = _collection.Dequeue() as System.Net.Sockets.SocketAsyncEventArgs;
                    token?.Dispose();
                }
            }
        }

        public T GetEmptyWait(Func<int, bool> fun, bool isWaitingFor = false)
        {
            var retry = 1;

            while (true)
            {
                var tArgs = Get();
                if (tArgs != null) return tArgs;
                if (isWaitingFor == false)
                {
                    if (retry > 16) break;
                    ++retry;
                }

                var isContinue = fun(retry);
                if (isContinue == false) break;

                Thread.Sleep(1000 * retry);
            }
            return default(T);
        }
    }
}