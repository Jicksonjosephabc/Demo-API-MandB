using System;
using System.Collections.Generic;
using AutoFixture;
using Dominos.Common.Events.Infrastructure.Interfaces;
using Dominos.Common.Rest.Adaptor.Stores;
using Dominos.Common.Rest.Api.Resources;
using Dominos.Common.Rest.Api.Resources.Prices;
using Dominos.Common.Rest.Api.Resources.Stores;
using Dominos.OLO.Common.Service.Contract.v2_2.Data;
using Dominos.OLO.Pricing.Adaptor.Clients;
using Dominos.OLO.Pricing.Features;
using Dominos.OLO.Pricing.Logging.Events;
using Dominos.OLO.Pricing.Service.Contract.v2_2.Data;
using Dominos.OLO.Stores.Api.StorePrices.Models;
using Dominos.Services.Common.Tools.TelemetryEvents;
using Dominos.Services.Common.Tools.TelemetryEvents.EventTypes;
using Dominos.Services.Common.Tools.TelemetryEvents.Implementations.ApplicationInsights;
using Dominos.Services.Coupons.Api.v1.Coupon;
using Dominos.Services.OrderTimer.Api.v1;
using Dominos.Services.Pricing.App.Clients.StoreMenuServiceClient;
using Dominos.Services.Pricing.App.Helpers;
using Dominos.Services.Pricing.App.Services;
using NSubstitute;
using Shouldly;
using Xunit;
using Order = Dominos.OLO.Pricing.Service.Contract.v2_2.Data.Order;

namespace Dominos.Services.Pricing.Tests.Internal.Unit.Services
{
    public class PricingServiceTests
    {
        private const string TooManyVouchersMessageEventId = "1018";

        private readonly ICouponQueries _couponClient;
        private readonly IStoreMenuServiceClient _storeMenuServiceClient;
        private readonly IPricingFeaturesService _pricingFeaturesService;
        private readonly IEvents _legacyLogger;
        private readonly IEmitter _emitter;
        private readonly IOrderTimerClient _orderTimerClient;
        private readonly PricingService _pricingService;
        private readonly Fixture _fixture;
        private readonly Store _store;
        private readonly IPricingSettingsHelper _pricingSettingsHelper;
        
        public PricingServiceTests()
        {
            _fixture = new Fixture();
            _store = _fixture.Create<Store>();
            
            _couponClient = Substitute.For<ICouponQueries>();
            _couponClient.GetAllAutoCoupons(Arg.Any<GetAutoDiscountRequest>()).Returns(new List<Coupon>());

            _storeMenuServiceClient = Substitute.For<IStoreMenuServiceClient>();
            _storeMenuServiceClient.Get<GetStoreResponse>(ResourceNames.StoresWithLanguage, Arg.Any<GetStoreRequest>())
                .Returns(new GetStoreResponse
                {
                    Store = _store
                });

            _pricingFeaturesService = Substitute.For<IPricingFeaturesService>();
            _legacyLogger = Substitute.For<IEvents>();
            _emitter = Substitute.For<IEmitter>();
            _orderTimerClient = Substitute.For<IOrderTimerClient>();
            _pricingSettingsHelper = Substitute.For<IPricingSettingsHelper>();
            _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(Arg.Any<PriceOrderRequest>())
                .Returns(callInfo => callInfo.ArgAt<PriceOrderRequest>(0));
            _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToProduct(Arg.Any<PriceProductRequest>())
                .Returns(callInfo => callInfo.ArgAt<PriceProductRequest>(0));

            _pricingService = new PricingService(_emitter, _couponClient, _storeMenuServiceClient, _pricingFeaturesService, _legacyLogger, _orderTimerClient, _pricingSettingsHelper);
        }

