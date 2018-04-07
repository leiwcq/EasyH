using System;

namespace EasyH.Net.Protocols
{
   public static class ByteConvert
    {
        public static byte[] ToBytes(this ushort value)
        {
            return new[]{
                     (byte)(value >> 8),
                     (byte)value
                };
        }

        public static byte[] ToBytes(this uint value)
        {
            return new[] {
                 (byte)(value >> 24),
                 (byte)(value >> 16),
                 (byte)(value >> 8),
                 (byte)value
                 };
        }

        public static byte[] ToBytes(this ulong value)
        {
            return new[]{
                  (byte)(value >> 56),
                  (byte)(value >> 48),
                  (byte)(value >> 40),
                  (byte)(value >> 32),
                  (byte)(value >> 24),
                  (byte)(value >> 16),
                  (byte)(value >> 8),
                  (byte)value
             };
        }

        public static ushort ToUInt16(this byte[] array,int offset=0)
        {
            return (ushort)((array[offset] << 8) | array[offset+1]);
        }

        public static uint ToUInt32(this byte[] array,int offset=0)
        {
            return (((uint)array[offset] << 24)
               | ((uint)array[offset+1] << 16)
               | ((uint)array[offset + 2] << 8)
               | array[offset + 3]);
        }

        public static ulong ToUInt64(this byte[] array, int offset = 0)
        {
            return (((ulong)array[offset] << 56)
                 | ((ulong)array[offset + 1] << 48)
                 | ((ulong)array[offset + 2] << 40)
                 | ((ulong)array[offset + 3] << 32)
                 | ((ulong)array[offset + 4] << 24)
                 | ((ulong)array[offset + 5] << 16)
                 | ((ulong)array[offset + 6] << 8)
                 | array[offset + 7]);
        }

        internal static unsafe void BulkCopy(byte* src, byte* dest, int count)
        {
            if (count >= 16)
            {
                do
                {
                    *((ulong*)dest) = *((ulong*)src);
                    *((ulong*)(dest + 8)) = *((ulong*)(src + 8));
                    dest += 16;
                    src += 16;
                }
                while ((count -= 16) >= 16);
            }
            if (count > 0)
            {
                if ((count & 8) != 0)
                {
                    *((ulong*)dest) = *((ulong*)src);
                    dest += 8;
                    src += 8;
                }
                if ((count & 4) != 0)
                {
                    *((uint*)dest) = *((uint*)src);
                    dest += 4;
                    src += 4;
                }
                if ((count & 2) != 0)
                {
                    *((ushort*)dest) = *((ushort*)src);
                    dest += 2;
                    src += 2;
                }
                if ((count & 1) != 0)
                {
                    dest[0] = src[0];
                    //dest++;
                    //src++;
                }
            }

        }
    }
}
