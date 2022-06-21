using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Dominos.Services.Common.Tools.ServiceClient;
using Dominos.Services.Common.Tools.TelemetryEvents;
using Dominos.Services.Features.Api.v1.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dominos.Services.Pricing.App.Clients.FeaturesServiceClient
{
    public class FeaturesServiceClient : ServiceClientBase, IFeaturesServiceClient
    {
        private const string GetFeaturesPath = "/api/Features";

        public FeaturesServiceClient(
            ILogger<FeaturesServiceClient> logger,
            IEmitter emitter,
            HttpClient client,
            IOptions<FeaturesServiceClientSettings> settings
            )
            : base(logger, emitter, client, settings.Value)
        {
        }

        public Task<GetFeaturesResponse> GetFeaturesAsync(GetFeaturesRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            var path = $"{GetFeaturesPath}?{GetFeaturesRequestToQueryString(request)}";

            return PerformRequest<GetFeaturesResponse>(path, HttpMethod.Get.ToString(), new RequestOptions { LogEvent = true });
        }

        private static string GetFeaturesRequestToQueryString(GetFeaturesRequest request)
        {
            var queryStringCollection = HttpUtility.ParseQueryString(string.Empty);
            foreach (var (key, value) in request.ToDictionary())
            {
                queryStringCollection.Add(key, value);
            }

            return queryStringCollection.ToString();
        }
    }
}
