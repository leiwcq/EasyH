using System;
using System.Collections.Generic;
using System.IO;


namespace EasyH.Core.Text.Common
{
    internal delegate void WriteListDelegate(TextWriter writer, object oList, WriteObjectDelegate toStringFn);

    internal delegate void WriteGenericListDelegate<T>(TextWriter writer, IList<T> list, WriteObjectDelegate toStringFn);

    internal delegate void WriteDelegate(TextWriter writer, object value);

    internal delegate ParseStringSegmentDelegate ParseFactoryDelegate();

    public delegate void WriteObjectDelegate(TextWriter writer, object obj);

    public delegate object ParseStringDelegate(string stringValue);

    public delegate object ParseStringSegmentDelegate(StringSegment value);

    public delegate object ConvertObjectDelegate(object fromObject);

    public delegate object ConvertInstanceDelegate(object obj, Type type);

    public delegate void DeserializationErrorDelegate(object instance, Type propertyType, string propertyName, string propertyValueStr, Exception ex);
}
