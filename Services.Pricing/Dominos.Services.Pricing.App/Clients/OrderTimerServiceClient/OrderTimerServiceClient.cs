using System;
using System.Globalization;
using System.Net.Http;
using System.Web;
using Dominos.OLO.Pricing.Adaptor.Clients;
using Dominos.Services.Common.Tools.ServiceClient;
using Dominos.Services.Common.Tools.TelemetryEvents;
using Dominos.Services.OrderTimer.Api.v1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RequestOptions = Dominos.Services.Common.Tools.ServiceClient.RequestOptions;

namespace Dominos.Services.Pricing.App.Clients.OrderTimerServiceClient
{
    public class OrderTimerServiceClient : ServiceClientBase, IOrderTimerClient
    {
        private const string EstimatedTimeAndDistancePath = "api/v1/Estimation/DriveTimeAndDistance";

        public OrderTimerServiceClient(
            ILogger<OrderTimerServiceClient> logger, 
            IEmitter emitter, 
            HttpClient client,
            IOptions<OrderTimerServiceClientSettings> settings 
            )
            : base(logger, emitter, client, settings.Value)
        {
        }

        public EstimateDriveTimeAndDistanceResponse GetEstimatedTimeAndDistance(EstimateDriveTimeAndDistanceRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            var path = $"{EstimatedTimeAndDistancePath}?{EstimateDriveTimeAndDistanceRequestToQueryString(request)}";

            return PerformRequest<EstimateDriveTimeAndDistanceResponse>(path, HttpMethod.Get.ToString(), new RequestOptions { LogEvent = true }).GetAwaiter().GetResult();
        }

        private static string EstimateDriveTimeAndDistanceRequestToQueryString(EstimateDriveTimeAndDistanceRequest request)
        {
            var queryStringCollection = HttpUtility.ParseQueryString(string.Empty);
            
            queryStringCollection.Add(nameof(request.Country), request.Country);
            queryStringCollection.Add(nameof(request.OrderId), request.OrderId.ToString());
            queryStringCollection.Add($"{nameof(request.StoreGeoCoords)}.{nameof(request.StoreGeoCoords.Latitude)}", request.StoreGeoCoords.Latitude.ToString(CultureInfo.InvariantCulture));
            queryStringCollection.Add($"{nameof(request.StoreGeoCoords)}.{nameof(request.StoreGeoCoords.Longitude)}", request.StoreGeoCoords.Longitude.ToString(CultureInfo.InvariantCulture));
            queryStringCollection.Add($"{nameof(request.DeliverToDetails)}.{nameof(request.DeliverToDetails.StreetName)}", request.DeliverToDetails.StreetName);
            queryStringCollection.Add($"{nameof(request.DeliverToDetails)}.{nameof(request.DeliverToDetails.StreetNumber)}", request.DeliverToDetails.StreetNumber);
            queryStringCollection.Add($"{nameof(request.DeliverToDetails)}.{nameof(request.DeliverToDetails.Suburb)}", request.DeliverToDetails.Suburb);
            queryStringCollection.Add($"{nameof(request.DeliverToDetails)}.{nameof(request.DeliverToDetails.PostalCode)}", request.DeliverToDetails.PostalCode);
            queryStringCollection.Add($"{nameof(request.DeliverToDetails)}.{nameof(request.DeliverToDetails.UnitNumber)}", request.DeliverToDetails.UnitNumber);
            queryStringCollection.Add($"{nameof(request.DeliverToDetails)}.{nameof(request.DeliverToDetails.BuildingName)}", request.DeliverToDetails.BuildingName);
            queryStringCollection.Add($"{nameof(request.DeliverToDetails)}.{nameof(request.DeliverToDetails.State)}", request.DeliverToDetails.State);

            return queryStringCollection.ToString();
        }
    }
}
