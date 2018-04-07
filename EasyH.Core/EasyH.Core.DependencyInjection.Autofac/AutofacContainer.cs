using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using EasyH.Core.DependencyInjection.Container;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace EasyH.Core.DependencyInjection.Autofac
{
    public class AutofacContainer : IDIContainer
    {

        public string Name { get; private set; }
        public string ConfigFile { get; private set; }

        public IServiceProvider ServiceProvider { get; protected set; }

        private readonly ContainerBuilder _containerBuilder;
        
        private IContainer _container;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AutofacContainer()
            : this(new ContainerBuilder())
        {
        }


        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="containerBuilder"></param>
        public AutofacContainer(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterCallback(registry =>
            {
                registry.Registered += (sender, e) =>
                {
                    //TODO:AOP Config
                    /*e.ComponentRegistration.Activating += (s, ea) =>
                    {

                        if (ea.Instance == null)
                        {
                            throw new ArgumentNullException("e");
                        }
                        var targetType = ea.Instance.GetType();
                        //Console.WriteLine("Activating:" + targetType.FullName);
                        if (AopContainer.HasInterceptor(targetType))
                        {
                            //Console.WriteLine("Activating:" + targetType.FullName + " HasInterceptor");
                            var interceptors = AopContainer.GetInterceptor(targetType).ToArray();
                            var targetInterfaces = targetType.GetInterfaces();
                            if (targetInterfaces.Any())
                            {
                                var instance =
                                    _proxyGenerator.CreateInterfaceProxyWithTargetInterface(targetInterfaces.Last(),
                                        ea.Instance, interceptors);
                                ea.Instance = instance;
                            }
                            else
                            {
                                var greediestCtor =
                                    targetType.GetConstructors().OrderBy(x => x.GetParameters().Count()).LastOrDefault();
                                var ctorDummyArgs = greediestCtor == null
                                    ? new object[0]
                                    : new object[greediestCtor.GetParameters().Count()];
                                var instance = _proxyGenerator.CreateClassProxyWithTarget(targetType, ea.Instance,
                                    ctorDummyArgs,
                                    interceptors);
                                ea.Instance = instance;
                            }
                        }
                    };*/
                };
            });
            _containerBuilder = containerBuilder;
        }

        public void Init(string name = "Default", string configFileName = "")
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = Guid.NewGuid().ToString("N");
            }
            if (!string.IsNullOrEmpty(ConfigFile))
            {
                ConfigureContainer(ConfigFile);
            }
        }

        protected ContainerBuilder ConfigureContainer(string configFileBaseName)
        {
            var fileName = string.Format(
                "{0}\\ContainerConfig\\{1}",
                AppDomain.CurrentDomain.BaseDirectory,
                configFileBaseName);
            if (!File.Exists(fileName)) return _containerBuilder;

            //var csr = new ConfigurationSettingsReader(SectionHandler.DefaultSectionName, fileName);
            //cb.RegisterModule(csr);

            var config = new ConfigurationBuilder();
            config.AddXmlFile(fileName);
            var module = new ConfigurationModule(config.Build());
            _containerBuilder.RegisterModule(module);
            return _containerBuilder;
        }

        public IDIContainer Populate(IServiceCollection services)
        {
            _containerBuilder.Populate(services);
            return this;
        }

        public IDIContainer Build()
        {
            _container = _containerBuilder.Build();
            ServiceProvider = _container.Resolve<IServiceProvider>();
            return this;
        }       
    }
}
