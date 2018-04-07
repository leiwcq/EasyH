using System.Collections.Generic;
using EasyH.Net.Base;
using EasyH.Net.Protocols;
using EasyH.Net.Providers.Interface;

namespace EasyH.Net.Providers.Instance
{
    public  class NetPacketProvider:INetPacketProvider
    {
        private readonly PacketQueue _packetQueue;
        private LockParam _lockParam;

        public NetPacketProvider(int capacity)
        {
            if (capacity < 128) capacity = 128;
            capacity += 1;
            _packetQueue = new PacketQueue(capacity);
            _lockParam = new LockParam();
        }

        public static NetPacketProvider CreateProvider(int capacity)
        {
            return new NetPacketProvider(capacity);
        }

        public int Count => _packetQueue.Count;

        public bool SetBlocks(byte[] bufffer,int offset,int size)
        {
            using (new LockWait(ref _lockParam))
            {
                return _packetQueue.SetBlock(bufffer, offset, size);
            }
        }

        public List<Packet> GetBlocks()
        {
            using (new LockWait(ref _lockParam))
            {
                return _packetQueue.GetBlocks();
            }
        }
    }
}
