using System;
using System.Threading;

namespace EasyH.Net.Base
{
    internal class LockWait:IDisposable
    {
        private readonly LockParam _param;
        public LockWait(ref LockParam param)
        {
            _param = param;
            while (Interlocked.CompareExchange(ref param.Signal, 1, 0) == 1)
            {
                Thread.Sleep(param.SleepInterval);
            }
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _param.Signal, 0);
        }
    }

    internal class LockParam
    {
        internal int Signal;

        internal int SleepInterval = 1;//ms
    }
}
