using System;
using System.Collections.Generic;
using EasyH.DependencyInjection.AOP;

namespace EasyH.DependencyInjection
{
    internal class ServiceTypesContext
    {
        internal ServiceTypesContext(Type serviceType)
        {
            TypeDef = TypeDef.Object;
            RequestServiceType = serviceType;
            Build();
        }

        /// <summary>
        /// 请求类型
        /// </summary>
        public Type RequestServiceType { set; get; }
        /// <summary>
        /// 请求类型的泛型
        /// </summary>
        public Type RequestGenericTypeDefinition { set; get; }
        /// <summary>
        /// 请求泛型类型的参数类型
        /// </summary>
        public Type GenericArgType { set; get; }
        /// <summary>
        /// 请求类型是否是泛型
        /// </summary>
        public bool IsGenericType { set; get; }

        /// <summary>
        /// 类型类别
        /// </summary>
        public TypeDef TypeDef { set; get; } 

        private void Build()
        {
            if (RequestServiceType.IsGenericType) {
                IsGenericType = true;
                RequestGenericTypeDefinition = RequestServiceType.GetGenericTypeDefinition();
            }

            if (!RequestServiceType.IsGenericType) { }
            else if (RequestGenericTypeDefinition == typeof(List<>))
            {
                GenericArgType = RequestServiceType.GetGenericArguments()[0];
                TypeDef = TypeDef.List;
            }
            else if (RequestGenericTypeDefinition == typeof(IEnumerable<>))
            {
                GenericArgType = RequestServiceType.GetGenericArguments()[0];
                TypeDef = TypeDef.Enumerable;
            }
            else if (RequestGenericTypeDefinition == typeof(IInvocation<>) ||
                     RequestGenericTypeDefinition == typeof(IInvocation<,>))
            {
                TypeDef = TypeDef.Proxy;
            }
        }

    }
}
