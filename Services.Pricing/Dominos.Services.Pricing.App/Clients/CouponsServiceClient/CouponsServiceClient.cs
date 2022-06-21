using Dominos.Services.Common.Tools.Extensions;
using Dominos.Services.Common.Tools.Redis.Configuration;
using Dominos.Services.Common.Tools.Redis.Services;
using Dominos.Services.Common.Tools.ServiceClient;
using Dominos.Services.Common.Tools.TelemetryEvents;
using Dominos.Services.Coupons.Api.v1.Coupon;
using Dominos.Services.Pricing.App.Clients.CouponsServiceClient.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Dominos.Services.Pricing.App.Hacks;

namespace Dominos.Services.Pricing.App.Clients.CouponsServiceClient
{
    public class CouponsServiceClient : ServiceClientBase, ICouponQueries
    {
        private readonly IEmitter _emitter;
        private readonly IRedisCachingService _redisCachingService;
        private readonly List<ClientCachingSettings> _clientCachingSettings;
        private readonly ISwapSetTransformer _swapSetTransformer;

        public CouponsServiceClient(
            ILogger<CouponsServiceClient> logger,
            IEmitter emitter,
            IOptions<CouponsServiceClientSettings> settings,
            HttpClient client,
            IRedisCachingService redisCachingService,
            ISwapSetTransformer swapSetTransformer)
            : base(logger, emitter, client, settings.Value)
        {
            _emitter = emitter;
            _redisCachingService = redisCachingService;
            _clientCachingSettings = settings.Value.Caching;
            _swapSetTransformer = swapSetTransformer;
        }

        public List<Coupon> GetAllAutoCoupons(GetAutoDiscountRequest request)
        {
            var path = $"coupons/auto?request.countryCode={request.CountryCode}&request.orderTime={request.OrderTime}&request.bypassCache={request.BypassCache}";
            var cachedResults = _redisCachingService.GetCacheValue<GetAutoDiscountResponse>(path, _clientCachingSettings).Result;
            if (cachedResults != null)
            {
                return cachedResults.Coupon;
            }

            try
            {
                var couponServiceResults = PerformRequest<List<Coupon>>(path, HttpMethod.Get.ToString(), new RequestOptions { LogEvent = true }).Result;
                var cachingResponse = _redisCachingService.SetCacheValue(path, new GetAutoDiscountResponse { Coupon = couponServiceResults }, _clientCachingSettings).Result;

                return couponServiceResults;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public GetCouponResponse GetCouponDetails(GetCouponRequest request)
        {
            var path = $"coupons/details/bycode?request.countryCode={request.CountryCode}&request.couponCode={request.CouponCode}&request.bypassCache={request.BypassCache}";
            var cachedResults = _redisCachingService.GetCacheValue<GetCouponResponse>(path, _clientCachingSettings).Result;
            if (cachedResults != null)
            {
                return cachedResults;
            }

            try
            {
                var couponServiceResults = PerformRequest<GetCouponResponse>(path, HttpMethod.Get.ToString(), new RequestOptions { LogEvent = true }).Result;

                couponServiceResults = _swapSetTransformer.AddMissingSwapSets(couponServiceResults);

                var cachingResponse = _redisCachingService.SetCacheValue(path, couponServiceResults, _clientCachingSettings).Result;

                return couponServiceResults;
            }
            catch
            {
                return null;
            }
        }

        protected override void SetCurrentAuthorization(HttpRequestHeaders requestHeaders, RequestOptions options)
        {
            base.SetCurrentAuthorization(requestHeaders, options);
            if (ApiKey.IsNotEmpty())
            {
                requestHeaders.Authorization = new AuthenticationHeaderValue("token", $"apikey={Uri.EscapeDataString(Settings.ApiKey)}");
            }
        }
        public void DeleteCoupon(DeleteCouponRequest request)
        {
            throw new NotImplementedException();
        }

        public void DeleteCouponGroup(DeleteCouponGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public GenerateRestrictedCouponCodesResponse GenerateRestrictedCouponCodes(GenerateRestrictedCouponCodesRequest request)
        {
            throw new NotImplementedException();
        }

        public List<Coupon> GetAllComboCoupons(GetAllComboCouponsRequest request)
        {
            throw new NotImplementedException();
        }

        public List<Coupon> GetAllCoupons(GetAllCouponsRequest request)
        {
            throw new NotImplementedException();
        }

        public Coupon GetCoupon(GetCouponRequest request)
        {
            throw new NotImplementedException();
        }

        public Coupon GetCouponByVoucherId(GetCouponByVoucherIdRequest request)
        {
            throw new NotImplementedException();
        }



        public List<Coupon> GetCouponsByName(GetCouponsByNameRequest request)
        {
            throw new NotImplementedException();
        }

        public List<Coupon> GetMenuCoupons(MenuCouponRequest request)
        {
            throw new NotImplementedException();
        }

        public RestrictedCoupon GetRestrictedCoupon(GetRestrictedCouponRequest request)
        {
            throw new NotImplementedException();
        }

        public GetWebCouponsForStoreResponse GetWebCouponsForStore(GetWebCouponsForStoreRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveCouponUseResponse RemoveCouponUse(RemoveCouponUseCommand command)
        {
            throw new NotImplementedException();
        }

        public SaveCouponResponse SaveCoupon(SaveCouponRequest request)
        {
            throw new NotImplementedException();
        }

        public void SaveRestrictedCoupon(RestrictedCoupon request)
        {
            throw new NotImplementedException();
        }

        public UpdateCouponUseResponse UpdateCouponUse(UpdateCouponUseCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
