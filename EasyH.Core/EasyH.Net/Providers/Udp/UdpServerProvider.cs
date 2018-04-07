using System;
using System.Net;
using System.Net.Sockets;
using EasyH.Net.Base;
using EasyH.Net.Common;

namespace EasyH.Net.Providers.Udp
{
    public class UdpServerProvider : IDisposable
    {
        #region variable
        private SocketReceive _socketRecieve;
        private SocketSend _socketSend;
        private bool _isDisposed;

        #endregion

        #region property

        public OnReceiveOffsetHandler ReceiveOffsetHanlder { get; set; }

        /// <summary>
        /// 接收事件响应回调
        /// </summary>
        public OnReceiveHandler ReceiveCallbackHandler { get; set; }

        /// <summary>
        /// 发送事件响应回调
        /// </summary>
        public OnSentHandler SentCallbackHandler { get; set; }

        /// <summary>
        /// 断开连接事件回调
        /// </summary>
        public OnDisconnectedHandler DisconnectedCallbackHandler { get; set; }

        #endregion

        #region structure
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
                _socketRecieve.Dispose();
                _socketSend.Dispose();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="port">接收数据端口</param>
        /// <param name="recBufferSize">接收缓冲区</param>
        /// <param name="maxConnectionCount">最大客户端连接数</param>
        public void Start(int port,
            int recBufferSize,
            int maxConnectionCount)
        {
            _socketSend = new SocketSend(maxConnectionCount, recBufferSize);
            _socketSend.SentEventHandler += sendSocket_SentEventHandler;

            _socketRecieve = new SocketReceive(port,maxConnectionCount, recBufferSize);
            _socketRecieve.OnReceived += receiveSocket_OnReceived;
            _socketRecieve.StartReceive();
        }
        #endregion

        #region public method
        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            _socketSend?.Dispose();
            _socketRecieve?.StopReceive();
        }
 
        public bool Send(SegmentOffset dataSegment,IPEndPoint remoteEp ,bool waiting = true)
        {
            return _socketSend.Send(dataSegment, remoteEp, waiting);
        }
 
        public int SendSync(IPEndPoint remoteEp, SegmentOffset dataSegment)
        {
            return _socketSend.SendSync(dataSegment , remoteEp);
        }
        #endregion

        #region private method
        private void sendSocket_SentEventHandler(object sender, SocketAsyncEventArgs e)
        {
            if (SentCallbackHandler != null && IsServerResponse(e) == false)
            {
                SentCallbackHandler(new SocketToken()
                {
                    TokenIpEndPoint = (IPEndPoint)e.RemoteEndPoint
                }, e.Buffer, e.Offset, e.BytesTransferred);
            }
        }

        private void receiveSocket_OnReceived(object sender, SocketAsyncEventArgs e)
        {
            if (IsClientRequest(e)) return;

            SocketToken sToken = new SocketToken()
            {
                TokenIpEndPoint = (IPEndPoint)e.RemoteEndPoint
            };

            ReceiveOffsetHanlder?.Invoke(sToken, e.Buffer, e.Offset, e.BytesTransferred);

            if (ReceiveCallbackHandler != null)
            {
                if (e.BytesTransferred > 0)
                {
                    if (e.Offset == 0 && e.BytesTransferred == e.Buffer.Length)
                    {
                        ReceiveCallbackHandler(sToken, e.Buffer);
                    }
                    else
                    {
                        byte[] realBytes = new byte[e.BytesTransferred];
                        Buffer.BlockCopy(e.Buffer, e.Offset, realBytes, 0, e.BytesTransferred);
                        ReceiveCallbackHandler(sToken, realBytes);
                    }
                }
            }
        }

        private bool IsClientRequest(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 1 && e.Buffer[0] == 0)
            {
                _socketSend.Send(new SegmentOffset(new byte[] { 1 }),
                  (IPEndPoint)e.RemoteEndPoint, true);
                return true;
            }

            return false;
        }

        private bool IsServerResponse(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 1 && e.Buffer[0] == 1)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}