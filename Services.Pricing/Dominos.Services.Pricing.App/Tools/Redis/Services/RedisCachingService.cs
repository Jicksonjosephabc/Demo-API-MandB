using Dominos.Services.Common.Tools.Extensions;
using Dominos.Services.Common.Tools.Redis.Configuration;
using Dominos.Services.Common.Tools.Redis.Events;
using Dominos.Services.Common.Tools.Redis.Extensions;
using Dominos.Services.Common.Tools.TelemetryEvents;
using Dominos.Services.Common.Tools.TelemetryEvents.Implementations.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dominos.Services.Common.Tools.Redis.Services
{
    public class RedisCachingService : IRedisCachingService
    {
        private readonly IRedisClient redisCacheClient;
        private readonly IMemoryCache memoryCache;
        private readonly IEmitter emitter;
        private readonly RedisSettings redisSettings;

        public RedisCachingService(
              IEmitter emitter,
              IRedisClient redisCacheClient,
              IMemoryCache memoryCache,
              RedisSettings redisSettings)
        {
            this.memoryCache = memoryCache;
            this.redisSettings = redisSettings;
            this.redisCacheClient = redisCacheClient;
            this.emitter = emitter;
        }

        public async Task<T> GetCacheValue<T>(string key, bool decompressCacheValue = false)
        {
            var jsonCacheValue = default(string);
            try
            {
                jsonCacheValue = await redisCacheClient.Db0.GetAsync<string>(key);
            }
            catch (Exception ex)
            {
                if (redisSettings.InMemoryFallback)
                {
                    emitter.RaiseInfo(new InMemoryFallbackEvent(ex));
                    if (memoryCache.TryGetValue($"RedisCachingService:{key}", out string jsonMemoryCacheValue))
                    {
                        jsonCacheValue = jsonMemoryCacheValue;
                    }
                }
                else
                {
                    emitter.RaiseError(new FailedToCallRedisEvent(ex));
                    return default;
                }
            }

            if (string.IsNullOrEmpty(jsonCacheValue))
            {
                return default;
            }

            if (decompressCacheValue)
            {
                jsonCacheValue = jsonCacheValue.GZipStringDecompress();
            }

            return JsonConvert.DeserializeObject<T>(jsonCacheValue);
        }

        public async Task<T> GetCacheValue<T>(string key, List<ClientCachingSettings> clientCachingSettings)
        {
            var objectName = typeof(T).Name;
            var settings = clientCachingSettings.FirstOrDefault(s => s.ObjectName.Equals(objectName, StringComparison.InvariantCultureIgnoreCase));

            if (settings == null)
            {
                return default;
            }

            return await GetCacheValue<T>(CreateKey(settings.KeyPrefix, objectName, key), settings.Compress).ConfigureAwait(false);
        }

        public async Task<bool> SetCacheValue<T>(string key, T valueToCache, List<ClientCachingSettings> clientCachingSettings)
        {
            var objectName = typeof(T).Name;
            var settings = clientCachingSettings.FirstOrDefault(s => s.ObjectName.Equals(objectName, StringComparison.InvariantCultureIgnoreCase));

            if (settings == null)
            {
                return false;
            }

            return await SetCacheValue<T>(CreateKey(settings.KeyPrefix, objectName, key), valueToCache, settings.TtlInSeconds, settings.Compress).ConfigureAwait(false);
        }

        private string CreateKey(string keyPrefix, string objectName, string key)
        {
            key = $"{objectName}::{key}";

            if (!string.IsNullOrEmpty(keyPrefix))
            {
                key = $"{keyPrefix}:{key}";
            }

            return key;
        }

        public async Task<bool> SetCacheValue<T>(string key, T valueToCache, int ttlInSeconds, bool compressCacheValue = false)
        {
            if (valueToCache == null)
            {
                return false;
            }

            var jsonCacheValue = valueToCache.ToJson();

            if (compressCacheValue)
            {
                jsonCacheValue = jsonCacheValue.GZipStringCompress();
            }

            try
            {
                return await redisCacheClient.Db0.AddAsync(key, jsonCacheValue, DateTimeOffset.Now.AddSeconds(ttlInSeconds)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (redisSettings.InMemoryFallback)
                {
                    emitter.RaiseInfo(new InMemoryFallbackEvent(ex));
                    memoryCache.Set($"RedisCachingService:{key}", jsonCacheValue, DateTimeOffset.Now.AddSeconds(ttlInSeconds));
                    return true;
                }

                emitter.RaiseError(new FailedToCallRedisEvent(ex));
                return false;
            }
        }
    }
}
