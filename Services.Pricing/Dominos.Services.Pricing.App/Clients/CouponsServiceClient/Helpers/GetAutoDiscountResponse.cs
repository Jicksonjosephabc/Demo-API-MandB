using Dominos.Services.Coupons.Api.v1.Coupon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dominos.Services.Pricing.App.Clients.CouponsServiceClient.Helpers
{
    public class GetAutoDiscountResponse
    {
        public List<Coupon> Coupon { get; set; }
    }
}
