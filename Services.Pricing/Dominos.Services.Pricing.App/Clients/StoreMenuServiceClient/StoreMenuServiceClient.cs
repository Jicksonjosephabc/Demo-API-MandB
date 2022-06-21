using AutoMapper;
using Dominos.Common.Rest.Api.Resources;
using Dominos.Common.Rest.Api.Resources.Products;
using Dominos.Common.Rest.Api.Resources.Stores;
using Dominos.Services.Common.Tools.Redis.Configuration;
using Dominos.Services.Common.Tools.Redis.Services;
using Dominos.Services.Common.Tools.ServiceClient;
using Dominos.Services.Common.Tools.TelemetryEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dominos.Common.Rest.Adaptor;
using Dominos.Common.Rest.Adaptor.Products;
using Dominos.Common.Rest.Adaptor.Stores;
using Dominos.Common.Rest.Api.Resources.Menus;
using Dominos.Common.Rest.Api.Resources.Territories;
using Dominos.Common.Rest.Api.Resources.Varieties;
using Dominos.OLO.Nutritionals.Api.Allergens;
using Dominos.OLO.Nutritionals.Api.Allergens.Queries;
using Dominos.Services.Pricing.App.Hacks;


namespace Dominos.Services.Pricing.App.Clients.StoreMenuServiceClient
{
    public class StoreMenuServiceClient : ServiceClientBase, IStoreMenuServiceClient
    {
        private readonly IEmitter _emitter;
        private readonly IMapper _mapper;
        private readonly IRedisCachingService _redisCachingService;
        private readonly List<ClientCachingSettings> _clientCachingSettings;
        private readonly IJapanTransformers _japanTransformers;

        public StoreMenuServiceClient(
            ILogger<StoreMenuServiceClient> logger,
            IEmitter emitter,
            IOptions<StoreMenuServiceClientSettings> settings,
            HttpClient client,
            IMapper mapper,
            IJapanTransformers japanTransformers,
            IRedisCachingService redisCachingService
            )
            : base(logger, emitter, client, settings.Value)
        {
            _emitter = emitter;
            _mapper = mapper;
            _japanTransformers = japanTransformers;
            _redisCachingService = redisCachingService;
            _clientCachingSettings = settings.Value.Caching;
        }

        public TResult Get<TResult>(ResourceNames name, object urlParams) where TResult : class
        {
            switch (name)
            {
                case ResourceNames.StoresWithLanguage:
                    return new GetStoreResponse { Store = GetStore(urlParams as GetStoreRequest).Result } as TResult;

                case ResourceNames.Products:
                    return GetProduct(urlParams as GetProductRequest).Result as TResult;

                default:
                    throw new NotImplementedException();
            }
        }

