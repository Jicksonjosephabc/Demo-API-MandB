using Dominos.Services.Common.Tools.Redis.Configuration;
using Dominos.Services.Common.Tools.ServiceClient;
using System.Collections.Generic;

namespace Dominos.Services.Pricing.App.Clients.CouponsServiceClient
{
    public class CouponsServiceClientSettings : ClientSettings
    {
        public List<ClientCachingSettings> Caching
        {
            get; set;
        }
    }
}
