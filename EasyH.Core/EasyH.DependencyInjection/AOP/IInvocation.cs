namespace EasyH.DependencyInjection.AOP
{
    //MethodInterceptor

    public interface IInvocation<T>
    {
        T Proxy { set; get; }
    }

    public interface IInvocation<T,TP> 
    {
        T Proxy { set; get; }
        TP Interceptor { set; get; }
    }


    public class InvocationProxy<TProxy> : IInvocation<TProxy>
    {
        public InvocationProxy(TProxy instance)
        {
            Proxy = instance;
        }

        public TProxy Proxy { set; get; }
    }


    public class InvocationProxy<TProxy, TInterceptor> : IInvocation<TProxy, TInterceptor> where TInterceptor : IMethodInterceptor
    {
        public InvocationProxy(TProxy instance, TInterceptor interceptor)
        {
            Proxy = instance;
            Interceptor = interceptor;
        }

        public TProxy Proxy { set; get; }
        public TInterceptor Interceptor { set; get; }
    }



}
