using System.Net;
using System.Reflection;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dominos.Services.Pricing.WebApi.Api
{
    [ApiVersionNeutral]
    [Route("")]
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public class HomeController : ControllerBase
    {
        private readonly IOptions<ApplicationInsightsServiceOptions> _options;
        private static AppInformation _api;

        public HomeController(IOptions<ApplicationInsightsServiceOptions> options)
        {
            _options = options;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var result = _api ?? (_api = new AppInformation
            {
                Description = "Service Features",
                Ikey = _options.Value.InstrumentationKey,
                Build = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                MachineName = System.Environment.MachineName,
            });

            return Ok(result);
        }
    }

    public class AppInformation
    {
        public string Description { get; set; }

        public string Build { get; set; }

        public string MachineName { get; set; }

        public string Ikey { get; set; }

    }
}