using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Dominos.Services.Common.Tools.FeatureToggles.Helpers;
using Dominos.Services.Features.Api.v1.Features;
using Dominos.Services.Pricing.App.Services;
using Microsoft.FeatureManagement;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Dominos.Services.Pricing.Tests.Internal.Unit.Services
{
    public class PricingFeaturesServiceTests
    {
        private readonly Fixture _fixture;

        private readonly IFeatureManager _featureManager;

        private readonly PricingFeaturesService _pricingFeaturesService;

        public PricingFeaturesServiceTests()
        {
            _fixture = new Fixture();

            _featureManager = Substitute.For<IFeatureManager>();

            _pricingFeaturesService = new PricingFeaturesService(_featureManager);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void GetFeatures_ReturnsPricingFeatures(bool enhancedMinimumOrderValue, bool enhancedMinimumOrderValuePerStore)
        {
            // Arrange
            var countryCode = _fixture.Create<string>();

            _featureManager.IsEnabledAsync("EnhancedMinimumOrderValue", Arg.Is<TargetingContext>(tc => tc.CountryCode == countryCode)).Returns(enhancedMinimumOrderValue);
            _featureManager.IsEnabledAsync("EnhancedMinimumOrderValuePerStore", Arg.Is<TargetingContext>(tc => tc.CountryCode == countryCode)).Returns(enhancedMinimumOrderValuePerStore);

            // Act
            var result = _pricingFeaturesService.GetFeatures(_fixture.Create<string>(), countryCode);

            // Assert
            result.EnhancedMinimumOrderValue.ShouldBe(enhancedMinimumOrderValue);
            result.EnhancedMinimumOrderValuePerStore.ShouldBe(enhancedMinimumOrderValuePerStore);
        }
    }
}
