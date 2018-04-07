using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EasyH.Net.Base;
using EasyH.Net.Common;
using EasyH.Net.Pools;

namespace EasyH.Net.Providers.Tcp
{
    public class TcpClientProvider : TcpSocket,IDisposable
    {
        #region variable
        private bool _isDisposed;
        private readonly int _bufferNumber = 8;
        private ChannelProviderType _channelProviderState = ChannelProviderType.Async;
        private LockParam _lockParam = new LockParam();
        private SocketTokenManager<SocketAsyncEventArgs> _sendTokenManager;
        private SocketBufferManager _sBufferManager;
        private readonly AutoResetEvent _mReset = new AutoResetEvent(false);
        #endregion

        #region properties
        /// <summary>
        /// 发送回调处理
        /// </summary>
        public OnSentHandler SentCallback { get; set; }

        /// <summary>
        /// 接收数据回调处理
        /// </summary>
        public OnReceiveHandler RecievedCallback { get; set; }

        /// <summary>
        /// 接受数据回调，返回缓冲区和偏移量
        /// </summary>
        public OnReceiveOffsetHandler ReceiveOffsetCallback { get; set; }

        /// <summary>
        /// 断开连接回调处理
        /// </summary>
        public OnDisconnectedHandler DisconnectedCallback { get; set; }

        /// <summary>
        /// 连接回调处理
        /// </summary>
        public OnConnectedHandler ConnectedCallback { get; set; }

        /// <summary>
        /// 是否连接状态
        /// </summary>
        public bool IsConnected => isConnected;

        public int SendBufferPoolNumber => _sendTokenManager.Count;

