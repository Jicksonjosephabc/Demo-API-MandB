using Dominos.Common.Events.Infrastructure.Interfaces;
using Dominos.Common.Rest.Api.Resources;
using Dominos.Common.Rest.Api.Resources.Prices;
using Dominos.OLO.Media;
using Dominos.OLO.Pricing.Adaptor.Adaptors;
using Dominos.OLO.Pricing.Adaptor.Clients;
using Dominos.OLO.Pricing.Features;
using Dominos.OLO.Pricing.Messages;
using Dominos.OLO.Pricing.Module;
using Dominos.OLO.Pricing.Service.Contract.v2_2.Data;
using Dominos.Services.Common.Tools.Exceptions;
using Dominos.Services.Common.Tools.Exceptions.ServiceExceptions;
using Dominos.Services.Common.Tools.Extensions;
using Dominos.Services.Common.Tools.TelemetryEvents;
using Dominos.Services.Common.Tools.TelemetryEvents.EventTypes;
using Dominos.Services.Common.Tools.TelemetryEvents.Implementations.ApplicationInsights;
using Dominos.Services.Coupons.Api.v1.Coupon;
using Dominos.Services.Pricing.App.Clients.StoreMenuServiceClient;
using Dominos.Services.Pricing.App.Events;
using Dominos.Services.Pricing.App.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Dominos.Services.Pricing.App.Services
{
    public class PricingService : IPricingService
    {
        private readonly IPricingModule _pricingModule;
        private readonly IEmitter _emitter;
        private readonly IPricingSettingsHelper _pricingSettingsHelper;

        public PricingService(
            IEmitter emitter,
            ICouponQueries couponClient,
            IStoreMenuServiceClient storeMenuServiceClient,
            IPricingFeaturesService pricingFeatureService,
            IEvents legacyLogger,
            IOrderTimerClient orderTimerClient,
            IPricingSettingsHelper pricingSettingsHelper
            )
        {
            _emitter = emitter;

            var dataAdapter = new ClientPricingDataAdapter(storeMenuServiceClient, couponClient, storeMenuServiceClient, orderTimerClient);
            _pricingModule = new PricingModule(dataAdapter, pricingFeatureService, legacyLogger);

            _pricingSettingsHelper = pricingSettingsHelper;
        }

        public PriceOrderResponse Price(PriceOrderRequest request)
        {
            var source = new CancellationTokenSource(_pricingSettingsHelper.GetTimeout());
            var uniqueId = Guid.NewGuid();
            var originalVoucherCount = request.Order?.Vouchers?.Count ?? 0;
            var maximumVoucherCount = _pricingSettingsHelper.GetMaximumVoucherCount();

            // log start call with unique id, request payload, order id etc
            _emitter?.RaiseDebug(new CustomEvent(request.Order.OrderId.ToString(), $"{nameof(PricingService)}.{nameof(Price)} Begin Order Pricing"
                , new Dictionary<string, string>()
                {
                    { "UniqueId", uniqueId.ToString() },
                    { "IncomingProductCount", (request.Order?.Products?.Count ?? 0).ToString() },
                    { "IncomingVoucherCount", originalVoucherCount.ToString() },
                    { "PriceOrderRequest", request.ToJson() },
                }));

            try
            {
                request = RemoveExcessOrderVouchers(request, maximumVoucherCount, uniqueId);
                request = _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToOrder(request);
                var response = _pricingModule.Price(request, source.Token);

                // log end call with unique id, response payload, order id etc
                _emitter?.RaiseDebug(new CustomEvent(request.Order.OrderId.ToString(), $"{nameof(PricingService)}.{nameof(Price)} End Order Pricing"
                    , new Dictionary<string, string>()
                    {
                        { "UniqueId", uniqueId.ToString() },
                        { "OutgoingProductCount", (response.Order?.Products?.Count ?? 0).ToString() },
                        { "OutgoingVoucherCount", (response.Order?.Vouchers?.Count ?? 0).ToString() },
                        { "VoucherCode", string.Join(',', response.Order?.Vouchers?.Select(v => v.VoucherCode))},
                    }));

                if (originalVoucherCount > maximumVoucherCount)
                {
                    response = ApplyExcessVouchersMessage(request, response, maximumVoucherCount);
                }

                return response;
            }
            catch (OperationCanceledException)
            {
                // log cancellation
                _emitter?.RaiseWarning(new CustomEvent(request.Order.OrderId.ToString(), $"{nameof(PricingService)}.{nameof(Price)} Order Pricing Timeout"
                    , new Dictionary<string, string>()
                    {
                        { "UniqueId", uniqueId.ToString() }
                    }));

                // throw an exception when the pricing module cancellation is performed
                throw new ServiceInternalException(
                    new ServiceError()
                    {
                        OrderId = $"{request?.Order?.OrderId}",
                        Code = "OrderPricingTimeout",
                        Message = "Timeout occurred during order pricing",
                    }, HttpStatusCode.GatewayTimeout);
            }
            catch (Exception ex)
            {
                _emitter.RaiseError(new PricingServiceEvent("PriceOrder.Error", request, ex));

                throw new BadRequestException(
                    new ServiceError()
                    {
                        OrderId = $"{request?.Order?.OrderId}",
                        Code = "OrderPricingError",
                        Message = "Unexpected pricing failure order",
                    });
            }
            finally
            {
                source?.Dispose();
            }
        }

        public PricedProduct PriceProduct(PriceProductRequest request)
        {
            try
            {
                request = _pricingSettingsHelper.AddHalfNHalfPricingBehaviourToProduct(request);
                return _pricingModule.PriceProduct(request);
            }
            catch (Exception ex)
            {
                _emitter.RaiseError(new PricingServiceEvent("PriceProduct.Error", request, ex));
                throw new BadRequestException(
                    new ServiceError()
                    {
                        Code = "PriceProductError",
                        Message = "Unexpected pricing failure for product",
                    });
            }
        }

        /// <summary>
        /// This method will add the TooManyVouchers message to the outgoing price order response, 
        /// if the original voucher count exceeded the maximum vouncher count.
        /// A check is made on the messages list, so that the message is added only if it does not exist.
        /// </summary>
        /// <param name="request">The incoming price order request object.</param>
        /// <param name="response">The outgoing price order response object.</param>
        /// <param name="originalVoucherCount">The original number of vouchers attached to the incoming price order request.</param>
        /// <returns>The outgoing price order request that may contain the TooManyVouchers messages.</returns>
        private PriceOrderResponse ApplyExcessVouchersMessage(PriceOrderRequest request, PriceOrderResponse response, int maximumVoucherCount)
        {
            var message = new TooManyVouchersMessage(maximumVoucherCount);

            if (!response.Messages.Any(m => m.Code == message.EventId.ToString()))
            {
                var messageProvider = new StaticMessageProvider();

                response.Messages.Add(new OLO.Common.Service.Contract.v2_2.Data.Message
                {
                    Code = message.EventId.ToString(),
                    Text = message.GetText(messageProvider.GetErrorMessages(request.RequestKey.Country, request.RequestKey.Culture)),
                    Type = message.GetType().Name,
                });
            }

            return response;
        }

        /// <summary>
        /// This method will check the number of vouchers attached to the incoming price order request.
        /// If the number of vouchers exceeds the maximum allowed, then all excess vouchers will be removed, 
        /// so only the maximum number of vouchers are used during pricing. 
        /// </summary>
        /// <param name="request">The incoming price order request object that contains the vouchers to be checked.</param>
        /// <param name="uniqueId">This is a value used to group the AppInsights requests together.</param>
        /// <returns>The incoming price order request object that contains up to the maximum number of vouchers.</returns>
        private PriceOrderRequest RemoveExcessOrderVouchers(PriceOrderRequest request, int maximumVoucherCount, Guid uniqueId)
        {
            var incomingVoucherCount = request.Order?.Vouchers?.Count ?? 0;

            if (incomingVoucherCount > maximumVoucherCount)
            {
                _emitter?.RaiseInfo(new CustomEvent(request.Order.OrderId.ToString(), $"{nameof(PricingService)}.{nameof(Price)} Order Voucher Exceeds Maximum"
                    , new Dictionary<string, string>()
                    {
                        { "UniqueId", uniqueId.ToString() },
                        { "IncomingVoucherCount", incomingVoucherCount.ToString() },
                        { "MaximumVoucherCount", maximumVoucherCount.ToString() },
                    }));

                request.Order.Vouchers = request.Order.Vouchers.GetRange(0, maximumVoucherCount);
            }

            return request;
        }
    }
}
