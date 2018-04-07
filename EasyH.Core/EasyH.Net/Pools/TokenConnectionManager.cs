using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EasyH.Net.Base;

namespace EasyH.Net.Pools
{
    internal class TokenConnectionManager
    {
        private readonly LinkedList<NetConnectionToken> _list;
        private int _period = 60;//s
        private readonly Timer _timeoutThreading;
        private LockParam _lockParam;

        public int ConnectionTimeout { get; set; } = 60;//s

        public int Count => _list.Count;

        public TokenConnectionManager(int period)
        {
            _period = period < 2 ? 2 : period;
 
            _lockParam = new LockParam();
            var tempPeriod = GetPeriodSeconds();
            _list = new LinkedList<NetConnectionToken>();
            _timeoutThreading = new Timer(timeoutHandler, null, tempPeriod, tempPeriod);
        }

        private int GetPeriodSeconds()
        {
            return (_period * 1000) >> 1;
        }

        public void TimerEnable(bool isContinue)
        {
            if (isContinue)
            {
                var period = GetPeriodSeconds();
                _timeoutThreading.Change(period, period);
            }
            else _timeoutThreading.Change(-1, -1);
        }

        public void TimeoutChange(int period)
        {
            _period = period;
            if (period < 2) _period = 2;

            var p = GetPeriodSeconds();
            _timeoutThreading.Change(p, p);
        }

        public NetConnectionToken GetTopToken()
        {
            using (new LockWait(ref _lockParam))
            {
                return _list.Count > 0 ? _list.First() : null;
            }
        }

        public IEnumerable<NetConnectionToken> ReadNext()
        {
            using (new LockWait(ref _lockParam))
            {
                foreach (var l in _list)
                {
                    yield return l;
                }
            }
        }

        public void InsertToken(NetConnectionToken ncToken)
        {
            using (new LockWait(ref _lockParam))
            {
                _list.AddLast(ncToken);
            }
        }

        public bool RemoveToken(NetConnectionToken ncToken,bool isClose)
        {
            using (new LockWait(ref _lockParam))
            {
                if (isClose) ncToken.Token.Close();
                return _list.Remove(ncToken);
            }
        }

        public bool RemoveToken(SocketToken sToken)
        {
            using (new LockWait(ref _lockParam))
            {
                var item = _list.FirstOrDefault(x => x.Token.CompareTo(sToken) == 0);
                if (item != null)
                {
                    return _list.Remove(item);
                }
            }

            return false;
        }

        public NetConnectionToken GetTokenById(int id)
        {
            using (new LockWait(ref _lockParam))
            {
                return _list.FirstOrDefault(x => x.Token.TokenId == id);
            }
        }

        public NetConnectionToken GetTokenBySocketToken(SocketToken sToken)
        {
            using (new LockWait(ref _lockParam))
            {
                return _list.FirstOrDefault(x => x.Token.CompareTo(sToken) == 0);
            }
        }

        public void Clear(bool isClose)
        {
            using (new LockWait(ref _lockParam))
            {
                while (_list.Count > 0)
                {
                    var item = _list.First();
                    _list.RemoveFirst();

                    if (isClose)
                    {
                        item.Token?.Close();
                    }
                }
            }
        }

        public bool RefreshConnectionToken(SocketToken sToken)
        {
            using (new LockWait(ref _lockParam))
            {
                var rt = _list.Find(new NetConnectionToken(sToken));

                if (rt == null) return false;

                rt.Value.ConnectionTime = DateTime.Now;
                return true;
            }
        }

        private void timeoutHandler(object obj)
        {
            using (new LockWait(ref _lockParam))
            {
                foreach (var item in _list)
                {
                    if (item.Verification == false 
                        || DateTime.Now.Subtract(item.ConnectionTime).TotalSeconds >= ConnectionTimeout)
                    {
                        item.Token.Close();
                        _list.Remove(item);
                    }
                }
            }
        }
    }
}
