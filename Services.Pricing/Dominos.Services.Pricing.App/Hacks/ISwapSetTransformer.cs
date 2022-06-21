using Dominos.Services.Coupons.Api.v1.Coupon;

namespace Dominos.Services.Pricing.App.Hacks
{
    public interface ISwapSetTransformer
    {
        GetCouponResponse AddMissingSwapSets(GetCouponResponse couponResponse);
    }
}