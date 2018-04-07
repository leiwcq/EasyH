using EasyH.Net.Protocols;
using EasyH.Net.Providers.Interface;

namespace EasyH.Net.Providers.Instance
{
    public class NetProtocolProvider : INetProtocolProvider
    {
        public static NetProtocolProvider CreateProvider()
        {
            return new NetProtocolProvider();
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Packet Decode(byte[] buffer, int offset, int size)
        {
            var pkg = new Packet();
            return pkg.DeocdeFromBytes(buffer, offset, size) ? pkg : null;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="pkg"></param>
        /// <returns></returns>
        public byte[] Encode(Packet pkg)
        {
            return pkg.EncodeToBytes();
        }
    }
}
