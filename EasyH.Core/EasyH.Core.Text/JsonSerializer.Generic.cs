using System;
using System.IO;
using EasyH.Core.Text.Common;
using EasyH.Core.Text.Json;

namespace EasyH.Core.Text
{
    public class JsonSerializer<T> : ITypeSerializer<T>
    {
        public bool CanCreateFromString(Type type)
        {
            return JsonReader.GetParseFn(type) != null;
        }

        /// <summary>
        /// Parses the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public T DeserializeFromString(string value)
        {
            if (string.IsNullOrEmpty(value)) return default(T);
            return (T)JsonReader<T>.Parse(value);
        }

        public T DeserializeFromReader(TextReader reader)
        {
            return DeserializeFromString(reader.ReadToEnd());
        }

        public string SerializeToString(T value)
        {
            if (value == null) return null;
            if (typeof(T) == typeof(string)) return value as string;
            if (typeof(T) == typeof(object) || typeof(T).IsAbstract || typeof(T).IsInterface)
            {
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = true;
                var result = JsonSerializer.SerializeToString(value, value.GetType());
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = false;
                return result;
            }

            var writer = StringWriterThreadStatic.Allocate();
            JsonWriter<T>.WriteObject(writer, value);
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
            if (typeof(T) == typeof(object) || typeof(T).IsAbstract || typeof(T).IsInterface)
            {
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = true;
                JsonSerializer.SerializeToWriter(value, value.GetType(), writer);
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = false;
                return;
            }

            JsonWriter<T>.WriteObject(writer, value);
        }
    }
}