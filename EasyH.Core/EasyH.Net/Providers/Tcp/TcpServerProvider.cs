using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EasyH.Net.Base;
using EasyH.Net.Common;
using EasyH.Net.Pools;

namespace EasyH.Net.Providers.Tcp
{
    public class TcpServerProvider : TcpSocket,IDisposable
    {
        #region variable
        private bool _isStoped = true;
        private bool _isDisposed;
        private int _numberOfConnections;
        private readonly int _maxNumberOfConnections = 32;
 
        private LockParam _lParam = new LockParam();
        private readonly Semaphore _maxNumberAcceptedClients;
        private readonly SocketTokenManager<SocketAsyncEventArgs> _sendTokenManager;
        private readonly SocketTokenManager<SocketAsyncEventArgs> _acceptTokenManager;
        private readonly SocketBufferManager _recvBufferManager;
        private readonly SocketBufferManager _sendBufferManager;

        #endregion

        #region properties
        /// <summary>
        /// 接受连接回调处理
        /// </summary>
        public OnAcceptHandler AcceptedCallback { get; set; }

        /// <summary>
        /// 接收数据回调处理
        /// </summary>
        public OnReceiveHandler ReceivedCallback { get; set; }

        /// <summary>
        ///接收数据缓冲区，返回缓冲区的实际偏移和数量
        /// </summary>
        public OnReceiveOffsetHandler ReceiveOffsetCallback { get; set; }

        /// <summary>
        /// 发送回调处理
        /// </summary>
        public OnSentHandler SentCallback { get; set; }

        /// <summary>
        /// 断开连接回调处理
        /// </summary>
        public OnDisconnectedHandler DisconnectedCallback { get; set; }

