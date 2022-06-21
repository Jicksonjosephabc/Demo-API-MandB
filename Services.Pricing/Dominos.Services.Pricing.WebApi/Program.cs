using System;
using System.IO;
using System.Reflection;
using Dominos.Services.Pricing.App.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dominos.Services.Pricing.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(baseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environments.Development}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var threadCount = configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>()?.ThreadCount ?? 0;

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.Sources.Clear();
                    builder.AddConfiguration(configuration);
                })
                .UseLibuv(o => o.ThreadCount = threadCount == 0 ? (int)Math.Ceiling((decimal)Environment.ProcessorCount / 2) : threadCount)
                .ConfigureLogging((hostingContext, options) =>
                {
                    options.AddApplicationInsights();
                })
                .UseStartup<Startup>();
        }

    }
}