        [Fact]
        public void Price_ShouldCalculateEnhancedMinimumOrderValue()
        {
            // Arrange
            var request = new PriceOrderRequest
            {
                RequestKey = new RequestKey
                {
                    Country = "DE", // Only DE has ring configuration
                    Culture = "en",
                    Application = _fixture.Create<string>()
                },
                Order = new Order
                {
                    StoreNo = _store.StoreNo,
                    OrderId = _fixture.Create<Guid>(),
                    OrderDate = new DateTime(2022, 2, 2 , 18, 0, 0, DateTimeKind.Utc),
                    ServiceMethod = "Delivery",
                    DeliveryAddress = _fixture.Create<DeliveryAddress>(),
                    CustomerDetails = _fixture.Create<CustomerDetails>()
                }
            };

            _pricingFeaturesService.GetFeatures(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new PricingFeatures
                {
                    EnhancedMinimumOrderValue = true
                });

            _storeMenuServiceClient.GetStoreDeliveryOrderMinimumValues(Arg.Any<GetStoreDeliveryOrderMinimumValuesRequest>())
                .Returns(_fixture.Build<GetStoreDeliveryOrderMinimumValuesResponse>().With(d => d.DeliveryMinimumValueRings, new[]
                    {
                        new DeliveryMinimumValueRing
                        {
                            RingNumber = 0,
                            DeliveryMinimumValue = 1
                        }
                    }).Create()
                );

            _orderTimerClient.GetEstimatedTimeAndDistance(Arg.Any<EstimateDriveTimeAndDistanceRequest>()).Returns(
                new EstimateDriveTimeAndDistanceResponse
                {
                    CalculatedDistanceInMeters = 10,
                    Success = true
                });

            // Act
            var result = _pricingService.Price(request);

            // Assert
            _pricingFeaturesService.Received().GetFeatures(
                Arg.Any<string>(),
                Arg.Is<string>(c => c == request.RequestKey.Country),
                Arg.Is<int>(s => s == _store.StoreNo));

            _storeMenuServiceClient.Received().GetStoreDeliveryOrderMinimumValues(
                Arg.Is<GetStoreDeliveryOrderMinimumValuesRequest> (r => r.StoreNo == _store.StoreNo)
                );

            _legacyLogger.Received().Raise<EnhancedMinimumOrderValueCalculationEvent>(Arg.Any<object>());

            _orderTimerClient.Received().GetEstimatedTimeAndDistance(
                Arg.Is<EstimateDriveTimeAndDistanceRequest>(r => r.DeliverToDetails.StreetName == request.Order.DeliveryAddress.StreetName));

            result.ShouldNotBeNull();
            result.Order.ShouldNotBeNull();
            result.Order.MinimumOrderPrice.ShouldBe(1);
        }

        [Fact]
        public void Price_ShouldCallAddHalfNHalfPricingBehaviourToOrder()
        {
            // Arrange
            var request = new PriceOrderRequest
            {
                RequestKey = new RequestKey
                {
                    Country = "DE",
                    Culture = "en",
                    Application = _fixture.Create<string>()
                },
                Order = new Order
                {
                    StoreNo = _store.StoreNo,
                    OrderId = _fixture.Create<Guid>(),
                    OrderDate = new DateTime(2022, 2, 2, 18, 0, 0, DateTimeKind.Utc),
                    ServiceMethod = "Delivery",
                    DeliveryAddress = _fixture.Create<DeliveryAddress>(),
                    CustomerDetails = _fixture.Create<CustomerDetails>()
                }
            };

            _pricingFeaturesService.GetFeatures(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new PricingFeatures
                {
                    EnhancedMinimumOrderValue = true
                });

            _storeMenuServiceClient.GetStoreDeliveryOrderMinimumValues(Arg.Any<GetStoreDeliveryOrderMinimumValuesRequest>())
                .Returns(_fixture.Build<GetStoreDeliveryOrderMinimumValuesResponse>().With(d => d.DeliveryMinimumValueRings, new[]
                    {
                        new DeliveryMinimumValueRing
                        {
                            RingNumber = 0,
                            DeliveryMinimumValue = 1
                        }
                    }).Create()
                );

            _orderTimerClient.GetEstimatedTimeAndDistance(Arg.Any<EstimateDriveTimeAndDistanceRequest>()).Returns(
                new EstimateDriveTimeAndDistanceResponse
                {
                    CalculatedDistanceInMeters = 10,
                    Success = true
                });

            // Act
            _pricingService.Price(request);

            // Assert
            _pricingSettingsHelper.Received(1).AddHalfNHalfPricingBehaviourToOrder(request);
        }

        [Fact]
        public void PriceProduct_ShouldCallAddHalfNHalfPricingBehaviourToProduct()
        {
            // Arrange
            var request = new PriceProductRequest();

            _pricingFeaturesService.GetFeatures(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new PricingFeatures
                {
                    EnhancedMinimumOrderValue = true
                });

            _storeMenuServiceClient.GetStoreDeliveryOrderMinimumValues(Arg.Any<GetStoreDeliveryOrderMinimumValuesRequest>())
                .Returns(_fixture.Build<GetStoreDeliveryOrderMinimumValuesResponse>().With(d => d.DeliveryMinimumValueRings, new[]
                    {
                        new DeliveryMinimumValueRing
                        {
                            RingNumber = 0,
                            DeliveryMinimumValue = 1
                        }
                    }).Create()
                );

            _orderTimerClient.GetEstimatedTimeAndDistance(Arg.Any<EstimateDriveTimeAndDistanceRequest>()).Returns(
                new EstimateDriveTimeAndDistanceResponse
                {
                    CalculatedDistanceInMeters = 10,
                    Success = true
                });

            // Act
            _pricingService.PriceProduct(request);

            // Assert
            _pricingSettingsHelper.Received(1).AddHalfNHalfPricingBehaviourToProduct(request);
        }
        
