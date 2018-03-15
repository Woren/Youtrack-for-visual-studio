﻿using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using YouTrackClientVS.Contracts.Interfaces.Services;
using YouTrackClientVS.Contracts.Models;
using YouTrackClientVS.Infrastructure.Extensions;

namespace YouTrackClientVS.Services
{
    [Export(typeof(ICacheService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        public void Add(string key, object value)
        {
            _cache.AddOrUpdate(key, value);
        }
        public Result<T> Get<T>(string key) where T : class
        {
            try
            {
                return _cache.ContainsKey(key) ? Result<T>.Success((T)_cache[key]) : Result<T>.Fail();
            }
            catch (Exception ex)
            {
                return Result<T>.Fail(ex);
            }
        }

        public void Delete(string key)
        {
            object res;
            if (_cache.ContainsKey(key))
                _cache.TryRemove(key, out res);
        }
    }
}
