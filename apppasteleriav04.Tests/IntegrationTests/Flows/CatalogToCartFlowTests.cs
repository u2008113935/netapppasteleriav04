using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Tests.Helpers;
using apppasteleriav04.Tests.Mocks;
using FluentAssertions;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.IntegrationTests.Flows
{
    [Trait("Category", "Integration")]
    public class CatalogToCartFlowTests : IDisposable
    {
        private readonly CartService _cartService;

        public CatalogToCartFlowTests()
        {
            _cartService = CartService.Instance;
            _cartService.Clear();
        }

        public void Dispose()
        {
            _cartService.Clear();
        }

        [Fact]
        public void Test_BrowseCatalog_AddMultipleProducts_VerifyCart()
        {
            // Arrange - Simulate browsing catalog
            var product1 = TestDataFactory.CreateValidProduct("Torta de Chocolate", 45.00m);
            var product2 = TestDataFactory.CreateValidProduct("Pastel de Fresa", 35.00m);
            var product3 = TestDataFactory.CreateValidProduct("Brownie", 15.00m);

            // Act - Add products to cart
            _cartService.Add(product1, 2);
            _cartService.Add(product2, 1);
            _cartService.Add(product3, 3);

            // Assert - Verify cart state
            _cartService.Items.Count.Should().Be(3);
            _cartService.Count.Should().Be(6); // 2 + 1 + 3
            _cartService.Total.Should().Be(170.00m); // (45*2) + (35*1) + (15*3)

            var tortaItem = _cartService.Items.FirstOrDefault(i => i.ProductId == product1.Id);
            tortaItem.Should().NotBeNull();
            tortaItem!.Quantity.Should().Be(2);
        }

        [Fact]
        public void Test_SearchProducts_AddToCart_VerifyFiltering()
        {
            // Arrange - Create list of products (simulating catalog)
            var products = TestDataFactory.CreateProductList(10);
            var selectedProduct = products[3];

            // Act - Add selected product to cart
            _cartService.Add(selectedProduct, 1);

            // Assert
            _cartService.Items.Should().ContainSingle();
            _cartService.Items.First().ProductId.Should().Be(selectedProduct.Id);
            _cartService.Total.Should().Be(selectedProduct.Precio ?? 0m);
        }

        [Fact]
        public void Test_AddSameProductTwice_QuantityIncreases()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct("Torta Especial", 50.00m);

            // Act - Add same product twice
            _cartService.Add(product, 1);
            var countAfterFirstAdd = _cartService.Items.Count;
            
            _cartService.Add(product, 2);
            var countAfterSecondAdd = _cartService.Items.Count;

            // Assert - Should not create duplicate item
            countAfterFirstAdd.Should().Be(1);
            countAfterSecondAdd.Should().Be(1);
            
            var item = _cartService.Items.First();
            item.Quantity.Should().Be(3); // 1 + 2
            _cartService.Total.Should().Be(150.00m); // 50 * 3
        }

        [Fact]
        public void Test_AddToCart_ModifyQuantity_VerifyTotal()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct("Pastel", 25.00m);
            _cartService.Add(product, 2);

            // Act - Modify quantity
            _cartService.UpdateQuantity(product.Id, 5);

            // Assert
            var item = _cartService.Items.First();
            item.Quantity.Should().Be(5);
            _cartService.Total.Should().Be(125.00m);
        }

        [Fact]
        public void Test_AddToCart_RemoveItem_VerifyCart()
        {
            // Arrange
            var product1 = TestDataFactory.CreateValidProduct("Producto 1", 10.00m);
            var product2 = TestDataFactory.CreateValidProduct("Producto 2", 20.00m);
            _cartService.Add(product1, 1);
            _cartService.Add(product2, 1);

            // Act - Remove first product
            _cartService.Remove(product1.Id);

            // Assert
            _cartService.Items.Should().ContainSingle();
            _cartService.Items.Should().NotContain(i => i.ProductId == product1.Id);
            _cartService.Total.Should().Be(20.00m);
        }
    }
}
