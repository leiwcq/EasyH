/*-------------------------------------------------------------
 *   auth: bouyei
 *   date: 2017/7/29 13:43:40
 *contact: 453840293@qq.com
 *profile: www.openthinking.cn
 *   guid: 7b7a759e-571f-4486-969a-5306e9dc0f51
---------------------------------------------------------------*/

using System;
using EasyH.Net.Base;
using EasyH.Net.Common;
using EasyH.Net.Providers.Interface;
using EasyH.Net.Providers.Tcp;
using EasyH.Net.Providers.Udp;

namespace EasyH.Net.Providers.Instance
{
    public class NetServerProvider : INetServerProvider
    {
        #region variable
        private readonly TcpServerProvider _tcpServerProvider;
        private readonly UdpServerProvider _udpServerProvider;
        private readonly int _chunkBufferSize = 4096;
        private readonly int _maxNumberOfConnections = 512;
        private bool _isDisposed;
        #endregion

        #region property
        private OnReceiveHandler _receiveHanlder;
        public OnReceiveHandler ReceiveHandler
        {
            get => _receiveHanlder;
            set
            {
                _receiveHanlder = value;
                switch (NetProviderType)
                {
                    case NetProviderType.Tcp:
                        _tcpServerProvider.ReceivedCallback = _receiveHanlder;
                        break;
                    case NetProviderType.Udp:
                        _udpServerProvider.ReceiveCallbackHandler = _receiveHanlder;
                        break;
                }
            }
        }

        private OnSentHandler _sentHanlder;
        public OnSentHandler SentHandler
        {
            get => _sentHanlder;
            set
            {
                _sentHanlder = value;
                switch (NetProviderType)
                {
                    case NetProviderType.Tcp:
                        _tcpServerProvider.SentCallback = _sentHanlder;
                        break;
                    case NetProviderType.Udp:
                        _udpServerProvider.SentCallbackHandler = _sentHanlder;
                        break;
                }
            }
        }

        private OnAcceptHandler _acceptHanlder;
        public OnAcceptHandler AcceptHandler
        {
            get => _acceptHanlder;
            set
            {
                _acceptHanlder = value;
                if (NetProviderType.Tcp == NetProviderType)
                {
                    _tcpServerProvider.AcceptedCallback = _acceptHanlder;
                }
            }
        }

        private OnReceiveOffsetHandler _receiveOffsetHandler;
        public OnReceiveOffsetHandler ReceiveOffsetHandler
        {
            get => _receiveOffsetHandler;
            set
            {
                _receiveOffsetHandler = value;
                switch (NetProviderType)
                {
                    case NetProviderType.Tcp:
                        _tcpServerProvider.ReceiveOffsetCallback = _receiveOffsetHandler;
                        break;
                    case NetProviderType.Udp:
                        _udpServerProvider.ReceiveOffsetHanlder = _receiveOffsetHandler;
                        break;
                }
            }
        }

        private OnDisconnectedHandler _disconnectedHanlder;
        public OnDisconnectedHandler DisconnectedHandler
        {
            get => _disconnectedHanlder;
            set
            {
                _disconnectedHanlder = value;
                switch (NetProviderType)
                {
                    case NetProviderType.Tcp:
                        _tcpServerProvider.DisconnectedCallback = _disconnectedHanlder;
                        break;
                    case NetProviderType.Udp:
                        _udpServerProvider.DisconnectedCallbackHandler = _disconnectedHanlder;
                        break;
                }
            }
        }

        public NetProviderType NetProviderType { get; }

        #endregion

        #region constructor
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_isDisposed) return;

            if (isDisposing)
            {
                _tcpServerProvider?.Dispose();

                _udpServerProvider?.Dispose();
                _isDisposed = true;
            }
        }

        public NetServerProvider(
            int chunkBufferSize = 4096,
            int maxNumberOfConnections = 64,
            NetProviderType netProviderType = NetProviderType.Tcp)
        {
            NetProviderType = netProviderType;
            _chunkBufferSize = chunkBufferSize;
            _maxNumberOfConnections = maxNumberOfConnections;

            switch (netProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpServerProvider = new TcpServerProvider(maxNumberOfConnections, chunkBufferSize);
                    break;
                case NetProviderType.Udp:
                    _udpServerProvider = new UdpServerProvider();
                    break;
            }
        }

        public NetServerProvider(NetProviderType netProviderType)
        {
            NetProviderType = netProviderType;

            switch (netProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpServerProvider = new TcpServerProvider(_maxNumberOfConnections, _chunkBufferSize);
                    break;
                case NetProviderType.Udp:
                    _udpServerProvider = new UdpServerProvider();
                    break;
            }
        }

        public static NetServerProvider CreateProvider(
            int chunkBufferSize = 4096,
            int maxNumberOfConnections = 64,
            NetProviderType netProviderType = NetProviderType.Tcp)
        {
            return new NetServerProvider(chunkBufferSize, maxNumberOfConnections, netProviderType);
        }

        #endregion

        #region public method
        public bool Start(int port, string ip = "0.0.0.0")
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    return _tcpServerProvider.Start(port, ip);
                case NetProviderType.Udp:
                    _udpServerProvider.Start(port, _chunkBufferSize, _maxNumberOfConnections);
                    return true;
            }

            return false;
        }

        public void Stop()
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpServerProvider.Stop();
                    break;
                case NetProviderType.Udp:
                    _udpServerProvider.Stop();
                    break;
            }
        }

        public bool Send(SegmentOffsetToken segToken, bool waiting = true)
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    return _tcpServerProvider.Send(segToken, waiting);
                case NetProviderType.Udp:
                    return _udpServerProvider.Send(segToken.DataSegment,segToken.SToken.TokenIpEndPoint, waiting);
            }

            return false;
        }

        public int SendSync(SegmentOffsetToken segToken)
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpServerProvider.SendSync(segToken);
                    break;
                case NetProviderType.Udp:
                    return _udpServerProvider.SendSync(
                        segToken.SToken.TokenIpEndPoint, segToken.DataSegment);
            }

            return 0;
        }

        public void CloseToken(SocketToken sToken)
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpServerProvider.Close(sToken);
                    break;
                case NetProviderType.Udp:
                    break;
            }
        }
        #endregion
    }
}
