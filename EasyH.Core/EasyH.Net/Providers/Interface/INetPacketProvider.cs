using System.Collections.Generic;
using EasyH.Net.Protocols;

namespace EasyH.Net.Providers.Interface
{
    public interface INetPacketProvider
    {
        int Count { get; }
        bool SetBlocks(byte[] buffer, int offset, int size);

        List<Packet> GetBlocks();
    }
}
