﻿using System;
using System.Reflection;
using EasyH.DependencyInjection.Internal.Extensions;

namespace EasyH.DependencyInjection.AOP
{
    public class DynamicProxy
    {
        public static T CreateWithInstance<T>(T model)
        {
            var proxy = DispatchProxy.Create<T, InvokeProxy>();
            ((InvokeProxy)(object)proxy).Initialize(typeof(T), model);
            return proxy;
        }


        public static T Create<T>(IServiceProvider sp)
        {
            var serviceInstance = sp.GetService(typeof(T));
            var proxy = DispatchProxy.Create<T, InvokeProxy>();
            ((InvokeProxy)(object)proxy).Initialize(typeof(T), serviceInstance);
            return proxy;
        }


        public static T Create<T>()
        {
            var proxy = DispatchProxy.Create<T, InvokeProxy>();
            return proxy;
        }

        public static object CreateWithInterceptor(Type serviceType, IMethodInterceptor methodInterceptor)
        {
            var proxy = Create(serviceType);
            ((InvokeProxy)(object)proxy).Initialize(serviceType, methodInterceptor);
            return proxy;
        }


        public static object Create(Type serviceType)
        {
            var genericMethodInfo = GetCreateMethodByDispatchProxy(serviceType);
            object proxy = genericMethodInfo.FastInvoke(null, null);
            return proxy;
        }

        private static MethodInfo GetCreateMethodByDispatchProxy(Type serviceType)
        {
            var factoryType = typeof(DispatchProxy);
            var methodInfo = factoryType.GetMethod("Create");
            MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(serviceType, typeof(InvokeProxy));
            return genericMethodInfo;
        }


    }
}
