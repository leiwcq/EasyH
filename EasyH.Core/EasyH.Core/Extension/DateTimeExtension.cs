using System;
using System.Globalization;

namespace EasyH.Core.Extension
{
    public static class DateTimeExtension
    {
        public static DateTime GetAllowDateTime(this DateTime dateTime)
        {
            return dateTime.Year < 1970 ? DateTime.Now : dateTime;
        }

        /// <summary>
        /// 时期时间格式转换
        /// </summary>
        /// <param name="dateTimeString">原串yyyyMMddHHmmss</param>
        /// <returns>日期时间yyyy-MM-dd HH:mm:ss</returns>
        public static DateTime ConvertStringToDateTime(this string dateTimeString)
        {
            DateTime dtime;
            if (
                !DateTime.TryParseExact(dateTimeString, "yyyyMMddHHmmss",
                    CultureInfo.CurrentCulture, DateTimeStyles.None, out dtime))
            {
                return new DateTime(1,1,1);
            }

            return dtime;
        }

        /// <summary>
        /// 月天时分
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToStringMdHm(this DateTime dateTime)
        {
            return dateTime.ToString("MM-dd HH:mm");
        }

        /// <summary>
        /// 获取当前时间的年月日时分秒
        /// </summary>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static DateTime ToLongTime(this DateTime endTime)
        {
            var resultTime = endTime.Year < 1970 ? DateTime.Now : endTime;
            resultTime = new DateTime(resultTime.Year, resultTime.Month, resultTime.Day, DateTime.Now.Hour,
                DateTime.Now.Minute, DateTime.Now.Second);
            return resultTime;
        }

        /// <summary>
        /// 获取当前时间的年月日时分秒
        /// </summary>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static DateTime ToLongTime(this DateTime endTime,int hour,int minute,int second)
        {
            var resultTime = endTime.Year < 1970 ? DateTime.Now : endTime;
            resultTime = new DateTime(resultTime.Year, resultTime.Month, resultTime.Day, hour,minute, second);
            return resultTime;
        }

        /// <summary>
        /// 获取当前时间的年月日时分秒
        /// </summary>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static DateTime ToLongTimeAll(this DateTime endTime)
        {
            var resultTime = endTime.Year < 1970 ? DateTime.Now : endTime;
            resultTime = new DateTime(resultTime.Year, resultTime.Month, resultTime.Day, 23, 59, 59);
            return resultTime;
        }

        /// <summary>
        /// 设置kind时间
        /// </summary>
        /// <param name="unspecifyTime">无时区的时间</param>
        /// <param name="kind">kind= DateTimeKind.Local</param>
        /// <returns></returns>
        public static DateTime ToSpecifyTime(this DateTime unspecifyTime, DateTimeKind kind= DateTimeKind.Local)
        {
            if(unspecifyTime.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(unspecifyTime, kind);
            return unspecifyTime;
        }
    }
}
