﻿using System;
using System.Text;

namespace EasyH.Core.Text
{
    /// <summary>
    /// Reusable StringBuilder ThreadStatic Cache
    /// </summary>
    public static class StringBuilderCache
    {
        [ThreadStatic]
        static StringBuilder cache;

        public static StringBuilder Allocate()
        {
            var ret = cache;
            if (ret == null)
                return new StringBuilder();

            ret.Length = 0;
            cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringBuilder sb)
        {
            cache = sb;
        }

        public static string ReturnAndFree(StringBuilder sb)
        {
            var ret = sb.ToString();
            cache = sb;
            return ret;
        }
    }

    /// <summary>
    /// Alternative Reusable StringBuilder ThreadStatic Cache
    /// </summary>
    public static class StringBuilderCacheAlt
    {
        [ThreadStatic]
        static StringBuilder cache;

        public static StringBuilder Allocate()
        {
            var ret = cache;
            if (ret == null)
                return new StringBuilder();

            ret.Length = 0;
            cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringBuilder sb)
        {
            cache = sb;
        }

        public static string ReturnAndFree(StringBuilder sb)
        {
            var ret = sb.ToString();
            cache = sb;
            return ret;
        }
    }

    //Use separate cache internally to avoid reallocations and cache misses
    internal static class StringBuilderThreadStatic
    {
        [ThreadStatic]
        static StringBuilder cache;

        public static StringBuilder Allocate()
        {
            var ret = cache;
            if (ret == null)
                return new StringBuilder();

            ret.Length = 0;
            cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringBuilder sb)
        {
            cache = sb;
        }

        public static string ReturnAndFree(StringBuilder sb)
        {
            var ret = sb.ToString();
            cache = sb;
            return ret;
        }
    }
}