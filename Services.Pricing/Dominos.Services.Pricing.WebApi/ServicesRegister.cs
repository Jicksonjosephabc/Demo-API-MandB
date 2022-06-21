using Dominos.Services.Common.Tools.ApiSecurity;
using Dominos.Services.Common.Tools.GreenBlueHeader;
using Dominos.Services.Common.Tools.ServiceClient.SerialiserProviders;
using Dominos.Services.Common.Tools.StartupTasks.Extensions;
using Dominos.Services.Common.Tools.StartupTasks.Tasks;
using Dominos.Services.Common.Tools.Swagger;
using Dominos.Services.Common.Tools.TelemetryEvents.Extensions;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Dominos.Common.Events.Infrastructure.Interfaces;
using Dominos.Services.Common.Tools.Configuration;
using Dominos.Services.Pricing.App.Services;
using Dominos.Services.Pricing.App.Clients.StoreMenuServiceClient;
using Dominos.OLO.Pricing.Adaptor.Clients;
using Dominos.OLO.Pricing.Features;
using Dominos.Services.Pricing.App.Clients.CouponsServiceClient;
using Dominos.Services.Coupons.Api.v1.Coupon;
using Dominos.Services.Common.Tools.Resilience.Extensions;
using Dominos.Services.Common.Tools.Redis.Extensions;
using Dominos.Services.Pricing.App.Clients.OrderTimerServiceClient;
using Dominos.Services.Pricing.App.Events;
using Dominos.Services.Pricing.App.Hacks;
using Dominos.Services.Common.Tools.HealthChecks.Extensions;
using Dominos.Services.Pricing.App.Configuration;
using Dominos.Services.Pricing.App.Helpers;
using Dominos.Services.Common.Tools.FeatureToggles.Extensions;

namespace Dominos.Services.Pricing.WebApi
{
    public static class ServicesRegister
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<InfrastructureSettings>(configuration.GetSection(nameof(InfrastructureSettings)));
            services.Configure<ApplicationInsightsServiceOptions>(configuration.GetSection("ApplicationInsights"));
            services.Configure<SecurityConfig>(configuration.GetSection(nameof(SecurityConfig)));
            services.Configure<SwaggerSettings>(configuration.GetSection(nameof(SwaggerSettings)));
            
            services.AddAutoMapper(typeof(App.AppAssemblyMarker));
            services.AddDominosSwagger(configuration);
            services.AddDominosLogs(configuration, "Pricing");
            services.AddHealthChecks(configuration);
            services.AddStartupTask<WarmupServicesStartupTask>();
            services.AddGreenBlueHeader();
            services.AddResiliencePolicies(configuration);
            services.AddRedis(configuration);

            services
                .Configure<CouponsServiceClientSettings>(configuration.GetSection(nameof(CouponsServiceClientSettings)))
                .AddResilientHttpClient<ICouponQueries, CouponsServiceClient>();

            services
                .Configure<StoreMenuServiceClientSettings>(configuration.GetSection(nameof(StoreMenuServiceClientSettings)))
                .AddSingleton<IJapanTransformers, JapanTransformers>()
                .AddResilientHttpClient<IStoreMenuServiceClient, StoreMenuServiceClient>();

            services
                .Configure<OrderTimerServiceClientSettings>(configuration.GetSection(nameof(OrderTimerServiceClientSettings)))
                .AddResilientHttpClient<IOrderTimerClient, OrderTimerServiceClient>();

            services.AddSingleton<IPricingFeaturesService, PricingFeaturesService>();

            services.AddSingleton(new CustomJsonSerializerSettingsProvider().GetSettings());
            
            services.AddSingleton<ISwapSetTransformer, SwapSetTransformer>();

            services
                .Configure<PricingSettings>(configuration.GetSection(nameof(PricingSettings)))
                .AddSingleton<IPricingSettingsHelper, PricingSettingsHelper>();

            services.AddScoped<IPricingService, PricingService>();
            services.AddSingleton<IEvents, LegacyLogger>();
            services.AddFeatureToggles(configuration);
        }
    }
}