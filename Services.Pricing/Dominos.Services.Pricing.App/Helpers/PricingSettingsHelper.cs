using System;
using System.Configuration;
using System.Linq;
using Dominos.Common.Rest.Api.Resources.Prices;
using Dominos.OLO.Pricing.Service.Contract.v2_2.Data;
using Dominos.Services.Pricing.App.Configuration;
using Microsoft.Extensions.Options;

// TODO: should eventually consolidate these types into new api version so we don't have to duplicate logic
using HalfNHalfPricingBehaviour = Dominos.OLO.Pricing.Service.Contract.v2_2.Data.HalfNHalfPricingBehaviour;
using ProductPricingSettingsOrder = Dominos.OLO.Pricing.Service.Contract.v2_2.Data.ProductPricingSettings;
using ProductPricingSettingsProduct = Dominos.Common.Rest.Api.Resources.Prices.ProductPricingSettings;
using HalfNHalfPricingBehaviourProduct = Dominos.Common.Rest.Api.Resources.Prices.HalfNHalfPricingBehaviour;

namespace Dominos.Services.Pricing.App.Helpers
{
    public class PricingSettingsHelper : IPricingSettingsHelper
    {
        // see card https://dominos-au.visualstudio.com/OneDigital/_workitems/edit/93479
        
        private readonly PricingSettings _pricingSettings;

        public PricingSettingsHelper(IOptions<PricingSettings> pricingSettings)
        {
            _pricingSettings = pricingSettings.Value;
        }

        public PriceOrderRequest AddHalfNHalfPricingBehaviourToOrder(PriceOrderRequest priceOrderRequest)
        {
            var setting = GetSettingForCountry(priceOrderRequest?.RequestKey?.Country);
            if (priceOrderRequest == null || setting == null) return priceOrderRequest;

            if (priceOrderRequest.Settings == null) priceOrderRequest.Settings = new ProductPricingSettingsOrder();
            priceOrderRequest.Settings.HalfNHalfPricingBehaviour = GetBehaviourForSetting(setting);
            return priceOrderRequest;
        }

        public PriceProductRequest AddHalfNHalfPricingBehaviourToProduct(PriceProductRequest priceProductRequest)
        {
            var setting = GetSettingForCountry(priceProductRequest?.CountryCode);
            if (priceProductRequest == null || setting == null) return priceProductRequest;

            if (priceProductRequest.Settings == null) priceProductRequest.Settings = new ProductPricingSettingsProduct();
            priceProductRequest.Settings.HalfNHalfPricingBehaviour = (HalfNHalfPricingBehaviourProduct)GetBehaviourForSetting(setting);
            return priceProductRequest;
        }

        public int GetMaximumVoucherCount()
        {
            return _pricingSettings?.MaximumVoucherCount ?? throw new ConfigurationErrorsException($"{nameof(PricingSettings)}.{nameof(PricingSettings.MaximumVoucherCount)} has not been defined in the settings.");
        }

        public TimeSpan GetTimeout()
        {
            return _pricingSettings?.Timeout ?? throw new ConfigurationErrorsException($"{nameof(PricingSettings)}.{nameof(PricingSettings.Timeout)} has not been defined in the settings.");
        }

        private HalfNHalfPricingBehaviour GetBehaviourForSetting(string setting)
        {
            switch (setting)
            {
                case "MostExpensive":
                    return HalfNHalfPricingBehaviour.MostExpensive;
                case "OwnPrice":
                    return HalfNHalfPricingBehaviour.OwnPrice;
                case "OwnPricePlusAvgDiffToBase":
                    return HalfNHalfPricingBehaviour.OwnPricePlusAvgDiffToBase;
                case "AveragePrice":
                default:
                    return HalfNHalfPricingBehaviour.AveragePrice;
            }
        }

        private string GetSettingForCountry(string countryCode)
        {
            if (countryCode == null) return null;

            return _pricingSettings.HalfNHalfPricingSettings.FirstOrDefault(setting =>
                string.Equals(setting.CountryCode, countryCode, StringComparison.InvariantCultureIgnoreCase))?.HalfNHalfPricingBehaviour;
        }
    }
}
