using System.Security.Cryptography;
using System.Text;

namespace EasyH.Core.Security
{
    public static class Encipherment
    {
        public static string MD5(string sourceString)
        {
            return MD5(sourceString, Encoding.Default);
        }

        public static string MD5(byte[] data)
        {
            byte[] buffer = new MD5CryptoServiceProvider().ComputeHash(data);
            var builder = new StringBuilder(0x20);
            for (int i = 0; i < buffer.Length; i++)
            {
                builder.Append(buffer[i].ToString("x").PadLeft(2, '0'));
            }
            return builder.ToString();
        }

        public static byte[] ToMD5(byte[] data,int offset,int count)
        {
            return new MD5CryptoServiceProvider().ComputeHash(data,offset,count);
        }

        /// <summary>
        /// 用指定编码得到哈希密码
        /// </summary>
        /// <param name="sourceString">源</param>
        /// <param name="encoding">gb2312 utf-8等</param>
        /// <returns></returns>
        public static string MD5(string sourceString, Encoding encoding)
        {
            var buffer = new MD5CryptoServiceProvider().ComputeHash(encoding.GetBytes(sourceString));
            var builder = new StringBuilder(0x20);
            for (var i = 0; i < buffer.Length; i++)
            {
                builder.Append(buffer[i].ToString("x").PadLeft(2, '0'));
            }
            return builder.ToString();
        }

        public static unsafe int Hash(string value)
        {
            fixed (char* cr = value.ToCharArray())
            {
                var cpt = cr;
                var flag = 0x15051505;
                var v = flag;
                var t = (int*) cpt;
                for (var i = value.Length; i > 0; i -= 4)
                {
                    flag = (((flag << 5) + flag) + (flag >> 0x1b)) ^ t[0];
                    if (i <= 2)
                    {
                        break;
                    }
                    v = (((v << 5) + v) + (v >> 0x1b)) ^ t[1];
                    t += 2;
                }
                return (flag + (v*0x5d588b65));
            }
        }

        public static string HMAC_MD5(string value, string key,Encoding encoding)
        {
            var m = new HMACMD5(encoding.GetBytes(key));
            var data=encoding.GetBytes(value);
            var buffer=m.ComputeHash(data, 0, data.Length);
            var builder = new StringBuilder(0x20);
            for (var i = 0; i < buffer.Length; i++)
            {
                builder.Append(buffer[i].ToString("x").PadLeft(2, '0'));
            }
            return builder.ToString();
        }
    }
}
