using System;
using System.IO;
using EasyH.Core.Text.Jsv;

namespace EasyH.Core.Text
{
    public class TypeSerializer<T> : ITypeSerializer<T>
    {
        public bool CanCreateFromString(Type type)
        {
            return JsvReader.GetParseFn(type) != null;
        }

        /// <summary>
        /// Parses the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public T DeserializeFromString(string value)
        {
            if (string.IsNullOrEmpty(value)) return default(T);
            return (T)JsvReader<T>.Parse(value);
        }

        public T DeserializeFromReader(TextReader reader)
        {
            return DeserializeFromString(reader.ReadToEnd());
        }

        public string SerializeToString(T value)
        {
            if (value == null) return null;
            if (typeof(T) == typeof(string)) return value as string;

            var writer = StringWriterThreadStatic.Allocate();
            JsvWriter<T>.WriteObject(writer, value);
            return StringWriterThreadStatic.ReturnAndFree(writer);
        }

        public void SerializeToWriter(T value, TextWriter writer)
        {
            if (value == null) return;
            if (typeof(T) == typeof(string))
            {
                writer.Write(value);
                return;
            }

            JsvWriter<T>.WriteObject(writer, value);
        }
    }
}