using System;
using System.Net;
using System.Net.Sockets;

namespace EasyH.Net.Providers.Udp
{
    public class UdpSocket
    {
        protected Socket Socket;
        protected bool IsConnected = false;
        protected EndPoint IpEndPoint;

        protected byte[] ReceiveBuffer;
        protected int ReceiveChunkSize = 4096;
        protected int ReceiveTimeout = 1000 * 60 * 30;
        protected int SendTimeout = 1000 * 60 * 30;

        public UdpSocket(int size)
        {
            ReceiveChunkSize = size;
            ReceiveBuffer = new byte[size];
        }
        protected void SafeClose()
        {
            if (Socket == null) return;

            try
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch
            {
                // ignored
            }

            try
            {
                Socket.Disconnect(false);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                Socket.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        protected void CreateUdpSocket(int port, string ip)
        {
            IpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Socket = new Socket(IpEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = ReceiveTimeout,
                SendTimeout = SendTimeout
            };
            Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        }
    }
}
