using System.Text.RegularExpressions;

namespace EasyH.Core.Extension
{
    public static class ReplaceStarExtension
    {
        public static string ReplaceStar(this string str, int begin, int last)
        {
            var pattern = "^(.{" + (begin <= 0 ? 3 : begin) + "})(.*)(.{" + (last <= 0 ? 3 : last) + "})$";
            var re = Regex.Replace(str, pattern, "$1***$3");
            return re;
        }

        /// <summary>
        /// 正则表达式截取两个字符之间的内容
        /// </summary>
        /// <param name="str">需截取的原始字符串</param>
        /// <param name="startStr">开始的字符</param>
        /// <param name="endStr">结束字符</param>
        /// <returns></returns>
        public static string GetRegStr(this string str, string startStr, string endStr)
        {
            Regex r = new Regex(@"(?<="+ startStr + ").*?(?="+endStr+")");
            Match mc = r.Match(str);
            var reStr = mc.Value;

            return reStr;
            
        }

    }
}