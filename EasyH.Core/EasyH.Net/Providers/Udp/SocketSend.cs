using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EasyH.Net.Common;
using EasyH.Net.Pools;

namespace EasyH.Net.Providers.Udp
{
    internal class SocketSend : UdpSocket,IDisposable
    {
        #region variable
        private readonly int _maxCount;
 
        private readonly SocketTokenManager<SocketAsyncEventArgs> _sendTokenManager;
        private readonly SocketBufferManager _sendBufferManager;
        private bool _isDisposed;

        #endregion

        #region structure
        /// <summary>
        /// 发送事件回调
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> SentEventHandler;

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
                Socket.Dispose();
                _sendBufferManager.Clear();
                _isDisposed = true;
            }
        }
        #endregion

        #region public method 

        /// <summary>
        /// 初始化发送对象
        /// </summary>
        /// <param name="maxCountClient">客户端最大数</param>
        /// <param name="blockSize"></param>
        public SocketSend(int maxCountClient, int blockSize = 4096)
            : base(blockSize)
        {
            _maxCount = maxCountClient;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = ReceiveTimeout,
                SendTimeout = SendTimeout
            };

            _sendTokenManager = new SocketTokenManager<SocketAsyncEventArgs>(maxCountClient);
            _sendBufferManager = new SocketBufferManager(maxCountClient, blockSize);

            for (var i = 0; i < _maxCount; ++i)
            {
                var socketArgs = new SocketAsyncEventArgs
                {
                    UserToken = Socket
                };
                socketArgs.Completed += ClientSocket_Completed;
                _sendBufferManager.SetBuffer(socketArgs);
                _sendTokenManager.Set(socketArgs);
            }
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="dataSegment"></param>
        /// <param name="remoteEp"></param>
        /// <param name="waiting"></param>
        public bool Send(SegmentOffset dataSegment, IPEndPoint remoteEp,bool waiting)
        {
            var isWillEvent = true;

            var segItems = _sendBufferManager.BufferToSegments(dataSegment.Buffer, dataSegment.Offset, dataSegment.Size);
            foreach (var seg in segItems)
            {
                var tArgs = _sendTokenManager.GetEmptyWait(retry => true, waiting);

                if (tArgs == null)
                    throw new Exception("发送缓冲池已用完,等待回收超时...");

                tArgs.RemoteEndPoint = remoteEp;
                var s = SocketVersion(remoteEp);
                tArgs.UserToken = s;

                if (!_sendBufferManager.WriteBuffer(tArgs, seg.Array, seg.Offset, seg.Count))
                {
                    _sendTokenManager.Set(tArgs);

                    throw new Exception(string.Format("发送缓冲区溢出...buffer block max size:{0}", _sendBufferManager.BlockSize));
                }

                if (tArgs.RemoteEndPoint != null)
                {
                    isWillEvent &= s.SendToAsync(tArgs);
                    if (!isWillEvent)
                    {
                        ProcessSent(tArgs);
                    }
                }
                Thread.Sleep(5);
            }
            return isWillEvent;
        }

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="dataSegment"></param>
        /// <param name="remoteEp"></param>
        /// <returns></returns>
        public int SendSync(SegmentOffset dataSegment, IPEndPoint remoteEp)
        {
            return SocketVersion(remoteEp).SendTo(dataSegment.Buffer, dataSegment.Offset, dataSegment.Size,
                SocketFlags.None, remoteEp);
        }

        #endregion

        #region private method
        /// <summary>
        /// 释放缓冲池
        /// </summary>
        private void DisposeSocketPool()
        {
            _sendTokenManager.ClearToCloseArgs();
        }

        /// <summary>
        /// 获取socket版本
        /// </summary>
        /// <param name="ips"></param>
        /// <returns></returns>
        private Socket SocketVersion(EndPoint ips)
        {
            if (ips.AddressFamily == Socket.AddressFamily)
            {
                return Socket;
            }

            Socket = new Socket(ips.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            return Socket;
        }

        /// <summary>
        /// 处理发送的数据
        /// </summary>
        /// <param name="e"></param>
        private void ProcessSent(SocketAsyncEventArgs e)
        {
            _sendTokenManager.Set(e);

            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                SentEventHandler?.Invoke(e.UserToken as Socket, e);
            }
        }

        /// <summary>
        /// 完成发送事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClientSocket_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.SendTo:
                    ProcessSent(e);
                    break;
            }
        }
        #endregion
    }
}