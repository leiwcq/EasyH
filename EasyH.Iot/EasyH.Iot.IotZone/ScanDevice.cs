using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyH.Iot.IotZone
{
    public class ScanDevice
    {
        public void Scan()
        {
            var receiveSocket =
                new UdpClient(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //初始化一个Scoket协议

            var receiveIep = new IPEndPoint(IPAddress.Any, 9127); //初始化一个侦听局域网内部所有IP和指定端口

            EndPoint receiveEp = receiveIep;

            receiveSocket.Bind(receiveIep); //绑定这个实例

            while (true)
            {
                var receiveBuffer = new byte[1024]; //设置缓冲数据流

                receiveSocket.ReceiveFrom(receiveBuffer, ref receiveEp); //接收数据,并确把数据设置到缓冲流里面

                Console.WriteLine(Encoding.Unicode.GetString(receiveBuffer).TrimEnd('\0') + " " +
                                  DateTime.Now.ToString(CultureInfo.InvariantCulture));
                break;
            }

            UdpClient client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            IPEndPoint endpoint =
                new IPEndPoint(IPAddress.Parse("255.255.255.255"), 7788); //默认向全世界所有主机发送即可，路由器自动给你过滤，只发给局域网主机
            String ip = "host:" + Dns.GetHostEntry(Dns.GetHostName()).AddressList.Last().ToString(); //对外广播本机的ip地址
            byte[] ipByte = Encoding.UTF8.GetBytes(ip);
            DispatcherTimer dt = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            }; //每隔1秒对外发送一次广播
            dt.Tick += delegate { client.Send(ipByte, ipByte.Length, endpoint); };
            dt.Start();
        }
    }
}
