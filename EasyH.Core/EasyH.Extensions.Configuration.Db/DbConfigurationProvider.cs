using System;
using System.Collections.Generic;
using System.Threading;
using CacheManager.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace EasyH.Extensions.Configuration.Db
{
    public class DbConfigurationProvider : IConfigurationProvider
    {
        private readonly DbConfigurationCacheFactory _cacheFactory;

        private readonly string _region;

        private DbConfigurationReloadToken _reloadToken = new DbConfigurationReloadToken();

        public DbConfigurationProvider(string configurationFileName = "cache.json",string region = "default")
        {
            if (string.IsNullOrWhiteSpace(_region))
            {
                _region = "default";
            }
            _region = region;
            _cacheFactory = new DbConfigurationCacheFactory(configurationFileName);
        }

        public bool TryGet(string key, out string value)
        {
            if (_cacheFactory.CacheManager.Exists(key))
            {
                value = _cacheFactory.CacheManager.Get(key, _region);
                return true;
            }

            value = string.Empty;
            return false;
        }

        public void Set(string key, string value)
        {
            var item = new CacheItem<string>(key, _region, value);
            _cacheFactory.CacheManager.AddOrUpdate(item, v => value);
        }

        public IChangeToken GetReloadToken()
        {
            return _reloadToken;
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            var prefix = parentPath == null ? string.Empty : parentPath + ConfigurationPath.KeyDelimiter;

            //return Data
            //    .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            //    .Select(kv => Segment(kv.Key, prefix.Length))
            //    .Concat(earlierKeys)
            //    .OrderBy(k => k, ConfigurationKeyComparer.Instance);
            return new string[0];
        }

        private static string Segment(string key, int prefixLength)
        {
            var indexOf = key.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
            return indexOf < 0 ? key.Substring(prefixLength) : key.Substring(prefixLength, indexOf - prefixLength);
        }

        /// <summary>
        /// Triggers the reload change token and creates a new one.
        /// </summary>
        protected void OnReload()
        {
            var previousToken = Interlocked.Exchange(ref _reloadToken, new DbConfigurationReloadToken());
            previousToken.OnReload();
        }
    }
}
