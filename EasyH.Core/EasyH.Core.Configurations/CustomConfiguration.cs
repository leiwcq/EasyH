using EasyH.Core.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace EasyH.Core.Configurations
{
    /// <summary>
    /// 自定义配置文件
    /// </summary>
    public class CustomConfiguration
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        public CustomConfiguration()
            : this(Assembly.GetCallingAssembly().ManifestModule.Name)
        {
        }

        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        /// <param name="fileName"></param>
        public CustomConfiguration(string fileName)
        {
            
            var path = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }

            path = Path.Combine(path, "Config");

            var configFileName = Path.Combine(path, $"{fileName}.json");
            if (File.Exists(configFileName))
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile($"{fileName}.json", true, true);

                _configuration = builder.Build();
            }
        }

        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="t"></param>
        public CustomConfiguration(string fileName,object t)
        {
            var path = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }
            path = Path.Combine(path, "Config");

            var configFileName = Path.Combine(path, $"{fileName}.json");

            #region 初始化.json文件
            if (!File.Exists(configFileName))
            {
                var strJson = t.ToJson();
                
                try
                {
                    var difo = new DirectoryInfo(Path.GetDirectoryName(configFileName));
                    if(!difo.Exists) difo.Create();
                    using (var sw = new StreamWriter(configFileName, false, Encoding.UTF8))
                    {
                        sw.Write(strJson);
                        sw.Close();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            #endregion

            if (File.Exists(configFileName))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(path)
                    .AddJsonFile($"{fileName}.json", true, true);

                _configuration = builder.Build();
            }
        }

        /// <summary>
        /// 返回指定的Section节点
        /// </summary>
        /// <param name="sectionName">Section节点名称</param>
        /// <typeparam name="TSection">ConfigurationSection</typeparam>
        /// <returns>Section节点</returns>
        public TSection GetSection<TSection>(string sectionName) where TSection : class, new()
        {
            var appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<TSection>(_configuration.GetSection(sectionName))
                .BuildServiceProvider()
                .GetService<IOptions<TSection>>()
                .Value;
            return appconfig;
        }

        /// <summary>
        /// 设置并获取配置节点对象
        /// </summary>  
        public TSection SetConfig<TSection>(string key, Action<TSection> action) where TSection : class, new()
        {
            var appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<TSection>(_configuration.GetSection(key))
                .Configure(action)
                .BuildServiceProvider()
                .GetService<IOptions<TSection>>()
                .Value;
            return appconfig;
        }
    }

    /// <summary>
    /// 自定义配置文件
    /// </summary>
    public class CustomConfiguration<T> where T:class 
    {
        private readonly IConfiguration _configuration;
        public T Model;

        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        public CustomConfiguration()
            : this(Assembly.GetCallingAssembly().ManifestModule.Name)
        {
        }

        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        /// <param name="fileName"></param>
        public CustomConfiguration(string fileName)
        {
            var path = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }

            path = Path.Combine(path, "Config");

            var configFileName = Path.Combine(path, $"{fileName}.json");
            if (File.Exists(configFileName))
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile($"{fileName}.json", true, true);

                _configuration = builder.Build();
            }
        }

        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="t"></param>
        public CustomConfiguration(string fileName, T t)
        {
            var path = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }
            path = Path.Combine(path, "Config");

            var configFileName = Path.Combine(path, $"{fileName}.json");

            #region 初始化.json文件
            if (!File.Exists(configFileName))
            {
                var strJson = t.ToJson();

                try
                {
                    var difo = new DirectoryInfo(Path.GetDirectoryName(configFileName));
                    if (!difo.Exists) difo.Create();
                    using (var sw = new StreamWriter(configFileName, false, Encoding.UTF8))
                    {
                        sw.Write(strJson);
                        sw.Close();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            #endregion

            if (File.Exists(configFileName))
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile($"{fileName}.json", optional: true, reloadOnChange: true);

                _configuration = builder.Build();
            }
        }

        /// <summary>
        /// 返回指定的Section节点
        /// </summary>
        /// <param name="sectionName">Section节点名称</param>
        /// <typeparam name="TSection">ConfigurationSection</typeparam>
        /// <returns>Section节点</returns>
        public TSection GetSection<TSection>(string sectionName) where TSection : class, new()
        {
            var appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<TSection>(_configuration.GetSection(sectionName))
                .BuildServiceProvider()
                .GetService<IOptions<TSection>>()
                .Value;
            return appconfig;
        }

        /// <summary>
        /// 设置并获取配置节点对象
        /// </summary>  
        public TSection SetConfig<TSection>(string key, Action<TSection> action) where TSection : class, new()
        {
            var appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<TSection>(_configuration.GetSection(key))
                .Configure(action)
                .BuildServiceProvider()
                .GetService<IOptions<TSection>>()
                .Value;
            return appconfig;
        }
    }
}
