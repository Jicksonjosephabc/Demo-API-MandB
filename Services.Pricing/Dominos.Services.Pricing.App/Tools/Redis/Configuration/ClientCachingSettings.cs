using System;
using System.Collections.Generic;
using System.Text;

namespace Dominos.Services.Common.Tools.Redis.Configuration
{
    public class ClientCachingSettings
    {
        public string ObjectName
        {
            get; set;
        }

        public int TtlInSeconds
        {
            get; set;
        }

        public string KeyPrefix
        {
            get; set;
        }

        public bool Compress
        {
            get; set;
        }
    }
}
