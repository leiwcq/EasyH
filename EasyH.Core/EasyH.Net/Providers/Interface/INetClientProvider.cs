using System;
using EasyH.Net.Base;
using EasyH.Net.Common;

namespace EasyH.Net.Providers.Interface
{
    public interface INetClientProvider:IDisposable
    {
        bool IsConnected { get; }
        OnReceiveHandler ReceiveHandler { get; set; }

        OnSentHandler SentHandler { get; set; }

        OnReceiveOffsetHandler ReceiveOffsetHandler { get; set; }

        OnDisconnectedHandler DisconnectedHandler { get; set; }

        OnConnectedHandler ConnectedHandler { get; set; }

        ChannelProviderType ChannelProviderType { get; }
        int BufferPoolCount { get; }
        NetProviderType NetProviderType { get;}
        void Disconnect();

        void Connect(int port, string ip);

        bool ConnectTo(int port, string ip);

        bool Send(SegmentOffset dataSegment, bool waiting = true);

        bool ConnectSync(int port, string ip);

        void SendSync(SegmentOffset sendSegment,SegmentOffset receiveSegment);

        void ReceiveSync(SegmentOffset receiveSegment,Action<SegmentOffset> receiveAction);
    }
}
