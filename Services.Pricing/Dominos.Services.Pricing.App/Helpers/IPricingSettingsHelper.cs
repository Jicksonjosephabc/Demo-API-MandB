using System;
using Dominos.Common.Rest.Api.Resources.Prices;
using Dominos.OLO.Pricing.Service.Contract.v2_2.Data;

namespace Dominos.Services.Pricing.App.Helpers
{
    public interface IPricingSettingsHelper
    {
        PriceOrderRequest AddHalfNHalfPricingBehaviourToOrder(PriceOrderRequest priceOrderRequest);
        PriceProductRequest AddHalfNHalfPricingBehaviourToProduct(PriceProductRequest priceProductRequest);
        int GetMaximumVoucherCount();
        TimeSpan GetTimeout();
    }
}