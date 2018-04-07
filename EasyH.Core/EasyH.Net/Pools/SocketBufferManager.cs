using System;
using System.Collections.Generic;
using System.Net.Sockets;
using EasyH.Net.Base;

namespace EasyH.Net.Pools
{
    internal class SocketBufferManager
    {
        private readonly int _wTotalSize;
        private int _wCurIndex;
        private readonly int _blockSize = 2048;
        private LockParam _lockParam = new LockParam();
        private readonly byte[] _wBuffer;
        private readonly Queue<int> _freeBufferIndexPool;

        /// <summary>
        /// 块缓冲区大小
        /// </summary>
        public int BlockSize => _blockSize;

        /// <summary>
        /// 缓冲区管理构造
        /// </summary>
        /// <param name="maxCounts"></param>
        /// <param name="blockSize"></param>
        public SocketBufferManager(int maxCounts, int blockSize)
        {
            if (blockSize < 4) blockSize = 4;
             
            _blockSize = blockSize;
            _wCurIndex = 0;
            _wTotalSize = maxCounts * blockSize;
            _wBuffer = new byte[_wTotalSize];
            _freeBufferIndexPool = new Queue<int>(maxCounts);
        }

        public void Clear()
        {
            using (new LockWait(ref _lockParam))
            {
                _freeBufferIndexPool.Clear();
            }
        }

        /// <summary>
        /// 设置缓冲区
        /// </summary>
        /// <param name="agrs"></param>
        /// <returns></returns>
        public bool SetBuffer(SocketAsyncEventArgs agrs)
        {
            using (new LockWait(ref _lockParam))
            {
                if (_freeBufferIndexPool.Count > 0)
                {
                    agrs.SetBuffer(_wBuffer, _freeBufferIndexPool.Dequeue(), _blockSize);
                }
                else
                {
                    if ((_wTotalSize - _blockSize) < _wCurIndex) return false;

                    agrs.SetBuffer(_wBuffer, _wCurIndex, _blockSize);

                    _wCurIndex += _blockSize;
                }
                return true;
            }
        }

        /// <summary>
        /// 写入缓冲区
        /// </summary>
        /// <param name="agrs"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public bool WriteBuffer(SocketAsyncEventArgs agrs, byte[] buffer, int offset, int cnt)
        {
            using (new LockWait(ref _lockParam))
            {
                //超出缓冲区则不写入
                if (agrs.Offset + cnt > _wBuffer.Length)
                {
                    return false;
                }

                //超出块缓冲区则不写入
                if (cnt > _blockSize) return false;

                Buffer.BlockCopy(buffer, offset, _wBuffer, agrs.Offset, cnt);

                agrs.SetBuffer(_wBuffer, agrs.Offset, cnt);

                return true;
            }
        }

        /// <summary>
        /// 释放缓冲区
        /// </summary>
        /// <param name="args"></param>
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            using (new LockWait(ref _lockParam))
            {
                _freeBufferIndexPool.Enqueue(args.Offset);
                args.SetBuffer(null, 0, 0);
            }
        }

        /// <summary>
        /// 自动按发送缓冲区的块大小分多次包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public ArraySegment<byte>[] BufferToSegments(byte[] buffer, int offset, int size)
        {
            if (size <= _blockSize)
                return new[] { new ArraySegment<byte>(buffer, offset, size) };

            var bSize = _blockSize;
            var bCnt = size / _blockSize;
            var isRem = false;
            if (size % _blockSize != 0)
            {
                isRem = true;
                bCnt += 1;
            }

            var segItems = new ArraySegment<byte>[bCnt];
            for (var i = 0; i < bCnt; ++i)
            {
                var bOffset = i * _blockSize;

                if (i == (bCnt - 1) && isRem)
                {
                    bSize = size - bOffset;
                }
                segItems[i] = new ArraySegment<byte>(buffer, offset + bOffset, bSize);
            }
            return segItems;
        }
    }
}