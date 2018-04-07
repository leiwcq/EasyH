﻿using System;

namespace EasyH.DependencyInjection.Internal
{
    /// <summary>
    /// Struct type tuple
    /// </summary>
    internal struct StructTuple<TFirst, TSecond> : IEquatable<StructTuple<TFirst, TSecond>>
    {
        public TFirst First { get; private set; }
        public TSecond Second { get; private set; }

        public StructTuple(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }

        public bool Equals(StructTuple<TFirst, TSecond> obj)
        {
            return ((First == null) == (obj.First == null)) &&
                (First == null ? true :
                    (ReferenceEquals(First, obj.First) || First.Equals(obj.First))) &&
                ((Second == null) == (obj.Second == null)) &&
                (Second == null ? true :
                    (ReferenceEquals(Second, obj.Second) || Second.Equals(obj.Second)));
        }

        public override bool Equals(object obj)
        {
            return (obj is StructTuple<TFirst, TSecond>) && Equals((StructTuple<TFirst, TSecond>)obj);
        }

        public override int GetHashCode()
        {
            // same with Tuple.CombineHashCodess
            var hash1 = First?.GetHashCode() ?? 0;
            var hash2 = Second?.GetHashCode() ?? 0;
            return (hash1 << 5) + hash1 ^ hash2;
        }
    }
}