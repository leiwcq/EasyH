using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EasyH.Core.Security
{
    public class DES
    {
        private readonly string _key;
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;
        private readonly Encoding _encoding;

        public DES(string key, int size, Encoding encoding)
        {

            _encoding = encoding;
            var des = new DESCryptoServiceProvider
            {
                Mode = CipherMode.CBC
            };
            _key = key.PadRight(size, '0');
            var ks = _encoding.GetBytes(_key);
            des.Key = ks;
            des.IV = _encoding.GetBytes(key.Substring(0, 8));

            _decryptor = des.CreateDecryptor();
            _encryptor = des.CreateEncryptor();
        }

        public DES(string key, int size, CipherMode mode, PaddingMode padding, Encoding encoding)
        {

            _encoding = encoding;
            var des = new DESCryptoServiceProvider
            {
                Mode = mode, 
                Padding = padding
            };
            _key = key;
            var ks = _encoding.GetBytes(_key);
            des.Key = ks;
            des.IV = _encoding.GetBytes(key.Substring(0, 8));


            _decryptor = des.CreateDecryptor();
            _encryptor = des.CreateEncryptor();
        }

        public DES(string key, int size) : this(key, size, Encoding.UTF8)
        {

        }

        public byte[] Decrypt(string value)
        {
            byte[] data = _encoding.GetBytes(value);
            return _decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] Decrypt(byte[] data,int offset,int length)
        {
            return _decryptor.TransformFinalBlock(data, offset, length);
        }

        public string DecryptToString(string value)
        {
            byte[] data = Decrypt(value);
            return _encoding.GetString(data);
        }

        public byte[] Encrypt(string value)
        {
            var data = _encoding.GetBytes(value);
            return _encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] Encrypt(byte[] data)
        {
            return _encryptor.TransformFinalBlock(data, 0, data.Length);
        }
        public byte[] Encrypt(byte[] data,int offset,int count)
        {
            return _encryptor.TransformFinalBlock(data, offset, count);
        }
        public string EncryptToString(string value)
        {
            var data = Encrypt(value);
            return Convert.ToBase64String(data);
        }

        public int GetKeyId()
        {
            var data = Encoding.Default.GetBytes(_key);
            return data.Select((t, i) => (t & 0xFF) << (8*(3 - i))).Sum();
        }
    }
}
