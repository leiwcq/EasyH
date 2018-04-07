using System;

namespace EasyH.Net.Protocols
{
    public class Packet
    {
        #region variable
        /// <summary>
        /// 包标志位
        /// </summary>
        public static byte PackageFlag { get; }= 0xfe;

        public static byte SubFlag { get; } = 0xfd;

        /// <summary>
        /// 报头信息
        /// </summary>
        public PacketHeader PacketHeader { get; set; }
        /// <summary>
        /// 包携带的数据内容
        /// </summary>
        public byte[] PacketPayload { get; set; }

        #endregion

        #region method
        /// <summary>
        /// 编码
        /// </summary>
        /// <returns></returns>
        internal byte[] EncodeToBytes()
        {
            var plen = PacketPayload.Length;
            PacketHeader.PacketAttribute.PayloadLength = (UInt32)plen;

            var buffer = new byte[11 + plen];
            buffer[0] = PackageFlag;
            buffer[1] = (byte)(PacketHeader.PacketId >> 8);
            buffer[2] = (byte)PacketHeader.PacketId;
            buffer[3] = PacketHeader.PacketType;
            buffer[4] = (byte)(PacketHeader.PacketAttribute.PacketCount >> 8);
            buffer[5] = (byte)PacketHeader.PacketAttribute.PacketCount;
            buffer[6] = (byte)(PacketHeader.PacketAttribute.PayloadLength >> 24);
            buffer[7] = (byte)(PacketHeader.PacketAttribute.PayloadLength >> 16);
            buffer[8] = (byte)(PacketHeader.PacketAttribute.PayloadLength >> 8);
            buffer[9] = (byte)(PacketHeader.PacketAttribute.PayloadLength);

            Buffer.BlockCopy(PacketPayload, 0, buffer, 10, plen);
            buffer[buffer.Length - 1] = PackageFlag;

            return Escape(buffer);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        internal bool DeocdeFromBytes(byte[] buffer, int offset, int size)
        {
            //还原转义并且过滤标志位
            var dst = Restore(buffer, offset, size);

            var plen = dst.ToUInt32(5);

            if (plen >= size - 9)
                return false;

            if (PacketHeader == null)
                PacketHeader = new PacketHeader();

            PacketHeader.PacketId = dst.ToUInt16();
            PacketHeader.PacketType = dst[2];

            if (PacketHeader.PacketAttribute == null)
                PacketHeader.PacketAttribute = new PacketAttribute();

            PacketHeader.PacketAttribute.PacketCount = dst.ToUInt16(3);
            PacketHeader.PacketAttribute.PayloadLength = plen;// dst.ToUInt32(5);

            PacketPayload = new byte[PacketHeader.PacketAttribute.PayloadLength];
            Buffer.BlockCopy(dst, 9, PacketPayload, 0, PacketPayload.Length);

            return true;
        }

        /// <summary>
        /// 转义
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private unsafe byte[] Escape(byte[] buffer)
        {
            var tCnt = CheckEscapeFlagBitCount(buffer);
            if ((tCnt.Item1 + tCnt.Item2) == 0) return buffer;

            var plen = buffer.Length - 2;

            var rBuffer = new byte[buffer.Length + tCnt.Item1 + tCnt.Item2];
            rBuffer[0] = buffer[0];//起始标识位

            fixed (byte* dst = &(rBuffer[1]), src = &(buffer[1]))
            {
                var tempDst = dst;
                var tempSrc = src;

                //消息头和消息体
                while (plen > 0)
                {
                    if (*tempSrc == PackageFlag)
                    {
                        *tempDst = SubFlag;
                        *(tempDst + 1) = 0x01;
                        tempDst += 2;
                    }
                    else if (*tempSrc == SubFlag)
                    {
                        *tempDst = SubFlag;
                        *(tempDst + 1) = 0x02;
                        tempDst += 2;
                    }
                    else
                    {
                        *tempDst = *tempSrc;
                        tempDst += 1;
                    }
                    tempSrc += 1;
                    plen -= 1;
                }

                //结束标志位
                *tempDst = *tempSrc;

                return rBuffer;
            }
        }

        /// <summary>
        /// 转义还原
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private unsafe byte[] Restore(byte[] buffer, int offset, int size)
        {
            var tCnt = CheckRestoreFlagBitCount(buffer, offset, size);
            if ((tCnt.Item1 + tCnt.Item2) == 0)
            {

                var buff = new byte[size - 2];
                Buffer.BlockCopy(buffer, offset + 1, buff, 0, buff.Length);
                return buff;
            }

            var pLen = size - 2;//去掉标志位后的长度
            var rBuffer = new byte[pLen - tCnt.Item1 - tCnt.Item2];

            fixed (byte* dst = rBuffer, src = &(buffer[offset + 1]))
            {
                var tempSrc = src;
                var tempDst = dst;

                //开始标志位
                //*(dst+0) = *(src + offset);

                //消息头和消息体
                while (pLen >= 0)
                {
                    if (*(tempSrc) == SubFlag && *(tempSrc + 1) == 0x01)
                    {

                        *(tempDst) = PackageFlag;
                        tempSrc += 2;
                        tempDst += 1;
                        pLen -= 2;
                    }
                    else if (*(tempSrc) == SubFlag && *(tempSrc + 1) == 0x02)
                    {
                        *(tempDst) = SubFlag;
                        tempSrc += 2;
                        tempDst += 1;
                        pLen -= 2;
                    }
                    else
                    {
                        *(tempDst) = *(tempSrc);
                        tempSrc += 1;
                        tempDst += 1;
                        pLen -= 1;
                    }
                }

                return rBuffer;
            }
        }

        /// <summary>
        /// 检查要转义的标志数
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private unsafe Tuple<int, int> CheckEscapeFlagBitCount(byte[] buffer)
        {
            var len = buffer.Length - 2;//去头尾标识位
            int pktCnt = 0, subCnt = 0;
            fixed (byte* src = &(buffer[1]))
            {
                var tempSrc = src;
                do
                {
                    if (*tempSrc == PackageFlag)
                    {
                        ++pktCnt;
                    }
                    else if (*tempSrc == SubFlag)
                    {
                        ++subCnt;
                    }
                    tempSrc += 1;
                    --len;
                } while (len > 0);
            }
            return Tuple.Create(pktCnt, subCnt);
        }

        /// <summary>
        /// 检查被转义的标志数
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private unsafe Tuple<int, int> CheckRestoreFlagBitCount(byte[] buffer, int offset, int size)
        {
            var len = size - 2;
            int pkgCnt = 0, subCnt = 0;
            fixed (byte* src = &buffer[offset + 1])
            {
                var tempSrc = src;
                do
                {
                    if (*tempSrc == SubFlag && *(tempSrc + 1) == 0x01)
                    {
                        ++pkgCnt;
                        tempSrc += 2;
                        len -= 2;
                    }
                    else if (*tempSrc == SubFlag && *(tempSrc + 1) == 0x02)
                    {
                        ++subCnt;
                        tempSrc += 2;
                        len -= 2;
                    }
                    else
                    {
                        tempSrc += 1;
                        --len;
                    }
                } while (len > 0);
            }
            return Tuple.Create(pkgCnt, subCnt);
        }
        #endregion
    }
}
