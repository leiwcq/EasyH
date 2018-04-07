using System.Collections.Generic;

namespace EasyH.Net.Protocols
{
    internal class PacketQueue
    {
        private readonly CycQueue<byte> _bucket;
        private readonly Packet _pkg;

        public PacketQueue(int maxCount)
        {
            _bucket = new CycQueue<byte>(maxCount);
            _pkg = new Packet();
        }

        public int Count => _bucket.Length;

        public bool SetBlock(byte[] buffer, int offset, int size)
        {
            if (_bucket.Capacity - _bucket.Length < size)
                return false;

            for (var i = 0; i < size; ++i)
            {
                var rt = _bucket.EnQueue(buffer[i + offset]);
                if (rt == false)
                    return false;
            }
            return true;
        }

        public List<Packet> GetBlocks()
        {
            var pos = 0;
            var pkgs = new List<Packet>(2);
            again:
            var head = _bucket.DeSearchIndex(Packet.PackageFlag, pos);
            if (head == -1) return pkgs;

            var peek = _bucket.PeekIndex(Packet.PackageFlag, 1);
            if (peek >= 0)
            {
                pos = 1;
                goto again;
            }

            //数据包长度
            var pkgLength = CheckCompletePackageLength(_bucket.Array, head);
            if (pkgLength == 0) return pkgs;

            //读取完整包并移出队列
            var array = _bucket.DeRange(pkgLength);
            if (array == null)return pkgs;
            
            //解析
            var rt = _pkg.DeocdeFromBytes(array, 0, array.Length);
            if (rt)
            {
                pkgs.Add(_pkg);
            }

            if (_bucket.Length > 0)
            {
                pos = 0;
                goto again;
            }
            
            return pkgs;
        }

        private unsafe int CheckCompletePackageLength(byte[] buff,int offset)
        {
            fixed (byte* src = buff)
            {
                var head = offset;
                var cnt = 0;
                byte flag = 0;
                do
                {
                    if (*(src + head) == Packet.PackageFlag)
                    {
                        ++flag;
                        if (flag == 2) return cnt + 1;
                    }
                    head = (head + 1) % _bucket.Capacity;
                    ++cnt;
                }
                while (cnt <= _bucket.Length);
                cnt = 0;
                return cnt;
            }
        }
    }
}
