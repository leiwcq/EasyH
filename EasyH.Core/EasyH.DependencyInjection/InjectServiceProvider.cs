using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using EasyH.DependencyInjection.Attributes;
using System.Collections.Concurrent;
using System.Reflection;
using EasyH.DependencyInjection.AOP;
using EasyH.DependencyInjection.Internal;
using EasyH.DependencyInjection.Internal.Extensions;

namespace EasyH.DependencyInjection
{
    public class InjectServiceProvider : IServiceProvider, IDisposable
    {

        private static ConcurrentDictionary<Type, List<PropertyInfo>> _injectServiceTypes = 
                                                    new ConcurrentDictionary<Type, List<PropertyInfo>>();

        private readonly Lazy<IServiceScopeFactory> _serviceScopeFactoryLazy;

        private IServiceCollection _serviceCollection;

        private IServiceProvider _serviceProvider = null;


        public InjectServiceProvider(IServiceProvider sp, IServiceCollection serviceCollection) {

            _serviceProvider = sp;
            _serviceCollection = serviceCollection;
            _serviceScopeFactoryLazy = new Lazy<IServiceScopeFactory>(
                () => new InjectServiceScopeFactory(this, _serviceCollection));

        }

       


        public object GetService(Type serviceType)
        {
            return GetServiceTypeOfFactory(new ServiceTypesContext(serviceType));
        }



        /// <summary>
        /// 构造指定类型实例的工厂
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private object GetServiceTypeOfFactory(ServiceTypesContext context)
        {
            object result = null;

            switch (context.TypeDef)
            {
                case TypeDef.Enumerable:
                    result = _serviceProvider.GetServices(
                                                    context.GenericArgType);
                    break;
                case TypeDef.List:
                    result = _serviceProvider.GetServices(
                                        context.GenericArgType)?.ToList();
                    break;
                case TypeDef.Object:
                    result = GetObjectService(context.RequestServiceType);
                    break;
                case TypeDef.Proxy:
                    result = GetProxyService(
                                        context.RequestGenericTypeDefinition,
                                        context.RequestServiceType);
                    break;
            }

            return result;
        }


        /// <summary>
        /// 通过IOC向属性注入实例
        /// </summary>
        /// <param name="serviceInstance"></param>
        /// <param name="serviceType"></param>
        private void GetObjectAndInjectPropertyService(object serviceInstance,Type serviceType)
        {
            if(!_injectServiceTypes.ContainsKey(serviceType))
            {
                var propertys = serviceType.GetProperties();
                var propertyInfoList = new List<PropertyInfo>();

                foreach (var propertyInfo in propertys)
                {
                    var injectAttr = propertyInfo.GetAttribute<InjectAttribute>();
                    if (injectAttr != null)
                        propertyInfoList.Add(propertyInfo);
                }
                if(propertyInfoList.Count > 0)
                    _injectServiceTypes.TryAdd(serviceType, propertyInfoList);
            }

            var injectPropertyInfoList = _injectServiceTypes.GetOrDefault(serviceType);
            if(injectPropertyInfoList != null)
            {
                foreach(var injectPropertyInfo in injectPropertyInfoList)
                {
                    var propertyValue = GetService(injectPropertyInfo.PropertyType);
                    if(propertyValue != null)
                        injectPropertyInfo.FastSetValue(serviceInstance,propertyValue);
                }
            }





        }

        /// <summary>
        /// 获取指定类型的实例，并注入InjectAttribute标记的属性。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private object GetObjectService(Type serviceType)
        {
            var result = _serviceProvider.GetRequiredService(serviceType);
            if (result != null)
                GetObjectAndInjectPropertyService(result, serviceType);

            return result;
        } 

        /// <summary>
        /// 获取指定类型的动态代理
        /// </summary>
        /// <param name="genericType"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private object GetProxyService(Type genericType, Type serviceType)
        {
            object result = null;
            Type proxyType = null;
            object proxy = null;
            if (genericType == typeof(IInvocation<>))
            {
                proxyType = serviceType.GetGenericArguments()[0];
                proxy = DynamicProxy.Create(proxyType);
                result = InvocationProxyFactory.CreateInvocation(proxyType, proxy);
            }
            else if (genericType == typeof(IInvocation<,>))
            {
                proxyType = serviceType.GetGenericArguments()[0];
                var interceptorType = serviceType.GetGenericArguments()[1];
                IMethodInterceptor methodInterceptor = (IMethodInterceptor)
                                 _serviceProvider.GetRequiredService(interceptorType);

                proxy = DynamicProxy.CreateWithInterceptor(proxyType, methodInterceptor);
                result = InvocationProxyFactory.CreateInvocationWithInterceptor(proxyType, proxy, methodInterceptor);
            }
            return result;
        }



        public void Dispose()
        {
            ((IDisposable)_serviceProvider)?.Dispose();
        }

    }

    /// <summary>
    /// 请求类型的定义
    /// </summary>
    enum TypeDef
    {
        List,
        Enumerable,
        Object,
        Proxy
    }

}
