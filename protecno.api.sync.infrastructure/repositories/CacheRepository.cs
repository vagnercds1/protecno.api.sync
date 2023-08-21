using protecno.api.sync.domain.interfaces.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace protecno.api.sync.infrastructure.repositories
{
    public class CacheRepository : ICacheRepository
    {
        private readonly MemoryCache _cache = MemoryCache.Default;

        public string GetKeyInMemory(string key)
        {
            var result = _cache.Get(key);

            return result == null ? "" : result.ToString();
        }

        public void SetKeyInMemory(string key, string value, int minutesTTL)
        {
            if (_cache.Contains(key))
                _cache.Remove(key);

            var policy = new CacheItemPolicy();
            policy.SlidingExpiration = TimeSpan.FromMinutes(minutesTTL);
            _cache.Add(key, value, policy);
        }

        public void RemoveKeysInMemoryCacheByPartKey(string partKey)
        {
            List<string> listKeys = _cache.Select(item => item.Key)
                                               .Where(chave => chave.Contains(partKey))
                                               .ToList();

            foreach (var chave in listKeys)
                _cache.Remove(chave);
        }
    }
}
