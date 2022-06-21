using Dominos.Common.Rest.Api.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dominos.Services.Pricing.App.Hacks
{
    public class JapanTransformers : IJapanTransformers
    {
        public ProductDetails UpdateInvalidSizeCode(ProductDetails productDetails)
        {
            if (productDetails.Sizes == null) return productDetails;
            if (productDetails.CountryCode.ToLowerInvariant() != "jp") return productDetails;

            if (productDetails.Sizes.Count == 1
                && productDetails.Sizes[0].Crusts == null
                && productDetails.Sizes[0].SizeCode == "N/A")
            {
                productDetails.Sizes[0].SizeCode = null;
            }

            return productDetails;
        }

        public StoreMenu.Api.v1.Product.Models.ProductDetails AddRecipeComponentsToToppings(StoreMenu.Api.v1.Product.Models.ProductDetails productDetails)
        {

            if (productDetails.Sizes == null) return productDetails;
            if (productDetails.RecipeComponents == null) return productDetails;
            if (productDetails.CountryCode.ToLowerInvariant() != "jp") return productDetails;

            foreach (var size in productDetails.Sizes.Where(x => x.Toppings?.Components != null))
            {
                foreach (var topping in productDetails.RecipeComponents)
                {
                    size.Toppings.Components.Add(topping);
                }
            }

            return productDetails;
        }
    }
}
