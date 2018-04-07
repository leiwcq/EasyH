using Microsoft.Extensions.DependencyInjection;

namespace EasyH.DependencyInjection.Registration
{
    /// <summary>
    /// Ñ¡ÔñÆ÷
    /// </summary>
    internal interface ISelector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        void Populate(IServiceCollection services);
    }
}