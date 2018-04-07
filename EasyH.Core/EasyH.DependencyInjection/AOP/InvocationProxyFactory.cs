using System;

namespace EasyH.DependencyInjection.AOP
{
    public class InvocationProxyFactory
    {
        public static object CreateInvocation(Type serviceType,object target)
        {
            Type invocationType = typeof(InvocationProxy<>);
            Type[] genericTypeArgs = { serviceType };
            var invocationGenericType = invocationType.MakeGenericType(genericTypeArgs);
            object result = Activator.CreateInstance(invocationGenericType,target);
            return result;
        }


        public static object CreateInvocationWithInterceptor(Type serviceType, object target, IMethodInterceptor interceptor)
        {
            Type invocationType = typeof(InvocationProxy<,>);
            Type[] genericTypeArgs = { serviceType , interceptor.GetType() };
            var invocationGenericType = invocationType.MakeGenericType(genericTypeArgs);
            object result = Activator.CreateInstance(invocationGenericType, target, interceptor);
            return result;
        }


    }
}
