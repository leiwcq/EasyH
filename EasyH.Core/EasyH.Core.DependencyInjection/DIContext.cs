using EasyH.Core.DependencyInjection.Container;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyH.Core.DependencyInjection
{
    /// <summary>
    /// 依赖注入上下文
    /// </summary>
    public class DiContext
    {
        /// <summary>
        /// 全局依赖注入上下文
        /// </summary>
        public static DiContext Instance { get; private set; }

        /// <summary>
        /// 类型查找器
        /// </summary>
        public static ITypeFinder TypeFinder { get; private set; }

        /// <summary>
        /// 上下文名称
        /// </summary>
        private string _name;

        internal DiContext()
        {
        }

        /// <summary>
        /// <para>创建依赖注入上下文</para>
        /// <para>默认上下文名称为Default</para>
        /// <para>在默认依赖注入容器中需要注册IDependencyInjectionContainer</para>
        /// <para>用于创建子容器时生成容器</para>
        /// </summary>
        /// <param name="name">上下文名称</param>
        /// <param name="container">依赖注入容器</param>
        /// <param name="configFileName">配置文件名</param>
        /// <returns>依赖注入上下文</returns>
        public static DiContext Create(string name = "Default", IDIContainer container = null, string configFileName = "")
        {
            //是否为创建默认依赖注入上下文
            if (string.IsNullOrWhiteSpace(name) || name.Equals("DEFAULT", StringComparison.CurrentCultureIgnoreCase))
            {
                //依赖注入上下文不允许重复创建
                if (Instance != null)
                {
                    throw new NotSupportedException("Could not create Default instance twice.");
                }
                Instance = new DiContext();
                TypeFinder = new ContainerTypeFinder();
                Instance.SetContainer(container);
                return Instance;
            }

            //下级依赖注入上下文创建之前必须创建默认依赖注入上下文
            if (Instance?.Container == null)
            {
                throw new NullReferenceException("Could not create configuration instance with base instance is null.");
            }
            var diContext = Instance.Container.ServiceProvider.GetServices<DiContext>();
            if (false)
            {
                throw new NotSupportedException("Could not create configuration instance twice.");
            }

            var instance = new DiContext
            {
                _name = name
            };

            if (container == null)
            {
                //var newContainer = Instance.Container.ServiceProvider.GetService.Resolve<IDIContainer>();
                //if (newContainer != null)
                //{
                //    instance.SetContainer(newContainer);
                //    Instance.Container.RegisterInstanceWithKeyed(instance, name);
                //    return instance;
                //}
                //else
                //{
                //    throw new NullReferenceException("Could not create instance.");
                //}
            }

            instance.SetContainer(container);
            //Instance.Container.RegisterInstanceWithKeyed(instance, name);
            return instance;
        }

        public static DiContext Load(string name = "Default")
        {
            if (string.IsNullOrWhiteSpace(name) || name.Equals("DEFAULT", StringComparison.CurrentCultureIgnoreCase))
            {
                return Instance;
            }
            //if (Instance.Container.IsRegisteredWithKey<DIContext>(name))
            //{
            //    var diContext = Instance.Container.ResolveKeyed<DIContext>(name);
            //    return diContext;
            //}

            throw new NullReferenceException($"Failed to load a container named {name}.");
        }

        public IDIContainer Container { get; protected set; }

        /// <summary>
        /// 设置新的依赖注入容器
        /// </summary>
        /// <param name="container">依赖注入容器</param>
        /// <param name="configFileName">配置文件名称</param>
        /// <returns>依赖注入上下文</returns>
        public DiContext SetContainer(IDIContainer container, string configFileName = "")
        {
            if (container != null)
            {
                Container = container;
                if (!string.IsNullOrEmpty(configFileName))
                {
                    container.Init(_name, configFileName);
                }
            }
            return this;
        }

        public DiContext RegisterUnhandledExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                //TODO:错误日志
            };
            return this;
        }

        /// <summary>
        /// 生成依赖注入容器
        /// </summary>
        /// <returns></returns>
        public DiContext Build()
        {
            //生成依赖注入容器之前必须创建依赖注入上下文
            if (Instance == null || Instance.Container == null)
            {
                throw new NullReferenceException("The container must be created before Build.");
            }
            Instance.Container.Build();
            return this;
        }
    }
}