        [Fact]
        public void Price_VoucherCheck_NoVouchers_ShouldReturnPricedOrder()
        {
            var request = new PriceOrderRequest
            {
                RequestKey = new RequestKey
                {
                    Country = "AU",
                    Culture = "en",
                    Application = _fixture.Create<string>()
                },
                Order = new Order
                {
                    StoreNo = _store.StoreNo,
                    OrderId = _fixture.Create<Guid>(),
                    OrderDate = new DateTime(2022, 2, 2, 18, 0, 0, DateTimeKind.Utc),
                    ServiceMethod = "Delivery",
                    DeliveryAddress = _fixture.Create<DeliveryAddress>(),
                    CustomerDetails = _fixture.Create<CustomerDetails>(),
                    Vouchers = null,
                }
            };

            _pricingFeaturesService.GetFeatures(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new PricingFeatures
                {
                    EnhancedMinimumOrderValue = false
                });

            _pricingSettingsHelper.GetMaximumVoucherCount()
                .Returns(callInfo => 6);

            _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(Arg.Any<PriceOrderRequest>())
                .Returns(callInfo => callInfo.ArgAt<PriceOrderRequest>(0));

            var result = _pricingService.Price(request);
            result.ShouldNotBeNull();
            result.Order.ShouldNotBeNull();
            result.Order.Vouchers.ShouldNotBeNull();
            result.Messages.ShouldNotContain(m => m.Code == TooManyVouchersMessageEventId);
        }

        [Fact]
        public void Price_VoucherCheck_OneVoucher_SShouldReturnPricedOrder()
        {
            var request = new PriceOrderRequest
            {
                RequestKey = new RequestKey
                {
                    Country = "AU",
                    Culture = "en",
                    Application = _fixture.Create<string>()
                },
                Order = new Order
                {
                    StoreNo = _store.StoreNo,
                    OrderId = _fixture.Create<Guid>(),
                    OrderDate = new DateTime(2022, 2, 2, 18, 0, 0, DateTimeKind.Utc),
                    ServiceMethod = "Delivery",
                    DeliveryAddress = _fixture.Create<DeliveryAddress>(),
                    CustomerDetails = _fixture.Create<CustomerDetails>(),
                    Vouchers = new List<OrderVoucher>() {
                        { new OrderVoucher() { VoucherCode = "123" } },
                    },
                }
            };

            _pricingFeaturesService.GetFeatures(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new PricingFeatures
                {
                    EnhancedMinimumOrderValue = false
                });

            _pricingSettingsHelper.GetMaximumVoucherCount()
                .Returns(callInfo => 6);

            _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(Arg.Any<PriceOrderRequest>())
                .Returns(callInfo => callInfo.ArgAt<PriceOrderRequest>(0));

            var result = _pricingService.Price(request);
            result.ShouldNotBeNull();
            result.Order.ShouldNotBeNull();
            result.Order.Vouchers.ShouldNotBeNull();
            result.Messages.ShouldNotContain(m => m.Code == TooManyVouchersMessageEventId);
        }

        [Fact]
        public void Price_VoucherCheck_TwoVouchers_ShouldReturnPricedOrder()
        {
            var request = new PriceOrderRequest
            {
                RequestKey = new RequestKey
                {
                    Country = "AU",
                    Culture = "en",
                    Application = _fixture.Create<string>()
                },
                Order = new Order
                {
                    StoreNo = _store.StoreNo,
                    OrderId = _fixture.Create<Guid>(),
                    OrderDate = new DateTime(2022, 2, 2, 18, 0, 0, DateTimeKind.Utc),
                    ServiceMethod = "Delivery",
                    DeliveryAddress = _fixture.Create<DeliveryAddress>(),
                    CustomerDetails = _fixture.Create<CustomerDetails>(),
                    Vouchers = new List<OrderVoucher>() {
                        { new OrderVoucher() { VoucherCode = "123" } },
                        { new OrderVoucher() { VoucherCode = "456" } },
                    },
                }
            };

            _pricingFeaturesService.GetFeatures(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new PricingFeatures
                {
                    EnhancedMinimumOrderValue = false
                });

            _pricingSettingsHelper.GetMaximumVoucherCount()
                .Returns(callInfo => 6);

            _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(Arg.Any<PriceOrderRequest>())
                .Returns(callInfo => callInfo.ArgAt<PriceOrderRequest>(0));

            var result = _pricingService.Price(request);
            result.ShouldNotBeNull();
            result.Order.ShouldNotBeNull();
            result.Order.Vouchers.ShouldNotBeNull();
            result.Messages.ShouldNotContain(m => m.Code == TooManyVouchersMessageEventId);
        }

