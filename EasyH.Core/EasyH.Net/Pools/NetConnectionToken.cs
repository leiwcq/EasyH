using System;
using EasyH.Net.Base;

namespace EasyH.Net.Pools
{
    public class NetConnectionToken:IComparable<NetConnectionToken>
    {

        public NetConnectionToken() { }

        public NetConnectionToken(SocketToken sToken)
        {
            Token = sToken;
            Verification = true;
            ConnectionTime = DateTime.Now;//兼容低版本语法
        }

        public SocketToken Token { get; set; }

        public DateTime ConnectionTime { get; set; }

        public bool Verification { get; set; }

        public int CompareTo(NetConnectionToken item)
        {
            return Token.CompareTo(item.Token);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NetConnectionToken nc)) return false;

            return CompareTo(nc) == 0;
        }

        public override int GetHashCode()
        {
            return Token.TokenId.GetHashCode()|Token.TokenSocket.GetHashCode();
        }
    }
}