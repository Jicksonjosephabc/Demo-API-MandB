using Dominos.Services.Pricing.App.Hacks;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dominos.Services.Pricing.Tests.Internal.Unit.Hacks
{
    public class JapanTransformersTests
    {
        private static IJapanTransformers transformer => new JapanTransformers();

        [Fact]
        public void ShouldReturnSizeCodeUnchangedIfNotForJP()
        {

            var productDetails = new Dominos.Common.Rest.Api.Resources.ProductDetails
            {
                CountryCode = "NOTJP",
                Sizes = new List<Dominos.Common.Rest.Api.Resources.ProductSize> {
                    new Dominos.Common.Rest.Api.Resources.ProductSize {
                        SizeCode = "N/A",
                        Crusts = null,
                    },
                },
            };

            var results = transformer.UpdateInvalidSizeCode(productDetails);

            productDetails.Sizes[0].SizeCode.ShouldBe("N/A");
            results.Sizes[0].SizeCode.ShouldBe("N/A");

        }

        [Fact]
        public void ShouldReturnSizeCodeUnchangedIfThereAreNoSizes()
        {

            var productDetails = new Dominos.Common.Rest.Api.Resources.ProductDetails
            {
                CountryCode = "JP",
                Sizes = null,
            };

            var results = transformer.UpdateInvalidSizeCode(productDetails);

            productDetails.Sizes.ShouldBeNull();
            results.Sizes.ShouldBeNull();

        }

        [Fact]
        public void ShouldReturnSizesUnchangedIfThereAreMoreThanOneSize()
        {

            var productDetails = new Dominos.Common.Rest.Api.Resources.ProductDetails
            {
                CountryCode = "JP",
                Sizes = new List<Dominos.Common.Rest.Api.Resources.ProductSize> {
                    new Dominos.Common.Rest.Api.Resources.ProductSize {
                        SizeCode = "Unchanged"
                    },
                    new Dominos.Common.Rest.Api.Resources.ProductSize()
                },
            };

            var results = transformer.UpdateInvalidSizeCode(productDetails);

            productDetails.Sizes[0].SizeCode.ShouldBe("Unchanged");
            results.Sizes[0].SizeCode.ShouldBe("Unchanged");

        }

        [Fact]
        public void ShouldReturnSizeUnchangedIfThereIsACrustCollection()
        {

            var productDetails = new Dominos.Common.Rest.Api.Resources.ProductDetails
            {
                CountryCode = "JP",
                Sizes = new List<Dominos.Common.Rest.Api.Resources.ProductSize> {
                    new Dominos.Common.Rest.Api.Resources.ProductSize {
                        SizeCode = "Unchanged",
                        Crusts = new Dominos.Common.Rest.Api.Resources.ComponentRules()
                    },
                },
            };

            var results = transformer.UpdateInvalidSizeCode(productDetails);

            productDetails.Sizes[0].SizeCode.ShouldBe("Unchanged");
            results.Sizes[0].SizeCode.ShouldBe("Unchanged");

        }

        [Fact]
        public void ShouldReturnSizeUpdatedSizeCode()
        {

            var productDetails = new Dominos.Common.Rest.Api.Resources.ProductDetails
            {
                CountryCode = "JP",
                Sizes = new List<Dominos.Common.Rest.Api.Resources.ProductSize> {
                    new Dominos.Common.Rest.Api.Resources.ProductSize {
                        SizeCode = "N/A",
                        Crusts = null,
                    },
                },
            };

            var results = transformer.UpdateInvalidSizeCode(productDetails);

            productDetails.Sizes[0].SizeCode.ShouldBeNull();
            results.Sizes[0].SizeCode.ShouldBeNull();

        }

        [Fact]
        public void ShouldReturnToppingComponentsUnchangedIfNotForJP()
        {

            var productDetails = new StoreMenu.Api.v1.Product.Models.ProductDetails
            {
                CountryCode = "NOTJP",
                Sizes = new List<StoreMenu.Api.v1.Product.Models.ProductSize> {
                    new StoreMenu.Api.v1.Product.Models.ProductSize
                    {
                        Toppings = new StoreMenu.Api.v1.Product.Models.ComponentRules {
                            Components = new List<StoreMenu.Api.v1.Product.Models.ComponentServing>()
                        }
                    }
                },
                RecipeComponents = new List<StoreMenu.Api.v1.Product.Models.ComponentServing> {
                    new StoreMenu.Api.v1.Product.Models.ComponentServing { ComponentCode = "1"},
                    new StoreMenu.Api.v1.Product.Models.ComponentServing { ComponentCode = "2"}
                }

            };

            var results = transformer.AddRecipeComponentsToToppings(productDetails);

            productDetails.Sizes[0].Toppings.Components.Count.ShouldBe(0);
            results.Sizes[0].Toppings.Components.Count.ShouldBe(0);

        }

        [Fact]
        public void ShouldReturnToppingComponentsUnchangedIfThereAreNoSizes()
        {

            var productDetails = new StoreMenu.Api.v1.Product.Models.ProductDetails
            {
                CountryCode = "JP",
                Sizes = null,
            };

            var results = transformer.AddRecipeComponentsToToppings(productDetails);

            productDetails.Sizes.ShouldBeNull();
            results.Sizes.ShouldBeNull();

        }

        [Fact]
        public void ShouldReturnSizeUnchangedIfThereAreNoToppingsOnASize()
        {

            var productDetails = new StoreMenu.Api.v1.Product.Models.ProductDetails
            {
                CountryCode = "JP",
                Sizes = new List<StoreMenu.Api.v1.Product.Models.ProductSize> {
                    new StoreMenu.Api.v1.Product.Models.ProductSize
                    {
                        Toppings = null
                    }
                },
            };

            var results = transformer.AddRecipeComponentsToToppings(productDetails);

            productDetails.Sizes[0].Toppings.ShouldBeNull();
            results.Sizes[0].Toppings.ShouldBeNull();

        }

        [Fact]
        public void ShouldReturnSizeUnchangedIfThereAreNoRecipeComponents()
        {

            var productDetails = new StoreMenu.Api.v1.Product.Models.ProductDetails
            {
                CountryCode = "JP",
                Sizes = new List<StoreMenu.Api.v1.Product.Models.ProductSize> {
                    new StoreMenu.Api.v1.Product.Models.ProductSize
                    {
                        Toppings = new StoreMenu.Api.v1.Product.Models.ComponentRules {
                            Components = new List<StoreMenu.Api.v1.Product.Models.ComponentServing>()
                        }
                    }
                },
                RecipeComponents = null

            };

            var results = transformer.AddRecipeComponentsToToppings(productDetails);

            productDetails.Sizes[0].Toppings.Components.Count.ShouldBe(0);
            results.Sizes[0].Toppings.Components.Count.ShouldBe(0);

        }

        [Fact]
        public void ShouldReturnAdjustedToppingWithRecipeComponents()
        {

            var productDetails = new StoreMenu.Api.v1.Product.Models.ProductDetails
            {
                CountryCode = "JP",
                Sizes = new List<StoreMenu.Api.v1.Product.Models.ProductSize> {
                    new StoreMenu.Api.v1.Product.Models.ProductSize
                    {
                        Toppings = new StoreMenu.Api.v1.Product.Models.ComponentRules {
                            Components = new List<StoreMenu.Api.v1.Product.Models.ComponentServing>()
                        }
                    }
                },
                RecipeComponents = new List<StoreMenu.Api.v1.Product.Models.ComponentServing> {
                    new StoreMenu.Api.v1.Product.Models.ComponentServing { ComponentCode = "1"},
                    new StoreMenu.Api.v1.Product.Models.ComponentServing { ComponentCode = "2"}
                }

            };

            var results = transformer.AddRecipeComponentsToToppings(productDetails);

            productDetails.Sizes[0].Toppings.Components.Count.ShouldBe(2);
            results.Sizes[0].Toppings.Components.Count.ShouldBe(2);

        }
    }
}
