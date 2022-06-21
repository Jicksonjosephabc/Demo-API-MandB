using Dominos.Common.Rest.Api.Resources.Prices;
using Dominos.OLO.Common.Service.Contract.v2_2.Data;
using Dominos.OLO.Pricing.Service.Contract.v2_2.Data;
using Dominos.Services.Pricing.App.Configuration;
using Dominos.Services.Pricing.App.Helpers;
using Microsoft.Extensions.Options;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Configuration;
using Xunit;
using HalfNHalfPricingBehaviour = Dominos.OLO.Pricing.Service.Contract.v2_2.Data.HalfNHalfPricingBehaviour;
using HalfNHalfPricingBehaviourProduct = Dominos.Common.Rest.Api.Resources.Prices.HalfNHalfPricingBehaviour;

namespace Dominos.Services.Pricing.Tests.Internal.Unit.Helpers
{
    public class PricingSettingsHelperTests
    {
        private const int MAXIMUM_VOUCHER_COUNT = 6;
        private const int TIMEOUT_SECONDS = 5;

        private static IPricingSettingsHelper _pricingSettingsHelper;
        
        public PricingSettingsHelperTests()
        {
            var settings = new PricingSettings
            { 
                MaximumVoucherCount = MAXIMUM_VOUCHER_COUNT,
                Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS),
                HalfNHalfPricingSettings = new List<HalfNHalfPricingSettings>
                {
                    new HalfNHalfPricingSettings {CountryCode = "FR", HalfNHalfPricingBehaviour = "MostExpensive"},
                    new HalfNHalfPricingSettings {CountryCode = "DE", HalfNHalfPricingBehaviour = "OwnPricePlusAvgDiffToBase"},
                    new HalfNHalfPricingSettings {CountryCode = "NL", HalfNHalfPricingBehaviour = "OwnPricePlusAvgDiffToBase"},
                    new HalfNHalfPricingSettings {CountryCode = "BE", HalfNHalfPricingBehaviour = "OwnPricePlusAvgDiffToBase"},
                    new HalfNHalfPricingSettings {CountryCode = "LU", HalfNHalfPricingBehaviour = "OwnPricePlusAvgDiffToBase"},
                    new HalfNHalfPricingSettings {CountryCode = "DK", HalfNHalfPricingBehaviour = "OwnPricePlusAvgDiffToBase"},
                    new HalfNHalfPricingSettings {CountryCode = "AU", HalfNHalfPricingBehaviour = "AveragePrice"},
                    new HalfNHalfPricingSettings {CountryCode = "NZ", HalfNHalfPricingBehaviour = "AveragePrice"},
                    new HalfNHalfPricingSettings {CountryCode = "JP", HalfNHalfPricingBehaviour = "AveragePrice"},
                    new HalfNHalfPricingSettings {CountryCode = "test", HalfNHalfPricingBehaviour = "invalid"}
                }
            };

            _pricingSettingsHelper = new PricingSettingsHelper(Options.Create(settings));
        }

        [Fact]
        public void AddHalfNHalfPricingBehaviourToOrder_ShouldReturnRequestUnchangedIfCountryIsNull()
        {
            // Arrange
            var request = new PriceOrderRequest
            {
                RequestKey = new RequestKey
                {
                    Culture = "en",
                    Application = "web"
                },
                Order = new Order
                {
                    StoreNo = 98415,
                    OrderId = Guid.NewGuid()
                }
            };

            // Act
            var response = _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(request);

            // Assert
            response.ShouldBe(request);
            response.Settings.ShouldBeNull();
        }

        [Fact]
        public void AddHalfNHalfPricingBehaviourToOrder_ShouldReturnNullIfRequestIsNull()
        {
            // Arrange

            // Act
            var response = _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(null);

            // Assert
            response.ShouldBeNull();
        }

