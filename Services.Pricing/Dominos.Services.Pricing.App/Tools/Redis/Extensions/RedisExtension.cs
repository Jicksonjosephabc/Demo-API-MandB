using Dominos.Services.Common.Tools.Redis.Configuration;
using Dominos.Services.Common.Tools.Redis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;
using System;

namespace Dominos.Services.Common.Tools.Redis.Extensions
{
    public static class RedisExtension
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var configSettings = configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>();
            configSettings.PoolSize = configSettings.PoolSize == 0 ? Environment.ProcessorCount : configSettings.PoolSize;

            services
                .AddSingleton((RedisConfiguration)configSettings)
                .AddSingleton(configSettings);

            services
                .AddMemoryCache()
                .AddSingleton<IRedisClient, RedisClient>()
                .AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>()
                .AddSingleton<ISerializer, NewtonsoftSerializer>()
                .AddSingleton<IRedisCachingService, RedisCachingService>()
                .AddSingleton((provider) => { return provider.GetRequiredService<IRedisClient>().GetDefaultDatabase(); });

            return services;
        }
    }
}
