using Dominos.Services.Common.Tools.Redis.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominos.Services.Common.Tools.Redis.Services
{
    public interface IRedisCachingService
    {
        Task<T> GetCacheValue<T>(string key, bool decompressCacheValue);

        Task<T> GetCacheValue<T>(string key, List<ClientCachingSettings> clientCachingSettings);

        Task<bool> SetCacheValue<T>(string key, T valueToCache, int ttlInSeconds, bool compressCacheValue);

        Task<bool> SetCacheValue<T>(string key, T valueToCache, List<ClientCachingSettings> clientCachingSettings);
    }
}