        [Theory]
        [InlineData("FR", HalfNHalfPricingBehaviour.MostExpensive)]
        [InlineData("DE", HalfNHalfPricingBehaviour.OwnPricePlusAvgDiffToBase)]
        [InlineData("NL", HalfNHalfPricingBehaviour.OwnPricePlusAvgDiffToBase)]
        [InlineData("BE", HalfNHalfPricingBehaviour.OwnPricePlusAvgDiffToBase)]
        [InlineData("LU", HalfNHalfPricingBehaviour.OwnPricePlusAvgDiffToBase)]
        [InlineData("DK", HalfNHalfPricingBehaviour.OwnPricePlusAvgDiffToBase)]
        [InlineData("AU", HalfNHalfPricingBehaviour.AveragePrice)]
        [InlineData("NZ", HalfNHalfPricingBehaviour.AveragePrice)]
        [InlineData("test", HalfNHalfPricingBehaviour.AveragePrice)]
        public void AddHalfNHalfPricingBehaviourToOrder_ShouldAddCorrectSettingToRequest(string country, HalfNHalfPricingBehaviour expectedBehaviour)
        {
            // Arrange
            var request = new PriceOrderRequest
            {
                RequestKey = new RequestKey
                {
                    Culture = "en",
                    Application = "web",
                    Country = country
                },
                Order = new Order
                {
                    StoreNo = 98415,
                    OrderId = Guid.NewGuid()
                }
            };

            // Act
            var response = _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(request);

            // Assert
            response.Settings.ShouldNotBeNull();
            response.Settings.HalfNHalfPricingBehaviour.ShouldBe(expectedBehaviour);
        }

        [Fact]
        public void AddHalfNHalfPricingBehaviourToProduct_ShouldReturnRequestUnchangedIfCountryIsNull()
        {
            // Arrange
            var request = new PriceProductRequest
            {
                CountryCode = null
            };

            // Act
            var response = _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToProduct(request);

            // Assert
            response.ShouldBe(request);
            response.Settings.ShouldBeNull();
        }

        [Fact]
        public void AddHalfNHalfPricingBehaviourToProduct_ShouldReturnNullIfRequestIsNull()
        {
            // Arrange

            // Act
            var response = _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToProduct(null);

            // Assert
            response.ShouldBeNull();
        }

        [Theory]
        [InlineData("fr", HalfNHalfPricingBehaviourProduct.MostExpensive)]
        [InlineData("DE", HalfNHalfPricingBehaviourProduct.OwnPricePlusAvgDiffToBase)]
        [InlineData("NL", HalfNHalfPricingBehaviourProduct.OwnPricePlusAvgDiffToBase)]
        [InlineData("BE", HalfNHalfPricingBehaviourProduct.OwnPricePlusAvgDiffToBase)]
        [InlineData("LU", HalfNHalfPricingBehaviourProduct.OwnPricePlusAvgDiffToBase)]
        [InlineData("DK", HalfNHalfPricingBehaviourProduct.OwnPricePlusAvgDiffToBase)]
        [InlineData("AU", HalfNHalfPricingBehaviourProduct.AveragePrice)]
        [InlineData("NZ", HalfNHalfPricingBehaviourProduct.AveragePrice)]
        [InlineData("test", HalfNHalfPricingBehaviourProduct.AveragePrice)]
        public void AddHalfNHalfPricingBehaviourToProduct_ShouldAddCorrectSettingToRequest(string country, HalfNHalfPricingBehaviourProduct expectedBehaviour)
        {
            // Arrange
            var request = new PriceProductRequest
            {
                CountryCode = country
            };

            // Act
            var response = _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToProduct(request);

            // Assert
            response.Settings.ShouldNotBeNull();
            response.Settings.HalfNHalfPricingBehaviour.ShouldBe(expectedBehaviour);
        }

        [Fact]
        public void GetMaximumVoucherCount_NotSpecified_ShowThrowException()
        {
            _pricingSettingsHelper = new PricingSettingsHelper(Options.Create((PricingSettings)null));

            Should.Throw<ConfigurationErrorsException>(() => _pricingSettingsHelper.GetMaximumVoucherCount())
                .Message.ShouldBe($"{nameof(PricingSettings)}.{nameof(PricingSettings.MaximumVoucherCount)} has not been defined in the settings.");
        }

        [Fact]
        public void GetMaximumVoucherCount_ValidSpecified_ShowReturnValue()
        {
            var result = _pricingSettingsHelper.GetMaximumVoucherCount();

            result.ShouldBe(MAXIMUM_VOUCHER_COUNT);
        }

        [Fact]
        public void GetTimeout_NotSpecified_ShowThrowException()
        {
            _pricingSettingsHelper = new PricingSettingsHelper(Options.Create((PricingSettings)null));

            Should.Throw<ConfigurationErrorsException>(() => _pricingSettingsHelper.GetTimeout())
                .Message.ShouldBe($"{nameof(PricingSettings)}.{nameof(PricingSettings.Timeout)} has not been defined in the settings.");
        }

        [Fact]
        public void GetTimeout_ValidSpecified_ShowReturnValue()
        {
            var result = _pricingSettingsHelper.GetTimeout();

            result.ShouldBe(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }
    }
}
