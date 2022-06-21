using System;
using System.Net;
using Dominos.Common.Rest.Api.Resources.Prices;
using Dominos.OLO.Pricing.Service.Contract.v2_2.Data;
using Dominos.Common.Rest.Api.Resources;
using Dominos.Services.Pricing.App.Services;
using Microsoft.AspNetCore.Mvc;
using Dominos.Services.Common.Tools.Extensions;

namespace Dominos.Services.Pricing.WebApi.Api.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/[controller]")]
    [Route("/")] // TODO : Backwards compatible, should be removed
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;

        public PricingController(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        [HttpPost]
        [Route("price/order/legacy")]
        public PriceOrderResponse PriceOrder([FromBody] PriceOrderRequest request)
        {
            return _pricingService.Price(request);
        }

        [HttpPost]
        [Route("price/product")]
        public PricedProduct PriceProduct([FromBody] PriceProductRequest request)
        {
            return _pricingService.PriceProduct(request);
        }
    }
}