        public async Task<Store> GetStore(GetStoreRequest request)
        {
            var storeMenuRequest = _mapper.Map<GetStoreRequest>(request);

            var path = $"v1/store?CountryCode={storeMenuRequest.CountryCode}&Language={request.Language}&StoreNo={storeMenuRequest.StoreNo}";
            if (storeMenuRequest.DayOfTrade.HasValue)
            {
                path += $"&DayOfTrade={storeMenuRequest.DayOfTrade.Value.AddDays(1).ToString("yyyy-MM-dd")}";
            }
            var cachedResults = _redisCachingService.GetCacheValue<Store>(path, _clientCachingSettings).Result;
            if (cachedResults != null)
            {
                return cachedResults; ;
            }

            try
            {
                var storeMenuResult = await PerformRequest<StoreMenu.Api.v1.Store.Models.Store>(path, HttpMethod.Get.ToString(), new RequestOptions { LogEvent = true });

                var translatedResponse = _mapper.Map<Store>(storeMenuResult);
                _redisCachingService.SetCacheValue(path, translatedResponse, _clientCachingSettings).Wait();

                return translatedResponse;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ProductDetails> GetProduct(GetProductRequest request)
        {
            var storeMenuRequest = _mapper.Map<GetProductRequest>(request);
            
            var path = $"v1/product?request.country={storeMenuRequest.Country}&request.culture={storeMenuRequest.Culture}&request.storeNo={storeMenuRequest.StoreNo}&request.productCode={storeMenuRequest.ProductCode}&request.OrderTime={request.OrderTime}&request.InvertPosCode=true&request.IncludePizzaChef=false";
            var cachedResults = _redisCachingService.GetCacheValue<ProductDetails>(path, _clientCachingSettings).Result;
            if (cachedResults != null)
            {
                return cachedResults;
            }

            try
            {
                var storeMenuResult = await PerformRequest<StoreMenu.Api.v1.Product.Models.ProductDetails>(path, HttpMethod.Get.ToString(), new RequestOptions { LogEvent = true });

                // adjustments
                storeMenuResult = _japanTransformers.AddRecipeComponentsToToppings(storeMenuResult);

                var translatedResponse = _mapper.Map<ProductDetails>(storeMenuResult);

                // adjustments
                translatedResponse = _japanTransformers.UpdateInvalidSizeCode(translatedResponse);

                _redisCachingService.SetCacheValue(path, translatedResponse, _clientCachingSettings).Wait();
                return translatedResponse;
            }
            catch
            {
                return null;
            }
        }

        public GetStoreDeliveryOrderMinimumValuesResponse GetStoreDeliveryOrderMinimumValues(GetStoreDeliveryOrderMinimumValuesRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            var path = $"v1/{request.CountryCode}/stores/{request.StoreNo}/deliveryorderminimumvalues";

            var cachedResults = _redisCachingService.GetCacheValue<GetStoreDeliveryOrderMinimumValuesResponse>(path, _clientCachingSettings).GetAwaiter().GetResult();
            if (cachedResults != null)
            {
                return cachedResults;
            }

            var storeMenuServiceResult = PerformRequest<StoreMenu.Api.v1.Store.Models.StoreDeliveryOrderMinimumValues>(path, HttpMethod.Get.ToString(), new RequestOptions { LogEvent = true }).GetAwaiter().GetResult();
            var storeMenuModuleResult = _mapper.Map<GetStoreDeliveryOrderMinimumValuesResponse>(storeMenuServiceResult);

            _redisCachingService.SetCacheValue(path, storeMenuModuleResult, _clientCachingSettings).Wait();

            return storeMenuModuleResult;
        }

        public Task<TResult> GetAsync<TResult>(ResourceNames name, object urlParams) where TResult : class
        {
            throw new NotImplementedException();
        }

        public Task<TResult> GetDirectAsync<TResult>(ResourceNames name, object urlParams)
        {
            throw new NotImplementedException();
        }

        public TResult Post<TPost, TResult>(ResourceNames name, object urlParams, TPost postData)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> PostAsync<TPost, TResult>(ResourceNames name, object urlParams, TPost postData)
        {
            throw new NotImplementedException();
        }

        public GetStoreResponse GetStoreWithLanguage(GetStoreRequest request)
        {
            throw new NotImplementedException();
        }

        Store IStoreMenuModule.GetStore(GetStoreRequest request)
        {
            throw new NotImplementedException();
        }

        public GetMenuRulesResponse GetMenuRules(GetMenuRulesRequest request)
        {
            throw new NotImplementedException();
        }

        public MenuResponse GetMenu(GetMenuRequest request)
        {
            throw new NotImplementedException();
        }

        public MenuPage GetVariety(GetVarietyRequest request)
        {
            throw new NotImplementedException();
        }

        public List<Store> GetStoresByRegion(GetStoresForRegionRequest request)
        {
            throw new NotImplementedException();
        }

        public GetStoresForCountryResponse GetStoresForCountry(GetStoresForCountryRequest request)
        {
            throw new NotImplementedException();
        }

        public GetStorePaymentSettingResponse GetStorePaymentSetting(GetStorePaymentSettingRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAllVarietiesResponse GetAllVarieties(GetAllVarietiesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAllProductsResponse GetAllProducts(GetAllProductsRequest request)
        {
            throw new NotImplementedException();
        }

        ProductDetails IStoreMenuModule.GetProduct(GetProductRequest request)
        {
            throw new NotImplementedException();
        }

        public GetProductsResponse GetProducts(GetProductsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetTerritoryGrantsResponse GetTerritoryGrants(GetTerritoryGrantsRequest request)
        {
            throw new NotImplementedException();
        }

        public AllergenModel GetAllergenModel(GetAllergensQuery request)
        {
            throw new NotImplementedException();
        }
    }
}
