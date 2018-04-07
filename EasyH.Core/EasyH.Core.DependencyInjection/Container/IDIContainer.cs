
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyH.Core.DependencyInjection.Container
{
    public interface IDIContainer
    {
        IServiceProvider ServiceProvider { get; }

        void Init(string name = "Default", string configFileName = "");

        IDIContainer Populate(IServiceCollection services);

        IDIContainer Build();
    }
}