        [Fact]
        public void Price_VoucherCheck_MaxVouchers_ShouldReturnPricedOrder()
        {
            var request = new PriceOrderRequest
            {
                RequestKey = new RequestKey
                {
                    Country = "AU",
                    Culture = "en",
                    Application = _fixture.Create<string>()
                },
                Order = new Order
                {
                    StoreNo = _store.StoreNo,
                    OrderId = _fixture.Create<Guid>(),
                    OrderDate = new DateTime(2022, 2, 2, 18, 0, 0, DateTimeKind.Utc),
                    ServiceMethod = "Delivery",
                    DeliveryAddress = _fixture.Create<DeliveryAddress>(),
                    CustomerDetails = _fixture.Create<CustomerDetails>(),
                    Vouchers = new List<OrderVoucher>() {
                        { new OrderVoucher() { VoucherCode = "123" } },
                        { new OrderVoucher() { VoucherCode = "456" } },
                        { new OrderVoucher() { VoucherCode = "789" } },
                        { new OrderVoucher() { VoucherCode = "ABC" } },
                        { new OrderVoucher() { VoucherCode = "DEF" } },
                        { new OrderVoucher() { VoucherCode = "GHI" } },
                    },
                }
            };

            _pricingFeaturesService.GetFeatures(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new PricingFeatures
                {
                    EnhancedMinimumOrderValue = false
                });

            _pricingSettingsHelper.GetMaximumVoucherCount()
                .Returns(callInfo => 6);

            _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(Arg.Any<PriceOrderRequest>())
                .Returns(callInfo => callInfo.ArgAt<PriceOrderRequest>(0));

            var result = _pricingService.Price(request);
            result.ShouldNotBeNull();
            result.Order.ShouldNotBeNull();
            result.Order.Vouchers.ShouldNotBeNull();
            result.Messages.ShouldNotContain(m => m.Code == TooManyVouchersMessageEventId);
        }

        [Fact]
        public void Price_VoucherCheck_ExceedsMaxVouchers_ShouldReturnAlteredPricedOrder()
        {
            var request = new PriceOrderRequest
            {
                RequestKey = new RequestKey
                {
                    Country = "AU",
                    Culture = "en",
                    Application = _fixture.Create<string>()
                },
                Order = new Order
                {
                    StoreNo = _store.StoreNo,
                    OrderId = _fixture.Create<Guid>(),
                    OrderDate = new DateTime(2022, 2, 2, 18, 0, 0, DateTimeKind.Utc),
                    ServiceMethod = "Delivery",
                    DeliveryAddress = _fixture.Create<DeliveryAddress>(),
                    CustomerDetails = _fixture.Create<CustomerDetails>(),
                    Vouchers = new List<OrderVoucher>() {
                        { new OrderVoucher() { VoucherCode = "123" } },
                        { new OrderVoucher() { VoucherCode = "456" } },
                        { new OrderVoucher() { VoucherCode = "789" } },
                        { new OrderVoucher() { VoucherCode = "ABC" } },
                        { new OrderVoucher() { VoucherCode = "DEF" } },
                        { new OrderVoucher() { VoucherCode = "GHI" } },
                        { new OrderVoucher() { VoucherCode = "JKL" } },
                    },
                }
            };

            _pricingFeaturesService.GetFeatures(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new PricingFeatures
                {
                    EnhancedMinimumOrderValue = false
                });

            _pricingSettingsHelper.GetMaximumVoucherCount()
                .Returns(callInfo => 6);

            _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(Arg.Any<PriceOrderRequest>())
                .Returns(callInfo => callInfo.ArgAt<PriceOrderRequest>(0));

            var result = _pricingService.Price(request);
            result.ShouldNotBeNull();
            result.Order.ShouldNotBeNull();
            result.Order.Vouchers.ShouldNotBeNull();
            result.Messages.ShouldNotBeNull();
            result.Messages.Count.ShouldBeGreaterThanOrEqualTo(1);
            result.Messages.ShouldContain(m => m.Code == TooManyVouchersMessageEventId, 1);

            _emitter.Received(1).RaiseInfo(Arg.Any<CustomEvent>());
        }
    }
}
