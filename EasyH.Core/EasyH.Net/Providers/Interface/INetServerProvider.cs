using System;
using EasyH.Net.Base;
using EasyH.Net.Common;

namespace EasyH.Net.Providers.Interface
{
   public interface INetServerProvider:IDisposable
    {
        OnReceiveHandler ReceiveHandler { get; set; }

        OnSentHandler SentHandler { get; set; }

        OnAcceptHandler AcceptHandler { get; set; }

        OnReceiveOffsetHandler ReceiveOffsetHandler { get; set; }

        OnDisconnectedHandler DisconnectedHandler { get; set; }

        bool Start(int port, string ip = "0.0.0.0");

        bool Send(SegmentOffsetToken segToken,bool waiting =true);
 
        int SendSync(SegmentOffsetToken segToken);

        void Stop();

        void CloseToken(SocketToken sToken);
    }
}
