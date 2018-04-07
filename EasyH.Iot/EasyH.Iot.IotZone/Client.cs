using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyH.Iot.IotZone
{
    public class Client
    {
        private readonly TcpClient _tcpClient;
        private NetworkStream _networkStream;

        private readonly Byte[] _sendBuf = new Byte[1024];          //写入Buf
        private readonly Byte[] _readBuf = new Byte[1024];          //读出Buf
        private Int32 _readBytes;                                    //读出字节数
        private readonly Byte[] _stateRelay = new Byte[10];

        public Client()
        {
            _tcpClient = new TcpClient();
        }

        public async Task Connect(string hostname, int port)
        {
            await _tcpClient.ConnectAsync(hostname, port);
            _networkStream = _tcpClient.GetStream();
        }

        public bool Connected => _tcpClient.Connected;

        public void ReadState()
        {
            _sendBuf[0] = 0x64; _sendBuf[1] = 0x75; _sendBuf[2] = 0x6D; _sendBuf[3] = 0x70;//dump ASCII 码
            _networkStream.Write(_sendBuf, 0, 4);
            _readBytes = _networkStream.Read(_readBuf, 0, _readBuf.Length);

            if (_readBytes < 61)
                return;
            int m = 0;
            for (int k = 0; k < 16; k++)
            {
                while (m < 200)
                {
                    if (_readBuf[m] == 0x0D && _readBuf[m + 1] == 0x0A)
                    {
                        if (k < 10) //处理继电器状态
                        {
                            if (_readBuf[m + 8] == 0x66) { _stateRelay[k] = 0; } //f ASCII 0x66 n ASCII 0x6e
                            if (_readBuf[m + 8] == 0x6e) { _stateRelay[k] = 1; } //Off 状态 0 On状态 1
                        }
                        else
                        {
                            if (_readBuf[m + 3] == 0x4c) { _stateRelay[k - 10] = 0; } //L ASCII 0x4c H ASCII 0x48
                            if (_readBuf[m + 3] == 0x48) { _stateRelay[k - 10] = 1; }
                        }
                        m++;
                        break;
                    }
                    m++;
                }
            }
        }
    }
}