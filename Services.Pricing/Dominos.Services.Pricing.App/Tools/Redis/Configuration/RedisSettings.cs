using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;


namespace Dominos.Services.Common.Tools.Redis.Configuration
{
    public class RedisSettings : RedisConfiguration
    {
        public bool InMemoryFallback
        {
            get; set;
        }
    }
}
