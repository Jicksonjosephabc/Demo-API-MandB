using Dominos.Common.Rest.Api.Resources;
using Dominos.Common.Rest.Api.Resources.Prices;
using Dominos.OLO.Pricing.Service.Contract.v2_2.Data;

namespace Dominos.Services.Pricing.App.Services
{
    public interface IPricingService
    {
        PriceOrderResponse Price(PriceOrderRequest request);
        PricedProduct PriceProduct(PriceProductRequest request);
    }
}