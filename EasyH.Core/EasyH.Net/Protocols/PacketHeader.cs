using System;

namespace EasyH.Net.Protocols
{
    public class PacketHeader
    {
        /// <summary>
        /// 包类型标识(自定义类型值)
        /// </summary>
        public ushort PacketId { get; set; }
        /// <summary>
        /// 包类型(扩展保留)
        /// </summary>
        public byte PacketType { get; set; }
        /// <summary>
        /// 包属性
        /// </summary>
        public PacketAttribute PacketAttribute { get; set; }

    }

}