        public ChannelProviderType ChannelProviderState => _channelProviderState;

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
                DisposeSocketPool();
                SafeClose();
                _isDisposed = true;
            }
        }

        private void DisposeSocketPool()
        {
            _sendTokenManager.Clear();
            _sBufferManager?.Clear();
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="chunkBufferSize">发送块缓冲区大小</param>
        /// <param name="bufferNumber">缓冲发送数</param>
        public TcpClientProvider(int chunkBufferSize = 4096, int bufferNumber = 8)
            :base(chunkBufferSize)
        {
            _bufferNumber = bufferNumber;
        }

        #endregion

        #region public method
        /// <summary>
        /// 异步建立连接
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        public void Connect(int port, string ip)
        {
            try
            {
                if (!IsClose())
                {
                    Close();
                }

                isConnected = false;
                _channelProviderState = ChannelProviderType.Async;

                using (new LockWait(ref _lockParam))
                {
                    CreatedConnectToBindArgs(port,ip);
                }
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        /// <summary>
        /// 异步等待连接返回结果
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool ConnectTo(int port,string ip)
        {
            try
            {
                if (!IsClose())
                {
                    Close();
                }

                isConnected = false;
                _channelProviderState = ChannelProviderType.AsyncWait;

                using (new LockWait(ref _lockParam))
                {
                    CreatedConnectToBindArgs(port,ip);
                }
                _mReset.WaitOne(connectioTimeout);
                isConnected = socket.Connected;

                return isConnected;
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        /// <summary>
        /// 同步连接
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool ConnectSync(int port, string ip)
        {

            if (!IsClose())
            {
                Close();
            }

            isConnected = false;
            _channelProviderState = ChannelProviderType.Sync;
            var retry = 3;

            using (new LockWait(ref _lockParam))
            {
                CreatedConnectToBindArgs(port,ip);
            }
            while (retry > 0)
            {
                try
                {
                    --retry;
                    socket.Connect(ipEndPoint);
                    isConnected = true;
                    return true;
                }
                catch (Exception)
                {
                    Close();
                    if (retry <= 0) throw;
                    Thread.Sleep(1000);
                }
            }
            return false;
        }

        /// <summary>
        /// 根据偏移发送缓冲区数据
        /// </summary>
        /// <param name="sendSegment"></param>
        /// <param name="waiting"></param>
        public bool Send(SegmentOffset sendSegment, bool waiting = true)
        {
            try
            {
                if (IsClose())
                {
                    Close();
                    return false;
                }

                var segItems = _sBufferManager.BufferToSegments(sendSegment.Buffer, sendSegment.Offset, sendSegment.Size);
                var isWillEvent = true;

                foreach (var seg in segItems)
                {
                    var tArgs = GetSocketAsyncFromSendPool(waiting);
                    if (tArgs == null)
                    {
                        return false;
                    }
                    if (!_sBufferManager.WriteBuffer(tArgs, seg.Array, seg.Offset, seg.Count))
                    {
                        _sendTokenManager.Set(tArgs);

                        throw new Exception(string.Format("发送缓冲区溢出...buffer block max size:{0}", _sBufferManager.BlockSize));
                    }
                    if (tArgs.UserToken == null)
                        ((SocketToken) tArgs.UserToken).TokenSocket = socket;

                    if (IsClose())
                    {
                        Close();
                        return false;
                    }

                    isWillEvent &= socket.SendAsync(tArgs);
                    if (!isWillEvent)//can't trigger the io complated event to do
                    {
                        ProcessSentCallback(tArgs);
                    }

                    if (_sendTokenManager.Count < (_sendTokenManager.Capacity >> 2))
                        Thread.Sleep(2);
                }

                return isWillEvent;
            }
            catch (Exception)
            {
                Close();

                throw;
            }
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="filename"></param>
        public void SendFile(string filename)
        {
            socket.SendFile(filename);
        }

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="sendSegment"></param>
        /// <param name="receiveSegment"></param>
        /// <returns></returns>
        public int SendSync(SegmentOffset sendSegment,SegmentOffset receiveSegment)
        {
            if (_channelProviderState != ChannelProviderType.Sync)
            {
                throw new Exception("需要使用同步连接...ConnectSync");
            }

            var sent = socket.Send(sendSegment.Buffer, sendSegment.Offset, sendSegment.Size, SocketFlags.None);
            if (receiveSegment?.Buffer == null || receiveSegment.Size==0) return sent;

            socket.Receive(receiveSegment.Buffer, receiveSegment.Size, 0);

            return sent;
        }

        /// <summary>
        /// 同步接收数据
        /// </summary>
        /// <param name="receiveSegment"></param>
        /// <param name="receivedAction"></param>
        public void ReceiveSync(SegmentOffset receiveSegment, Action<SegmentOffset> receivedAction)
        {
            if (_channelProviderState != ChannelProviderType.Sync)
            {
                throw new Exception("需要使用同步连接...ConnectSync");
            }

            do
            {
                if (socket.Connected == false) break;

                var cnt = socket.Receive(receiveSegment.Buffer, receiveSegment.Size, 0);
                if (cnt <= 0) break;

                receivedAction(receiveSegment);

            } while (true);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            Close();
            isConnected = false;
        }

        #endregion

        #region private method

        private void  CreatedConnectToBindArgs(int port,string ip)
        {
            CreateTcpSocket(port,ip);

            //连接事件绑定
            var sArgs = new SocketAsyncEventArgs
            {
                RemoteEndPoint = ipEndPoint,
                UserToken = new SocketToken(-1) {TokenSocket = socket},
                AcceptSocket = socket
            };
            sArgs.Completed += IO_Completed;
            if (!socket.ConnectAsync(sArgs))
            {
                ProcessConnectCallback(sArgs);
            }
        }

        private void Close()
        {
            using (new LockWait(ref _lockParam))
            {
                DisposeSocketPool();
                SafeClose();
                isConnected = false;
            }
        }

        private bool IsClose()
        {
            return (IsConnected == false
                || socket == null
                || socket.Connected == false);
        }

        private SocketAsyncEventArgs GetSocketAsyncFromSendPool(bool waiting)
        {
            var tArgs = _sendTokenManager.GetEmptyWait((retry) => !IsClose(), waiting);

            if (IsConnected == false) return null;

            if (tArgs == null)
                throw new Exception("发送缓冲池已用完,等待回收超时...");

            return tArgs;
        }

        private void InitializePool(int maxNumberOfConnections)
        {
            _sendTokenManager?.Clear();
            _sBufferManager?.Clear();

            _sendTokenManager = new SocketTokenManager<SocketAsyncEventArgs>(maxNumberOfConnections);
            _sBufferManager = new SocketBufferManager(maxNumberOfConnections, receiveChunkSize);
          
            for (var i = 0; i < maxNumberOfConnections; ++i)
            {
                var tArgs = new SocketAsyncEventArgs() {
                    DisconnectReuseSocket=true
                };
                tArgs.Completed +=  IO_Completed;
                tArgs.UserToken = new SocketToken(i)
                {
                    TokenSocket = socket,
                    TokenId = i
                };
                _sBufferManager.SetBuffer(tArgs);
                _sendTokenManager.Set(tArgs);
            }
        }

        private void ProcessSentCallback(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    if (SentCallback != null)
                    {
                        if (e.UserToken is SocketToken sToken)
                        {
                            sToken.TokenIpEndPoint = (IPEndPoint) e.RemoteEndPoint;

                            SentCallback(sToken, e.Buffer, e.Offset, e.BytesTransferred);
                        }
                    }
                }
                else
                {
                    ProcessDisconnectAsync(e);
                }
            }
            finally
            {
                _sendTokenManager.Set(e);
            }
        }

        private void ProcessReceiveCallback(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0
                || e.SocketError != SocketError.Success
                || e.AcceptSocket.Connected == false)
            {
                Close();
                return;
            }

            if (e.UserToken is SocketToken sToken)
            {
                sToken.TokenIpEndPoint = (IPEndPoint) e.RemoteEndPoint;

                ReceiveOffsetCallback?.Invoke(sToken, e.Buffer, e.Offset, e.BytesTransferred);

                if (RecievedCallback != null)
                {
                    if (e.Offset == 0 && e.BytesTransferred == e.Buffer.Length)
                    {
                        RecievedCallback(sToken, e.Buffer);
                    }
                    else
                    {
                        var realBytes = new byte[e.BytesTransferred];

                        Buffer.BlockCopy(e.Buffer, e.Offset, realBytes, 0, e.BytesTransferred);
                        RecievedCallback(sToken, realBytes);
                    }
                }
            }

            if (!e.AcceptSocket.ReceiveAsync(e))
            {
                ProcessReceiveCallback(e);
            }
        }

        private void ProcessConnectCallback(SocketAsyncEventArgs e)
        {
            try
            {
                isConnected = (e.SocketError == SocketError.Success);
                if (isConnected)
                {
                    using (new LockWait(ref _lockParam))
                    {
                        InitializePool(_bufferNumber);
                    }
                    e.SetBuffer(receiveBuffer, 0, receiveChunkSize);
                    if (ConnectedCallback != null)
                    {
                        if (e.UserToken is SocketToken sToken)
                        {
                            sToken.TokenIpEndPoint = (IPEndPoint) e.RemoteEndPoint;
                            ConnectedCallback(sToken, isConnected);
                        }
                    }

                    if (!e.AcceptSocket.ReceiveAsync(e))
                    {
                        ProcessReceiveCallback(e);
                    }
                }
                else
                {
                    ProcessDisconnectAsync(e);
                }
                if (_channelProviderState == ChannelProviderType.AsyncWait)
                    _mReset.Set();
            }
            catch(Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name + ex.Message);
#endif
            }
        }

        private void ProcessDisconnectCallback(SocketAsyncEventArgs e)
        {
            try
            {
                isConnected = (e.SocketError == SocketError.Success);
                if (isConnected)
                {
                    Close();
                }

                if (DisconnectedCallback != null)
                {
                    if (e.UserToken is SocketToken sToken)
                    {
                        sToken.TokenIpEndPoint = (IPEndPoint) e.RemoteEndPoint;
                        DisconnectedCallback(sToken);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ProcessDisconnectAsync(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.AcceptSocket == null) return;

                var willRaiseEvent = e.AcceptSocket.DisconnectAsync(e);

                if (!willRaiseEvent)
                {
                    ProcessDisconnectCallback(e);
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name+ex.Message);
#endif
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    ProcessSentCallback(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceiveCallback(e);
                    break;
                case SocketAsyncOperation.Connect:
                    ProcessConnectCallback(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    ProcessDisconnectCallback(e);
                    break;
            }
        }
        #endregion
    }
}