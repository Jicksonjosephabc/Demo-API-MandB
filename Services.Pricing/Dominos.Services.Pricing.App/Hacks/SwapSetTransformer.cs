using System;
using System.Collections.Generic;
using System.Linq;
using Dominos.Services.Coupons.Api.v1.Coupon;
using Dominos.Services.Coupons.Api.v1.SwapSet;

namespace Dominos.Services.Pricing.App.Hacks
{
    public class SwapSetTransformer : ISwapSetTransformer
    {
        // when a coupon has a single item with no swap set (eg garlic bread on 
        // some vouchers), we have to create and add a swapset with just that
        // product in it for pricing to handle it correctly
        // see card https://dominos-au.visualstudio.com/OneDigital/_workitems/edit/93064
        public GetCouponResponse AddMissingSwapSets(GetCouponResponse couponResponse)
        {
            if (couponResponse?.Coupon?.Rules == null) return couponResponse;

            foreach (var couponRule in couponResponse.Coupon.Rules.Where(couponRule => couponRule.SwapSetCode == null))
            {
                var newSwapSetCode = Guid.NewGuid().ToString();
                var newSwapSet = CreateNewSwapSet(couponRule, newSwapSetCode);
                
                if (couponResponse.SwapSets == null)
                {
                    couponResponse.SwapSets = new List<SwapSet>();
                }
                couponResponse.SwapSets.Add(newSwapSet);
                couponRule.SwapSetCode = newSwapSetCode;
            }

            return couponResponse;
        }

        private static SwapSet CreateNewSwapSet(CouponRule couponRule, string swapSetCode)
        {
            return new SwapSet
            {
                SwapSetCode = swapSetCode,
                Products = new List<SwapSetProduct>
                {
                    new SwapSetProduct
                        {BaseCalculations = true, ProductCode = couponRule?.DefaultProduct?.ProductCode}
                }
            };
        }
    }
}
