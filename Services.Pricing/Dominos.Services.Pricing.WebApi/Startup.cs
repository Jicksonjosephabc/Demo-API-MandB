using Dominos.Services.Pricing.App;
using Dominos.Services.Common.Tools.Extensions;
using Dominos.Services.Common.Tools.Swagger;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Dominos.Services.Pricing.WebApi.Helpers;
using System.Collections.Generic;
using Dominos.Services.Common.Tools.HealthChecks.Extensions;

namespace Dominos.Services.Pricing.WebApi
{
    public class Startup
    {

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHealthChecks();
            app.UseRouting();
            app.UseDominosSwagger(provider);
            app.UseAllDominosMiddleware(env.IsDevelopment());
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services
                .AddControllers(options => options.EnableEndpointRouting = true)
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<AppAssemblyMarker>();
                    fv.ImplicitlyValidateChildProperties = true;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options =>
                {
                    // TODO : Aligned to original service - may need to revisit
                    options.SerializerSettings.ContractResolver = new IgnoreDataContractContractResolver();
                    options.SerializerSettings.Converters = new List<JsonConverter> { new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() } };
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    //options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                    //options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    //options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                });

            services.AddApiVersioning(
                options =>
                {
                    // Reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions".
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                });

            services.AddVersionedApiExplorer(
            options =>
            {
                    // Add the versioned api explorer, which also adds IApiVersionDescriptionProvider service.
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // Can also be used to control the format of the API version in route templates.
                    options.SubstituteApiVersionInUrl = true;
            });

            services.AddHttpContextAccessor();
            services.RegisterServices(this.Configuration);
            services.AddApplicationInsightsTelemetry();

        }

    }
}