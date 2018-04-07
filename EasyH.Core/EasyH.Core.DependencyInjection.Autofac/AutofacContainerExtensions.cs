using Autofac;
using EasyH.Core.DependencyInjection.Container;

namespace EasyH.Core.DependencyInjection.Autofac
{
    public static class AutofacContainerExtensions
    {
        /// <summary>
        /// Use Autofac as the object container.
        /// </summary>
        /// <returns></returns>
        public static DIContext UseAutofac(this DIContext diContext)
        {
            return diContext.UseAutofac(new ContainerBuilder());
        }
        /// <summary>
        /// Use Autofac as the object container.
        /// </summary>
        /// <returns></returns>
        public static DIContext UseAutofac(this DIContext diContext, ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<AutofacContainer>().As<IDIContainer>();
            diContext.SetContainer(new AutofacContainer(containerBuilder));
            return diContext;
        }
    }
}
