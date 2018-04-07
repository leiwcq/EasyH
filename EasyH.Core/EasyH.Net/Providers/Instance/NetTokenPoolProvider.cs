using System.Collections.Generic;
using EasyH.Net.Base;
using EasyH.Net.Pools;
using EasyH.Net.Providers.Interface;

namespace EasyH.Net.Providers.Instance
{
    public class NetTokenPoolProvider:INetTokenPoolProvider
    {
        private readonly TokenConnectionManager _tokenManager;
        public int ConnectionTimeout
        {
            get => _tokenManager.ConnectionTimeout;
            set => _tokenManager.ConnectionTimeout = value;
        }

        public int Count => _tokenManager.Count;

        public static NetTokenPoolProvider CreateProvider(int taskExecutePeriod)
        {
            return new NetTokenPoolProvider(taskExecutePeriod);
        }

        public NetTokenPoolProvider(int taskExecutePeriod)
        {
            _tokenManager = new TokenConnectionManager(taskExecutePeriod);
        }

        public void TimerEnable(bool isContinue)
        {
            _tokenManager.TimerEnable(isContinue);
        }

        public NetConnectionToken GetTopToken()
        {
           return _tokenManager.GetTopToken();
        }

        public void InsertToken(NetConnectionToken ncToken)
        {
            _tokenManager.InsertToken(ncToken);
        }


        public IEnumerable<NetConnectionToken> Reader()
        {
            foreach (var item in _tokenManager.ReadNext())
            {
                yield return item;
            }
        }

        public bool RemoveToken(NetConnectionToken ncToken,bool isClose=true)
        {
          return  _tokenManager.RemoveToken(ncToken,isClose);
        }

        public NetConnectionToken GetTokenById(int id)
        {
          return  _tokenManager.GetTokenById(id);
        }

        public NetConnectionToken GetTokenBySocketToken(SocketToken sToken)
        {
            return _tokenManager.GetTokenBySocketToken(sToken); 
        }

        public bool RefreshExpireToken(SocketToken sToken)
        {
            return _tokenManager.RefreshConnectionToken(sToken);
        }

        public void Clear(bool isClose=true)
        {
            _tokenManager.Clear(isClose);
        }
    }
}
