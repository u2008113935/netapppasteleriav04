using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Tests.Helpers;
using FluentAssertions;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.IntegrationTests.Flows
{
    [Trait("Category", "Integration")]
    public class CatalogToCartFlowTests : IAsyncLifetime //IDisposable
    {
        private readonly CartService _cartService;

        public CatalogToCartFlowTests()
        {
            _cartService = CartService.Instance;
            //_cartService.Clear();
        }

        /*
        public void Dispose()
        {
            _cartService.Clear();
        }
        
         */

        // IAsyncLifetime setup: se ejecuta antes de cada test
        public async Task InitializeAsync()
        {
            // Asegurarse de partir de un estado limpio
            var clearAsync = _cartService.GetType().GetMethod("ClearAsync");
            if (clearAsync != null)
                await _cartService.ClearAsync();
            else
                _cartService.Clear();
        }


        // IAsyncLifetime teardown: se ejecuta después de cada test
        public async Task DisposeAsync()
        {
            var clearAsync = _cartService.GetType().GetMethod("ClearAsync");
            if (clearAsync != null)
                await _cartService.ClearAsync();
            else
                _cartService.Clear();
        }

        // Helper: agrega item usando AddAsync si existe, si no usa Add
        private async Task AddToCartAsync(Product product, int qty)
        {
            var addAsync = _cartService.GetType().GetMethod("AddAsync");
            if (addAsync != null)
                await _cartService.AddAsync(product, qty);
            else
                _cartService.Add(product, qty);
        }

        private async Task RemoveFromCartAsync(Guid productId)
        {
            var removeAsync = _cartService.GetType().GetMethod("RemoveAsync");
            if (removeAsync != null)
                await _cartService.RemoveAsync(productId);
            else
                _cartService.Remove(productId);
        }

        private async Task UpdateQuantityOrSetPropertyAsync(Guid productId, int qty)
        {
            var updateAsync = _cartService.GetType().GetMethod("UpdateQuantityAsync");
            var item = _cartService.Items.FirstOrDefault(i => i.ProductId == productId);
            if (updateAsync != null)
            {
                await _cartService.UpdateQuantityAsync(productId, qty);
            }
            else if (item != null)
            {
                // asignar propiedad (dispara persistencia si el servicio escucha PropertyChanged)
                item.Quantity = qty;
            }
            else
            {
                // si no existe el item, no hacer nada
            }
        }

        [Fact]
        public void Test_BrowseCatalog_AddMultipleProducts_VerifyCart()
        {
            // Arrange - Simulate browsing catalog
            var product1 = TestDataFactory.CreateValidProduct("Torta de Chocolate", 45.00m);
            var product2 = TestDataFactory.CreateValidProduct("Pastel de Fresa", 35.00m);
            var product3 = TestDataFactory.CreateValidProduct("Brownie", 15.00m);

            // Act - Add products to cart (async)
            if (_cartService.GetType().GetMethod("AddAsync") != null)
            {
                _cartService.Add(product1, 2);
                _cartService.Add(product2, 1);
                _cartService.Add(product3, 3);
            }
            else
            {
                // fallback a API sincrónica si existe
                _cartService.Add(product1, 2);
                _cartService.Add(product2, 1);
                _cartService.Add(product3, 3);
            }

            // Verifica
            _cartService.Items.Count.Should().Be(3);
            _cartService.Count.Should().Be(6); // 2 + 1 + 3
            _cartService.Total.Should().Be(170.00m); // (45*2) + (35*1) + (15*3)

            var tortaItem = _cartService.Items.FirstOrDefault(i => i.ProductId == product1.Id);
            tortaItem.Should().NotBeNull();
            tortaItem!.Quantity.Should().Be(2);
        }

        [Fact]
        public async Task Test_SearchProducts_AddToCart_VerifyFiltering()
        {
            // Arrange - Create list of products (simulating catalog)
            var products = TestDataFactory.CreateProductList(10);
            var selectedProduct = products[3];

            // Act - Add selected product to cart
            if (_cartService.GetType().GetMethod("AddAsync") != null)
                await _cartService.AddAsync(selectedProduct, 1);
            else
                _cartService.Add(selectedProduct, 1);

            // Assert
            _cartService.Items.Should().ContainSingle();
            _cartService.Items.First().ProductId.Should().Be(selectedProduct.Id);
            _cartService.Total.Should().Be(selectedProduct.Precio ?? 0m);
        }

        [Fact]
        public async Task Test_AddSameProductTwice_QuantityIncreases()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct("Torta Especial", 50.00m);

            // Act - Add same product twice
            if (_cartService.GetType().GetMethod("AddAsync") != null)
            {
                await _cartService.AddAsync(product, 1);
                var countAfterFirstAdd = _cartService.Items.Count;

                await _cartService.AddAsync(product, 2);
                var countAfterSecondAdd = _cartService.Items.Count;

                // Assert - Should not create duplicate item
                countAfterFirstAdd.Should().Be(1);
                countAfterSecondAdd.Should().Be(1);
            }
            else
            {
                _cartService.Add(product, 1);
                var countAfterFirstAdd = _cartService.Items.Count;

                _cartService.Add(product, 2);
                var countAfterSecondAdd = _cartService.Items.Count;

                countAfterFirstAdd.Should().Be(1);
                countAfterSecondAdd.Should().Be(1);
            }

            var item = _cartService.Items.First();
            item.Quantity.Should().Be(3); // 1 + 2
            _cartService.Total.Should().Be(150.00m); // 50 * 3

        }

        [Fact]
        public async Task Test_AddToCart_ModifyQuantity_VerifyTotal()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct("Pastel", 25.00m);

            if (_cartService.GetType().GetMethod("AddAsync") != null)
                await _cartService.AddAsync(product, 2);
            else
                _cartService.Add(product, 2);

            // Act - Modify quantity
            var item = _cartService.Items.First(i => i.ProductId == product.Id);

            // Preferible: usar UpdateQuantityAsync si existe
            if (_cartService.GetType().GetMethod("UpdateQuantityAsync") != null)
            {
                await _cartService.UpdateQuantityAsync(product.Id, 5);
            }
            else
            {
                // fallback: asignar la propiedad Quantity directamente
                item.Quantity = 5;
            }

            // Assert
            item.Quantity.Should().Be(5);
            _cartService.Total.Should().Be(125.00m);
        }

        [Fact]
        public async Task Test_AddToCart_RemoveItem_VerifyCart()
        {
            // Arrange
            var product1 = TestDataFactory.CreateValidProduct("Producto 1", 10.00m);
            var product2 = TestDataFactory.CreateValidProduct("Producto 2", 20.00m);

            if (_cartService.GetType().GetMethod("AddAsync") != null)
            {
                await _cartService.AddAsync(product1, 1);
                await _cartService.AddAsync(product2, 1);
            }
            else
            {
                _cartService.Add(product1, 1);
                _cartService.Add(product2, 1);
            }

            // Act - Remove first product (async)
            if (_cartService.GetType().GetMethod("RemoveAsync") != null)
                await _cartService.RemoveAsync(product1.Id);
            else
                _cartService.Remove(product1.Id);

            // Assert
            _cartService.Items.Should().ContainSingle();
            _cartService.Items.Should().NotContain(i => i.ProductId == product1.Id);
            _cartService.Total.Should().Be(20.00m);
        }
    }
}