        /// <summary>
        /// 连接数
        /// </summary>
        public int NumberOfConnections => _numberOfConnections;

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
                _recvBufferManager.Clear();
                _sendBufferManager.Clear();
                _isDisposed = true;
                _maxNumberAcceptedClients.Dispose();
            }
        }

        private void DisposeSocketPool()
        {
            _sendTokenManager.ClearToCloseArgs();
            _acceptTokenManager.ClearToCloseArgs();
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="maxConnections">最大连接数</param>
        /// <param name="chunkBufferSize">接收块缓冲区</param>
        public TcpServerProvider(int maxConnections = 32, int chunkBufferSize = 4096)
            :base(chunkBufferSize)
        {
            if (maxConnections < 2) maxConnections = 2;
            _maxNumberOfConnections = maxConnections;

            _maxNumberAcceptedClients = new Semaphore(maxConnections, maxConnections);

            _recvBufferManager = new SocketBufferManager(maxConnections, chunkBufferSize);
            _acceptTokenManager = new SocketTokenManager<SocketAsyncEventArgs>(maxConnections);

            _sendTokenManager = new SocketTokenManager<SocketAsyncEventArgs>(maxConnections);
            _sendBufferManager = new SocketBufferManager(maxConnections, chunkBufferSize);
        }

        #endregion

        #region public method

        public bool Start(int port, string ip = "0.0.0.0")
        {
            var errorCount = 0;
            Stop();
            InitializeAcceptPool();
            InitializeSendPool();

            reStart:
            try
            {
                SafeClose();

                using (new LockWait(ref _lParam))
                {
                    CreateTcpSocket(port,ip);
 
                    socket.Bind(ipEndPoint);

                    socket.Listen(128);

                    _isStoped = false;
                }

                StartAccept(null);
                return true;
            }
            catch (Exception)
            {
                SafeClose();
                ++errorCount;

                if (errorCount >= 3)
                {
                    throw;
                }

                Thread.Sleep(1000);
                goto reStart;
            }
        }

        public void Stop()
        {
            try
            {
                using (new LockWait(ref _lParam))
                {
                    DisposePoolToken();

                    if (_numberOfConnections > 0)
                    {
                        _maxNumberAcceptedClients?.Release(_numberOfConnections);

                        _numberOfConnections = 0;
                    }
                    SafeClose();
                    _isStoped = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Close(SocketToken sToken)
        {
            ProcessAsyncDisconnect(sToken);
        }

        public bool Send(SegmentOffsetToken segToken, bool waiting = true)
        {
            try
            {
                if (!segToken.SToken.TokenSocket.Connected) return false;
               
                var isWillEvent = true;

                var segItems = _sendBufferManager.BufferToSegments(
                    segToken.DataSegment.Buffer,
                    segToken.DataSegment.Offset,
                    segToken.DataSegment.Size);

                foreach (var seg in segItems)
                {
                    var tArgs = GetSocketAsyncFromSendPool(waiting, segToken.SToken.TokenSocket);
                    if (tArgs == null) return false;

                    tArgs.UserToken = segToken.SToken;
                    if (!_sendBufferManager.WriteBuffer(tArgs, seg.Array, seg.Offset, seg.Count))
                    {
                        _sendTokenManager.Set(tArgs);

                        throw new Exception(string.Format("发送缓冲区溢出...buffer block max size:{0}", _sendBufferManager.BlockSize));
                    }

                    if (!segToken.SToken.TokenSocket.Connected) return false;

                    isWillEvent &= segToken.SToken.SendAsync(tArgs);
                    if (!isWillEvent)
                    {
                        ProcessSentCallback(tArgs);
                    }

                    if (_sendTokenManager.Count < (_sendTokenManager.Capacity >> 2))
                        Thread.Sleep(5);
                }
                return isWillEvent;
            }
            catch (Exception)
            {
                Close(segToken.SToken);

                throw;
            }
        }

        public int SendSync(SegmentOffsetToken segToken)
        {
            return segToken.SToken.Send(segToken.DataSegment);
        }

        #endregion

        #region private method

        private void DisposePoolToken()
        {
            _sendTokenManager.ClearToCloseArgs();
            _acceptTokenManager.ClearToCloseArgs();
        }

        private void InitializeAcceptPool()
        {
            _acceptTokenManager.Clear();
            for (var i = 0; i < _maxNumberOfConnections; ++i)
            {
                var args = new SocketAsyncEventArgs
                {
                    DisconnectReuseSocket=true,
                    SocketError=SocketError.SocketError
                };
                args.Completed += IO_Completed;
                args.UserToken = new SocketToken(i)
                {
                    TokenAgrs = args,
                };
                _recvBufferManager.SetBuffer(args);
                _acceptTokenManager.Set(args);
            }
        }

        private void InitializeSendPool()
        {
            _sendTokenManager.Clear();
            for (var i = 0; i < _maxNumberOfConnections; ++i)
            {
                var args = new SocketAsyncEventArgs
                {
                    DisconnectReuseSocket=true,
                    SocketError=SocketError.NotInitialized
                };
                args.Completed += IO_Completed;
                args.UserToken = new SocketToken(i);
                _sendBufferManager.SetBuffer(args);
                _sendTokenManager.Set(args);
            }
        }

        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (_isStoped || socket == null)
            {
                _isStoped = true;
                return;
            }
            if (e == null)
            {
                e = new SocketAsyncEventArgs
                {
                    DisconnectReuseSocket = true,
                    UserToken = new SocketToken(-255)
                };
                e.Completed += Accept_Completed;
            }
            else
            {
                e.AcceptSocket = null;
            }
            _maxNumberAcceptedClients.WaitOne();

            if (!socket.AcceptAsync(e))
            {
                ProcessAcceptCallback(e);
            }
        }

        private void ProcessAcceptCallback(SocketAsyncEventArgs e)
        {
            if (_isStoped 
                || _maxNumberOfConnections <= _numberOfConnections
                ||e.SocketError!=SocketError.Success)
            {
                DisposeSocketArgs(e);
                return;
            }

            //从对象池中取出一个对象
            var tArgs = _acceptTokenManager.GetEmptyWait((retry) => true);
            if (tArgs == null)
            {
                DisposeSocketArgs(e);
                return;
                //throw new Exception(string.Format("已经达到最大连接数max:{0};used:{1}",
                //    maxNumberOfConnections, numberOfConnections));
            }

            Interlocked.Increment(ref _numberOfConnections);

            var sToken = ((SocketToken)tArgs.UserToken);
            sToken.TokenSocket = e.AcceptSocket;
            sToken.TokenSocket.ReceiveTimeout = receiveTimeout;
            sToken.TokenSocket.SendTimeout = sendTimeout;
            sToken.TokenIpEndPoint = (IPEndPoint)e.AcceptSocket.RemoteEndPoint;
            sToken.TokenAgrs = tArgs;
            tArgs.UserToken = sToken;
             
            //listening receive 
            if (e.AcceptSocket.Connected)
            {
                if (!e.AcceptSocket.ReceiveAsync(tArgs))
                {
                    ProcessReceiveCallback(tArgs);
                }

                //将信息传递到自定义的方法
                AcceptedCallback?.Invoke(sToken);
            }
            else{
                ProcessDisconnectCallback(tArgs);
            }

            if (_isStoped) return;
            
            //继续准备下一个接收
            StartAccept(e);
        }

        private void ProcessReceiveCallback(SocketAsyncEventArgs e)
        {
            if ((e.BytesTransferred > 0
                && e.SocketError == SocketError.Success) == false)
            {
                ProcessDisconnectCallback(e);
                return;
            }

            var sToken = e.UserToken as SocketToken;

            ReceiveOffsetCallback?.Invoke(sToken, e.Buffer, e.Offset, e.BytesTransferred);

            //处理接收到的数据
            if (ReceivedCallback != null)
            {
                if (e.Offset == 0 && e.BytesTransferred == e.Buffer.Length)
                {
                    ReceivedCallback(sToken, e.Buffer);
                }
                else
                {
                    var realBytes = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(e.Buffer, e.Offset, realBytes, 0, e.BytesTransferred);
                    ReceivedCallback(sToken, realBytes);
                }
            }
            //继续投递下一个接受请求
            if (sToken != null && !sToken.TokenSocket.ReceiveAsync(e))
            {
                ProcessReceiveCallback(e);
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
                        var sToken = e.UserToken as SocketToken;
                        SentCallback(sToken, e.Buffer, e.Offset, e.BytesTransferred);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _sendTokenManager.Set(e);
            }
        }

        private void ProcessDisconnectCallback(SocketAsyncEventArgs e)
        {
            if (!(e.UserToken is SocketToken sToken)) {
                throw new Exception("关闭的对象为空");
            }

            sToken.Close();

            Interlocked.Decrement(ref _numberOfConnections);
            //递减信号量
            _maxNumberAcceptedClients.Release();

            //将断开的对象重新放回复用队列
            _acceptTokenManager.Set(e);

            DisconnectedCallback?.Invoke(sToken);
        }

        private void DisposeSocketArgs(SocketAsyncEventArgs e)
        {
            if (e.UserToken is SocketToken s) s.Close();// if (e.UserToken is SocketToken s) --新语法
            e.Dispose();
        }

        private void CloseSocket(Socket s)
        {
            using (new LockWait(ref _lParam))
            {
                SafeClose(s);
            }
        }

        private SocketAsyncEventArgs GetSocketAsyncFromSendPool(bool waiting, Socket socket)
        {
            var tArgs = _sendTokenManager.GetEmptyWait(retry =>
            {
                if (socket.Connected == false) return false;
                return true;
            }, waiting);

            if (socket.Connected == false)
                return null;

            if (tArgs == null)
                throw new Exception("发送缓冲池已用完,等待回收超时...");

            return tArgs;
        }

        //slow close client socket
        private void ProcessAsyncDisconnect(SocketToken sToken)
        {
            try
            {
                if (sToken?.TokenSocket == null || sToken.TokenAgrs == null) return;

                if (sToken.TokenSocket.Connected)
                    sToken.TokenSocket.Shutdown(SocketShutdown.Send);
 
                var args = new SocketAsyncEventArgs
                {
                    DisconnectReuseSocket = true,
                    SocketError = SocketError.SocketError,
                    UserToken = null
                };
                args.Completed += Accept_Completed;
                if (sToken.TokenSocket.DisconnectAsync(args) == false)
                {
                    ProcessDisconnectCallback(sToken.TokenAgrs);
                }
            }
            catch (ObjectDisposedException oe)
            {
#if DEBUG
                Console.WriteLine(oe.TargetSite.Name + oe.Message);
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name + ex.Message);
#endif
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceiveCallback(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSentCallback(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    ProcessDisconnectCallback(e);
                    break;
                case SocketAsyncOperation.Accept:
                    ProcessAcceptCallback(e);
                    break;
            }
        }

        void Accept_Completed(object send,SocketAsyncEventArgs e)
        {
            ProcessAcceptCallback(e);
        }
        #endregion
    }
}