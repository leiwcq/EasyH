using EasyH.Net.Base;

namespace EasyH.Net.Common
{
    public class SegmentOffset
    {
        public byte[] Buffer { get; set; }

        public int Offset { get; set; }

        public int Size { get; set; }

        public SegmentOffset()
        {

        }

        public SegmentOffset(byte[] buffer)
        {
            Buffer = buffer;
            Size = buffer.Length;
        }

        public SegmentOffset(byte[] buffer, int offset, int size)
        {
            Buffer = buffer;
            Offset = offset;
            Size = size;
        }
    }

    public class SegmentOffsetToken
    {
        public SocketToken SToken { get; set; }

        public SegmentOffset DataSegment { get; set; }

        public SegmentOffsetToken()
        {

        }

        public SegmentOffsetToken(SocketToken sToken)
        {
            SToken = sToken;
        }

        public SegmentOffsetToken(SocketToken sToken,SegmentOffset dataSegment)
        {
            SToken = sToken;
            DataSegment = dataSegment;
        }

        public SegmentOffsetToken (SocketToken sToken,byte[] buffer)
        {
            SToken = sToken;
            DataSegment = new SegmentOffset(buffer);
        }

        public SegmentOffsetToken(SocketToken sToken,byte[] buffer,int offset,int size)
        {
            SToken = sToken;
            DataSegment = new SegmentOffset(buffer, offset, size);
        }
    }
}
