using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EasyH.Net.Base;
using EasyH.Net.Common;
using EasyH.Net.Pools;

namespace EasyH.Net.Providers.Udp
{
    public class UdpClientProvider : UdpSocket,IDisposable
    {
        #region 定义变量
        private bool _isDisposed;
        private readonly int _bufferSizeByConnection = 4096;
        private readonly int _maxNumberOfConnections = 64;

        private LockParam _lParam = new LockParam();
        private readonly ManualResetEvent _mReset = new ManualResetEvent(false);
        private SocketTokenManager<SocketAsyncEventArgs> _sendTokenManager;
        private SocketBufferManager _sendBufferManager;

        #endregion

        #region 属性
        public int SendBufferPoolNumber => _sendTokenManager.Count;

        /// <summary>
        /// 接收回调处理
        /// </summary>
        public OnReceiveHandler ReceiveCallbackHandler { get; set; }

        /// <summary>
        /// 发送回调处理
        /// </summary>
        public OnSentHandler SentCallbackHandler { get; set; }
        /// <summary>
        /// 接收缓冲区回调
        /// </summary>
        public OnReceiveOffsetHandler ReceiveOffsetHandler { get; set; }
        #endregion

        #region public method
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
            _sendTokenManager.ClearToCloseArgs();
            _sendBufferManager?.Clear();
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        public UdpClientProvider(int bufferSizeByConnection, int maxNumberOfConnections)
            :base(bufferSizeByConnection)
        {
            _maxNumberOfConnections = maxNumberOfConnections;
            _bufferSizeByConnection = bufferSizeByConnection;
            Initialize();
        }

        public void Disconnect()
        {
            Close();
            IsConnected = false;
        }

