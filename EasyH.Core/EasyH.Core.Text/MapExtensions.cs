using System.Collections.Generic;
using EasyH.Core.Text.Common;

namespace EasyH.Core.Text
{
    public static class MapExtensions
    {
        public static string Join<K, V>(this Dictionary<K, V> values)
        {
            return Join(values, JsWriter.ItemSeperatorString, JsWriter.MapKeySeperatorString);
        }

        public static string Join<K, V>(this Dictionary<K, V> values, string itemSeperator, string keySeperator)
        {
            var sb = StringBuilderThreadStatic.Allocate();
            foreach (var entry in values)
            {
                if (sb.Length > 0)
                    sb.Append(itemSeperator);

                sb.Append(entry.Key).Append(keySeperator).Append(entry.Value);
            }
            return StringBuilderThreadStatic.ReturnAndFree(sb);
        }
    }
}