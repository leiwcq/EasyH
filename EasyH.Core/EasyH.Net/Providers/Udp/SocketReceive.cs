using System;
using System.Net.Sockets;
using EasyH.Net.Base;

namespace EasyH.Net.Providers.Udp
{
    internal class SocketReceive :UdpSocket, IDisposable
    {
        #region variable
        private readonly SocketAsyncEventArgs _recArgs;
        private LockParam _lParam = new LockParam();
        private bool _isStoped;
        private bool _isDisposed;

        /// <summary>
        /// 接收事件
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> OnReceived;

        #endregion

        #region structure
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="port">本机接收数据端口</param>
        /// <param name="bufferSize">接收缓冲区大小</param>
        public SocketReceive(int port,int maxNumberOfConnections, int bufferSize = 4096)
            :base(bufferSize)
        {
            CreateUdpSocket(port, "0.0.0.0");
            Socket.Bind(IpEndPoint);

            using (new LockWait(ref _lParam))
            {
                _recArgs = new SocketAsyncEventArgs
                {
                    UserToken = Socket,
                    RemoteEndPoint = Socket.LocalEndPoint
                };

                _recArgs.Completed += SocketArgs_Completed;
                _recArgs.SetBuffer(ReceiveBuffer, 0, ReceiveChunkSize);
            }
        }

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
                _isStoped = true;
                _isDisposed = true;
                Socket.Dispose();
                _recArgs.Dispose();
            }
        }
        #endregion

        #region public
 

        /// <summary>
        /// 开始接收数据
        /// </summary>
        public void StartReceive()
        {
            using (new LockWait(ref _lParam))
            {
                var rt =Socket.ReceiveFromAsync(_recArgs);
                if (rt == false)
                {
                    ProcessReceive(_recArgs);
                }
            }
        }

        /// <summary>
        /// 停止接收数据
        /// </summary>
        public void StopReceive()
        {
            using (new LockWait(ref _lParam))
            {
                _isStoped = true;
                Socket.Dispose();
                _recArgs?.Dispose();
            }
        }
        #endregion

        #region private

        /// <summary>
        /// 接收完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    ProcessReceive(e);
                    break;
            }
        }

        /// <summary>
        /// 处理接收信息
        /// </summary>
        /// <param name="arg"></param>
        private void ProcessReceive(SocketAsyncEventArgs arg)
        {
            // receivePool.Set(args);
            if (arg.BytesTransferred > 0
                && arg.SocketError == SocketError.Success)
            {
                OnReceived?.Invoke(arg.UserToken as Socket, arg);
            }

            if (_isStoped) return;

            StartReceive();
        }

        #endregion
    }
}