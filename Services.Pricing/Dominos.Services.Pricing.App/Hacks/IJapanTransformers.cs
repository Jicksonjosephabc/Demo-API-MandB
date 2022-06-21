namespace Dominos.Services.Pricing.App.Hacks
{
    public interface IJapanTransformers
    {
        StoreMenu.Api.v1.Product.Models.ProductDetails AddRecipeComponentsToToppings(StoreMenu.Api.v1.Product.Models.ProductDetails productDetails);
        Dominos.Common.Rest.Api.Resources.ProductDetails UpdateInvalidSizeCode(Dominos.Common.Rest.Api.Resources.ProductDetails productDetails);
    }
}