        /// <summary>
        /// 尝试连接
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool Connect(int port, string ip)
        {
            Close();

            CreateUdpSocket(port, ip);

            var retry = 3;
            again:
            try
            {
                //探测是否有效连接
                var sArgs = new SocketAsyncEventArgs();
                sArgs.Completed += IO_Completed;
                sArgs.UserToken = Socket;
                sArgs.RemoteEndPoint =IpEndPoint;
                sArgs.SetBuffer(new byte[] { 0 }, 0, 1);

                var rt = Socket.SendToAsync(sArgs);
                if (rt)
                {
                    StartReceive();
                    _mReset.WaitOne();
                }
            }
            catch (Exception)
            {
                retry -= 1;
                if (retry > 0)
                {
                    Thread.Sleep(1000);
                    goto again;
                }
                throw;
            }
            return IsConnected;
        }

 
        public bool Send(SegmentOffset sendSegment, bool waiting = true)
        {
            try
            {
                var isWillEvent = true;
                var segItems = _sendBufferManager.BufferToSegments(sendSegment.Buffer, sendSegment.Offset, sendSegment.Size);
                foreach (var seg in segItems)
                {
                    var tArgs = _sendTokenManager.GetEmptyWait((retry) => true, waiting);

                    if (tArgs == null)
                        throw new Exception("发送缓冲池已用完,等待回收...");

                    tArgs.RemoteEndPoint = IpEndPoint;

                    if (!_sendBufferManager.WriteBuffer(tArgs, seg.Array, seg.Offset, seg.Count))
                    {
                        _sendTokenManager.Set(tArgs);

                        throw new Exception(string.Format("发送缓冲区溢出...buffer block max size:{0}", _sendBufferManager.BlockSize));
                    }

                    isWillEvent &= Socket.SendToAsync(tArgs);
                    if (!isWillEvent)
                    {
                        ProcessSent(tArgs);
                    }
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
        /// 同步发送
        /// </summary>
        /// <param name="sendSegment"></param>
        /// <param name="receiveSegment"></param>
        /// <returns></returns>
        public int SendSync(SegmentOffset sendSegment, SegmentOffset receiveSegment)
        {
            var sent = Socket.SendTo(sendSegment.Buffer, sendSegment.Offset, sendSegment.Size, 0, IpEndPoint);
            if (receiveSegment?.Buffer == null || receiveSegment.Size == 0) return sent;

            Socket.ReceiveFrom(receiveSegment.Buffer,
                receiveSegment.Size,
                SocketFlags.None,
                ref IpEndPoint);

            return sent;
        }

        /// <summary>
        /// 同步接收
        /// </summary>
        /// <param name="receiveSegment"></param>
        /// <param name="receiveAction"></param>
        public void ReceiveSync(SegmentOffset receiveSegment, Action<SegmentOffset> receiveAction)
        {
            do
            {
                var cnt = Socket.ReceiveFrom(receiveSegment.Buffer,
                    receiveSegment.Size,
                    SocketFlags.None,
                    ref IpEndPoint);

                if (cnt <= 0) break;

                receiveAction(receiveSegment);
            } while (true);
        }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        public void StartReceive()
        {
            using (new LockWait(ref _lParam))
            {
                var sArgs = new SocketAsyncEventArgs();
                sArgs.Completed += IO_Completed;
                sArgs.UserToken = Socket;
                sArgs.RemoteEndPoint = IpEndPoint;
                sArgs.SetBuffer(ReceiveBuffer, 0, _bufferSizeByConnection);
                if (!Socket.ReceiveFromAsync(sArgs))
                {
                    ProcessReceive(sArgs);
                }
            }
        }

        #endregion

        #region private method
        /// <summary>
        /// 初始化对象
        /// </summary>
        private void Initialize()
        {
            _sendTokenManager = new SocketTokenManager<SocketAsyncEventArgs>(_maxNumberOfConnections);
            _sendBufferManager = new SocketBufferManager(_maxNumberOfConnections, _bufferSizeByConnection);

            //初始化发送接收对象池
            for (var i = 0; i < _maxNumberOfConnections; ++i)
            {
                var sendArgs = new SocketAsyncEventArgs();
                sendArgs.Completed += IO_Completed;
                sendArgs.UserToken = Socket;
                _sendBufferManager.SetBuffer(sendArgs);
                _sendTokenManager.Set(sendArgs);
            }
        }

        private void Close()
        {
            if (Socket != null)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
                Socket.Dispose();
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            using (var lwait = new LockWait(ref _lParam))
            {
                var sToken = new SocketToken()
                {
                    TokenSocket = e.UserToken as Socket,
                    TokenIpEndPoint = (IPEndPoint)e.RemoteEndPoint
                };

                try
                {
                    if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
                        return;

                    //初次连接心跳
                    if (IsServerResponse(e) == false)
                        return;

                    //缓冲区偏移量返回
                    ReceiveOffsetHandler?.Invoke(sToken, e.Buffer, e.Offset, e.BytesTransferred);

                    //截取后返回
                    if (ReceiveCallbackHandler != null)
                    {
                        if (e.Offset == 0 && e.BytesTransferred == e.Buffer.Length)
                        {
                            ReceiveCallbackHandler(sToken, e.Buffer);
                        }
                        else
                        {
                            var realbytes = new byte[e.BytesTransferred];
                            Buffer.BlockCopy(e.Buffer, e.Offset, realbytes, 0, e.BytesTransferred);

                            ReceiveCallbackHandler(sToken, realbytes);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        //继续下一个接收
                        if (!sToken.TokenSocket.ReceiveFromAsync(e))
                        {
                            ProcessReceive(e);
                        }
                    }
                }
            }
        }

        private void ProcessSent(SocketAsyncEventArgs e)
        {
            try
            {
                IsConnected = e.SocketError == SocketError.Success;

                if (SentCallbackHandler != null && isClientRequest(e) == false)
                {
                    var sToken = new SocketToken()
                    {
                        TokenSocket = e.UserToken as Socket,
                        TokenIpEndPoint = (IPEndPoint)e.RemoteEndPoint
                    };
                    SentCallbackHandler(sToken, e.Buffer, e.Offset, e.BytesTransferred);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sendTokenManager.Set(e);
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.SendTo:
                    ProcessSent(e);
                    break;
            }
        }

        private bool IsServerResponse(SocketAsyncEventArgs e)
        {
            IsConnected = e.SocketError == SocketError.Success;

            if (e.BytesTransferred == 1 && e.Buffer[0] == 1)
            {
                _mReset.Set();
                return true;
            }

            return false;
        }

        private bool isClientRequest(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 1 && e.Buffer[0] == 0)
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}