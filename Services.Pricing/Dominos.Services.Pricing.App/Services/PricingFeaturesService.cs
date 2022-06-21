using System;
using Dominos.OLO.Pricing.Features;
using Dominos.Services.Common.Tools.FeatureToggles.Helpers;
using Microsoft.FeatureManagement;

namespace Dominos.Services.Pricing.App.Services
{
    public class PricingFeaturesService : IPricingFeaturesService
    {
        private readonly IFeatureManager _featureManager;
        
        public PricingFeaturesService(IFeatureManager featureManager)
        {
            _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        }

        public PricingFeatures GetFeatures(string userKey, string country, int? storeNo = null)
        {
            var targetingContext  = new TargetingContext{ CountryCode = country };
            var enhancedMinimumOrderValueFeatureValue = _featureManager.IsEnabledAsync(Constants.EnhancedMinimumOrderValueFeatureKey, targetingContext).GetAwaiter().GetResult();
            var enhancedMinimumOrderValuePerStoreFeatureValue = _featureManager.IsEnabledAsync(Constants.EnhancedMinimumOrderValuePerStoreFeatureKey, targetingContext).GetAwaiter().GetResult();

            return new PricingFeatures
            {
                EnhancedMinimumOrderValue = enhancedMinimumOrderValueFeatureValue,
                EnhancedMinimumOrderValuePerStore = enhancedMinimumOrderValuePerStoreFeatureValue
            };
        }
    }
}