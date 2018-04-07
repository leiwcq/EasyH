using System;
using System.Reflection;

namespace EasyH.DependencyInjection.AOP
{
    public interface IMethodInterceptor
    {
        void OnBefore(Type targetType,object target, MethodInfo targetMethod, object[] args);

        object OnAfter(Type targetType, object target, object returnValue ,MethodInfo targetMethod, object[] args);

        void OnException(Type targetType, object target, MethodInfo targetMethod, object[] args ,Exception ex);

    }

    public interface IMethodInterceptor<T>: IMethodInterceptor { }

}
