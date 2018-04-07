using System;
using EasyH.Net.Base;
using EasyH.Net.Common;
using EasyH.Net.Providers.Interface;
using EasyH.Net.Providers.Tcp;
using EasyH.Net.Providers.Udp;

namespace EasyH.Net.Providers.Instance
{
    public class NetClientProvider : INetClientProvider
    {
        #region variable
        private bool _isDisposed;
        private readonly int _chunkBufferSize = 4096;
        private readonly int _sendConcurrentSize = 64;
        private readonly TcpClientProvider _tcpClientProvider;
        private readonly UdpClientProvider _udpClientProvider;
        #endregion

        #region property

        public bool IsConnected => NetProviderType.Tcp == NetProviderType && _tcpClientProvider.IsConnected;

        /// <summary>
        /// 发送缓冲区个数
        /// </summary>
        public int BufferPoolCount
        {
            get
            {
                switch (NetProviderType)
                {
                    case NetProviderType.Tcp:
                        return _tcpClientProvider.SendBufferPoolNumber;
                    case NetProviderType.Udp:
                        return _udpClientProvider.SendBufferPoolNumber;
                    default:
                        return 0;
                }
            }
        }

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
                        _tcpClientProvider.RecievedCallback = _receiveHanlder;
                        break;
                    case NetProviderType.Udp:
                        _udpClientProvider.ReceiveCallbackHandler = _receiveHanlder;
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
                        _tcpClientProvider.SentCallback = _sentHanlder;
                        break;
                    case NetProviderType.Udp:
                        _udpClientProvider.SentCallbackHandler = _sentHanlder;
                        break;
                }
            }
        }

        private OnConnectedHandler _connectedHanlder;
        public OnConnectedHandler ConnectedHandler
        {
            get => _connectedHanlder;
            set
            {
                _connectedHanlder = value;
                if (NetProviderType.Tcp == NetProviderType)
                {
                    _tcpClientProvider.ConnectedCallback = _connectedHanlder;
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
                        _tcpClientProvider.ReceiveOffsetCallback = _receiveOffsetHandler;
                        break;
                    case NetProviderType.Udp:
                        _udpClientProvider.ReceiveOffsetHandler = _receiveOffsetHandler;
                        break;
                }
            }
        }

        private OnDisconnectedHandler _disconnectedHandler;
        public OnDisconnectedHandler DisconnectedHandler
        {
            get => _disconnectedHandler;
            set
            {
                _disconnectedHandler = value;
                if (NetProviderType.Tcp == NetProviderType)
                {
                    _tcpClientProvider.DisconnectedCallback = _disconnectedHandler;
                }
            }
        }

        public NetProviderType NetProviderType { get; }

        public ChannelProviderType ChannelProviderType => NetProviderType.Tcp == NetProviderType ? _tcpClientProvider.ChannelProviderState : ChannelProviderType.Async;

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
                _tcpClientProvider?.Dispose();

                _udpClientProvider?.Dispose();

                _isDisposed = true;
            }
        }

        public NetClientProvider(
            int chunkBufferSize = 4096, 
            int sendConcurrentSize = 8,
             NetProviderType netProviderType = NetProviderType.Tcp)
        {
            NetProviderType = netProviderType;
            _chunkBufferSize = chunkBufferSize;
            _sendConcurrentSize = sendConcurrentSize;

            if (netProviderType == NetProviderType.Tcp)
            {
                _tcpClientProvider = new TcpClientProvider(chunkBufferSize, sendConcurrentSize);
            }
            else if (netProviderType == NetProviderType.Udp)
            {
                _udpClientProvider = new UdpClientProvider(chunkBufferSize,sendConcurrentSize);
            }
        }

        public NetClientProvider(NetProviderType netProviderType)
        {
            NetProviderType = netProviderType;

            switch (netProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpClientProvider = new TcpClientProvider(_chunkBufferSize, _sendConcurrentSize);
                    break;
                case NetProviderType.Udp:
                    _udpClientProvider = new UdpClientProvider(_chunkBufferSize,_sendConcurrentSize);
                    break;
            }
        }

        public static NetClientProvider CreateProvider(
             int chunkBufferSize = 4096, 
             int sendConcurrentSize = 8,
             NetProviderType netProviderType = NetProviderType.Tcp)
        {
            return new NetClientProvider(chunkBufferSize, sendConcurrentSize, netProviderType);
        }

        #endregion

        #region public method
        public void Disconnect()
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpClientProvider.Disconnect();
                    break;
                case NetProviderType.Udp:
                    _udpClientProvider.Disconnect();
                    break;
            }
        }

        public void Connect(int port, string ip)
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpClientProvider.Connect(port, ip);
                    break;
                case NetProviderType.Udp:
                    _udpClientProvider.Connect(port, ip);
                    break;
            }
        }

        public bool ConnectTo(int port, string ip)
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    return _tcpClientProvider.ConnectTo(port, ip);
                case NetProviderType.Udp:
                    return _udpClientProvider.Connect(port,ip);
            }

            return false;
        }

        public bool Send(SegmentOffset dataSegment, bool waiting = true)
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    return  _tcpClientProvider.Send(dataSegment, waiting);
                case NetProviderType.Udp:
                    return _udpClientProvider.Send(dataSegment, waiting);
            }

            return false;
        }

        public bool ConnectSync(int port, string ip)
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    return _tcpClientProvider.ConnectSync(port, ip);
                case NetProviderType.Udp:
                    _udpClientProvider.Connect(port,ip);
                    return true;
            }

            return false;
        }

        public void SendSync(SegmentOffset sendSegment, SegmentOffset receiveSegment)
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpClientProvider.SendSync(sendSegment, receiveSegment);
                    break;
                case NetProviderType.Udp:
                    _udpClientProvider.SendSync(sendSegment, receiveSegment);
                    break;
            }
        }

        public void ReceiveSync(SegmentOffset receiveSegment, Action<SegmentOffset> receiveAction)
        {
            switch (NetProviderType)
            {
                case NetProviderType.Tcp:
                    _tcpClientProvider.ReceiveSync( receiveSegment, receiveAction);
                    break;
                case NetProviderType.Udp:
                    _udpClientProvider.ReceiveSync(receiveSegment, receiveAction);
                    break;
            }
        }
        #endregion
    }
}
