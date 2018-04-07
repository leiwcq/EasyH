/*-------------------------------------------------------------
 *project:Bouyei.NetFactory.Providers.Tcp
 *   auth: bouyei
 *   date: 2018/1/27 16:00:48
 *contact: 453840293@qq.com
 *profile: www.openthinking.cn
---------------------------------------------------------------*/

using System;
using System.Net;
using System.Net.Sockets;

namespace EasyH.Net.Providers.Tcp
{
    public class TcpSocket
    {
        protected Socket socket;
        protected bool isConnected = false;
        protected EndPoint ipEndPoint;

        protected byte[] receiveBuffer;
        protected int receiveTimeout = 1000 * 60 * 30;
        protected int sendTimeout = 1000 * 60 * 30;
        protected int connectioTimeout = 1000 * 60 * 30;
        protected int receiveChunkSize = 4096;

        public TcpSocket(int size)
        {
            receiveChunkSize = size;
            receiveBuffer = new byte[size];
        }

        protected void CreateTcpSocket(int port,string ip)
        {
            ipEndPoint =  new IPEndPoint(IPAddress.Parse(ip), port);
            Socket socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                LingerState = new LingerOption(true, 0),
                NoDelay = true,
                ReceiveTimeout = receiveTimeout,
                SendTimeout = sendTimeout
            };
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.socket = socket;
        }

        protected void SafeClose()
        {
            if (socket == null) return;

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch(Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name + ex.Message);
#endif
            }
            try
            {
                socket.Disconnect(false);
            }
            catch (Exception ex) {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name + ex.Message);
#endif
            }
            try
            {
                socket.Dispose();
            }
            catch(Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name + ex.Message);
#endif
            }
        }

        protected void SafeClose(Socket s)
        {
            if (s == null) return;

            try
            {
                s.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException ex)
            {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name + ex.Message);
#endif
                return;
            }
            catch(Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name + ex.Message);
#endif
            }
            try
            {
                s.Disconnect(false);
            }
            catch (Exception ex) {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name + ex.Message);
#endif
            }
            try
            {
                s.Dispose();
                s = null;
            }
            catch(Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.TargetSite.Name + ex.Message);
#endif
            }
        }
    }
}
