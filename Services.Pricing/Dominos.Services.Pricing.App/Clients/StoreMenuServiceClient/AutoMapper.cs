using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dominos.Services.Pricing.App.Clients.StoreMenuServiceClient
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<Dominos.Common.Rest.Api.Resources.Products.GetProductRequest, StoreMenu.Api.v1.Product.Requests.GetProductRequest>();

            CreateMap<StoreMenu.Api.v1.Product.Models.ProductDetails, Dominos.Common.Rest.Api.Resources.ProductDetails>();

            CreateMap<Dominos.Common.Rest.Api.Resources.Stores.GetStoreRequest, StoreMenu.Api.v1.Store.Requests.GetStoreRequest>();

            CreateMap<StoreMenu.Api.v1.Store.Models.Store, Dominos.Common.Rest.Api.Resources.Store>();

            CreateMap<StoreMenu.Api.v1.Store.Models.StoreAddress, Dominos.Common.Rest.Api.Resources.StoreAddress>();
            CreateMap<StoreMenu.Api.v1.Store.Models.StoreCapacityOverride, Dominos.Common.Rest.Api.Resources.StoreCapacityOverride>();
            CreateMap<StoreMenu.Api.v1.Store.Models.StoreCapacity, Dominos.Common.Rest.Api.Resources.StoreCapacity>();
            CreateMap<StoreMenu.Api.v1.Store.Models.StorePulseConfig, Dominos.Common.Rest.Api.Resources.StorePulseConfig>();
            CreateMap<StoreMenu.Api.v1.Store.Models.StorePriceInfo, Dominos.Common.Rest.Api.Resources.StorePriceInfo>();
            CreateMap<StoreMenu.Api.v1.Store.Models.OrderTracking, Dominos.Common.Rest.Api.Resources.OrderTracking>();
            CreateMap<StoreMenu.Api.v1.Store.Models.StoreServiceMethodUnavailability, Dominos.Common.Rest.Api.Resources.StoreServiceMethodUnavailability>();
            CreateMap<StoreMenu.Api.v1.Store.Models.StoreClosure, Dominos.Common.Rest.Api.Resources.StoreClosure>();
            CreateMap<StoreMenu.Api.v1.Common.Models.GeoCoords, Dominos.Common.Rest.Api.Resources.GeoCoords>();
            CreateMap<StoreMenu.Api.v1.Store.Models.StoreHours, Dominos.Common.Rest.Api.Resources.StoreHours>();
            CreateMap<StoreMenu.Api.v1.Common.Models.ServiceMethods, Dominos.Common.Rest.Api.Resources.ServiceMethods>();
            CreateMap<StoreMenu.Api.v1.Common.Models.OrderingMethods, Dominos.Common.Rest.Api.Resources.OrderingMethods>();
            CreateMap<StoreMenu.Api.v1.Store.Models.StoreSurcharge, Dominos.Common.Rest.Api.Resources.StoreSurcharge>();

            CreateMap<StoreMenu.Api.v1.Product.Models.ProductPortion, Dominos.Common.Rest.Api.Resources.ProductPortion>();
            CreateMap<StoreMenu.Api.v1.Product.Models.ProductSize, Dominos.Common.Rest.Api.Resources.ProductSize>();
            CreateMap<StoreMenu.Api.v1.Common.Models.Legend, Dominos.Common.Rest.Api.Resources.Legend>();
            CreateMap<StoreMenu.Api.v1.Common.Models.ImageLink, Dominos.Common.Rest.Api.Resources.ImageLink>();
            CreateMap<StoreMenu.Api.v1.Product.Models.ComponentServing, Dominos.Common.Rest.Api.Resources.ProductRecipeComponent>();
            CreateMap<StoreMenu.Api.v1.Product.Models.ComponentServing, Dominos.Common.Rest.Api.Resources.ComponentServing>();

            CreateMap<StoreMenu.Api.v1.Common.Models.SizePrice, Dominos.Common.Rest.Api.Resources.SizePrice>();
            CreateMap<StoreMenu.Api.v1.Common.Models.Nutritionals, Dominos.Common.Rest.Api.Resources.Nutritionals>();
            CreateMap<StoreMenu.Api.v1.Product.Models.ComponentRules, Dominos.Common.Rest.Api.Resources.ComponentRules>();
            CreateMap<StoreMenu.Api.v1.Common.Models.PriceInfo, Dominos.Common.Rest.Api.Resources.PriceInfo>();
            CreateMap<StoreMenu.Api.v1.Common.Models.PromotionPrice, Dominos.Common.Rest.Api.Resources.PromotionPrice>();
            CreateMap<StoreMenu.Api.v1.Common.Models.PizzaChefInfo, Dominos.Common.Rest.Api.Resources.PizzaChefInfo>();
            CreateMap<StoreMenu.Api.v1.Common.Models.TaggedImage, Dominos.Common.Rest.Api.Resources.TaggedImage>();

            CreateMap<StoreMenu.Api.v1.Store.Models.StoreDeliveryOrderMinimumValues, Dominos.Common.Rest.Adaptor.Stores.GetStoreDeliveryOrderMinimumValuesResponse>();
            CreateMap<StoreMenu.Api.v1.Store.Models.StoreDeliveryOrderMinimumValueRing, OLO.Stores.Api.StorePrices.Models.DeliveryMinimumValueRing>();
        }
    }
    
}
