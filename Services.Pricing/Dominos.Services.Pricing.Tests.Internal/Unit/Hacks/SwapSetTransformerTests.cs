using Dominos.Services.Pricing.App.Hacks;
using Shouldly;
using System.Collections.Generic;
using Dominos.Services.Coupons.Api.v1.Coupon;
using Dominos.Services.Coupons.Api.v1.SwapSet;
using Xunit;

namespace Dominos.Services.Pricing.Tests.Internal.Unit.Hacks
{
    public class SwapSetTransformerTests
    {
        private static ISwapSetTransformer transformer => new SwapSetTransformer();

        [Fact]
        public void AddMissingSwapSets_ShouldReturnNull_IfCouponResponseIsNull()
        {
            var result = transformer.AddMissingSwapSets(null);

            result.ShouldBe(null);
        }

        [Fact]
        public void AddMissingSwapSets_ShouldReturnUnchangedCouponResponse_IfCouponResponseCouponIsNull()
        {
            var couponResponse = new GetCouponResponse
            {
                Coupon = null,
                Found = true,
                SwapSets = new List<SwapSet>()
            };
            var result = transformer.AddMissingSwapSets(couponResponse);

            result.ShouldBe(couponResponse);
        }

        [Fact]
        public void AddMissingSwapSets_ShouldReturnUnchangedCouponResponse_IfCouponResponseRulesIsNull()
        {
            var couponResponse = new GetCouponResponse
            {
                Coupon = new Coupon 
                {
                    CouponCode = "cool-code",
                    Rules = null
                },
                Found = true,
                SwapSets = new List<SwapSet>()
            };
            var result = transformer.AddMissingSwapSets(couponResponse);

            result.ShouldBe(couponResponse);
        }

        [Fact]
        public void AddMissingSwapSets_ShouldReturnUnchangedCouponResponse_IfCouponResponseRulesAllHaveSwapSetCodes()
        {
            var couponResponse = new GetCouponResponse
            {
                Coupon = new Coupon 
                {
                    CouponCode = "cool-code",
                    Rules = new List<CouponRule>{
                        new CouponRule{ SwapSetCode = "sscode1" },
                        new CouponRule{ SwapSetCode = "sscode2" },
                    }
                },
                Found = true,
                SwapSets = new List<SwapSet>()
            };
            var result = transformer.AddMissingSwapSets(couponResponse);

            result.ShouldBe(couponResponse);
        }

        [Fact]
        public void AddMissingSwapSets_ShouldAddNewSwapSet_IfCouponResponseRuleIsMissingSwapSetCode()
        {
            var productCode = "CLAMPIZZA";
            var couponResponse = new GetCouponResponse
            {
                Coupon = new Coupon 
                {
                    CouponCode = "cool-code",
                    Rules = new List<CouponRule>{
                        new CouponRule
                        { 
                            SwapSetCode = null, 
                            DefaultProduct = new ProductConfiguration 
                            {
                                ProductCode = productCode
                            }
                        },
                    }
                },
                Found = true,
                SwapSets = new List<SwapSet>()
            };
            var result = transformer.AddMissingSwapSets(couponResponse);

            result.SwapSets.ShouldNotBe(null);
            result.SwapSets.Count.ShouldBe(1);
            result.SwapSets[0].SwapSetCode.ShouldNotBeNull();
            result.SwapSets[0].Products.Count.ShouldBe(1);
            result.SwapSets[0].Products[0].BaseCalculations.ShouldBeTrue();
            result.SwapSets[0].Products[0].ProductCode.ShouldBe(productCode);
        }
    }
}
