using System;
using System.Collections.Generic;
using System.Text;
using CacheManager.Core;
using Microsoft.Extensions.Configuration;

namespace EasyH.Extensions.Configuration.Db
{
    public class DbConfigurationCacheFactory
    {
        public DbConfigurationCacheFactory(string configurationFileName = "cache.json")
        {
            if (string.IsNullOrWhiteSpace(configurationFileName))
            {
                configurationFileName = "cache.json";
            }

            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddJsonFile(configurationFileName);

            var configuration = builder.Build();

            var cacheConfiguration = configuration.GetCacheConfiguration()
                .Builder
                .WithJsonSerializer()
                .Build();
            CacheManager = new BaseCacheManager<string>(cacheConfiguration);
        }

        public ICacheManager<string> CacheManager { get; }
    }
}
