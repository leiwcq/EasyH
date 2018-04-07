using System;

namespace EasyH.DependencyInjection.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]

    public class InjectAttribute : Attribute
    {
    }
}
