using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EasyH.Core.DependencyInjection.Aop
{
    public class AopContainer
    {
        /*private readonly IDictionary<Type, IEnumerable<IInterceptor>> _aopInterceptor;

        private readonly ICollection<IInterceptor> _globalInterceptor;

        public IEnumerable<IInterceptor> AddGlobalInterceptor(params IInterceptor[] interceptors)
        {
            foreach (var interceptor in interceptors)
            {
                _globalInterceptor.Add(interceptor);
            }
            return _globalInterceptor;
        }

        public AopContainer()
        {
            _aopInterceptor = new ConcurrentDictionary<Type, IEnumerable<IInterceptor>>();
            _globalInterceptor = new List<IInterceptor>();
            //TODO:从配置文件中加载默认公用拦截器
        }

        public void AddInterceptor(Type type, params IInterceptor[] interceptors)
        {
            var interceptor = Enumerable.Empty<IInterceptor>();
            if (_aopInterceptor.ContainsKey(type))
            {
                interceptor = _aopInterceptor[type];
            }
            if (interceptor == null)
            {
                interceptor = Enumerable.Empty<IInterceptor>();
            }
            if (_aopInterceptor.ContainsKey(type))
            {
                _aopInterceptor[type] = interceptor.Union(interceptors);
            }
            else
            {
                _aopInterceptor.Add(type, interceptor);
            }
        }

        public bool HasInterceptor(Type type)
        {
            return (_aopInterceptor.ContainsKey(type) && _aopInterceptor[type].Any()) || _globalInterceptor.Any();
        }

        public IEnumerable<IInterceptor> GetInterceptor(Type type)
        {
            if (_aopInterceptor.ContainsKey(type))
            {
                var interceptor = _aopInterceptor[type];
                if (interceptor != null)
                {
                    return _globalInterceptor.Union(interceptor);
                }
            }
            return _globalInterceptor;
        }*/
    }
}
