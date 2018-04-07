using EasyH.Net.Protocols;

namespace EasyH.Net.Providers.Interface
{
    public interface INetProtocolProvider
    {
        Packet Decode(byte[] buffer, int offset, int size);

        byte[] Encode(Packet pkg);

    }
}
