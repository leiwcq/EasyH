using System;

namespace EasyH.Net.Protocols
{
    public class PacketAttribute
    {
        /// <summary>
        /// 数据内容包总数
        /// </summary>
        public ushort PacketCount { get; set; } = 1;
        /// <summary>
        /// 数据内容长度
        /// </summary>
        public uint PayloadLength { get;internal set; }
    }
}
