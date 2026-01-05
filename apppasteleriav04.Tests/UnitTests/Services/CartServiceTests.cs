using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Tests.Helpers;
using FluentAssertions;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.UnitTests.Services
{
    public class CartServiceTests : IDisposable
    {
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            // Create a new instance for testing instead of using singleton
            // Note: Since CartService is a singleton, we'll test with the Instance
            _cartService = CartService.Instance;
            _cartService.Clear();
        }

        public void Dispose()
        {
            _cartService.Clear();
        }

        [Fact]
        public void Test_Add_NewProduct_AddsToItems()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            var initialCount = _cartService.Items.Count;

            // Act
            _cartService.Add(product, 1);

            // Assert
            _cartService.Items.Count.Should().Be(initialCount + 1);
            _cartService.Items.Should().Contain(i => i.ProductId == product.Id);
        }

        [Fact]
        public void Test_Add_ExistingProduct_IncreasesQuantity()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 2);
            var itemBeforeAdd = _cartService.Items.First(i => i.ProductId == product.Id);
            var initialQuantity = itemBeforeAdd.Quantity;

            // Act
            _cartService.Add(product, 3);

            // Assert
            var item = _cartService.Items.First(i => i.ProductId == product.Id);
            item.Quantity.Should().Be(initialQuantity + 3);
            _cartService.Items.Count.Should().Be(1); // Should not add duplicate
        }

        [Fact]
        public void Test_Remove_ExistingProduct_RemovesFromItems()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 1);
            var countAfterAdd = _cartService.Items.Count;

            // Act
            _cartService.Remove(product.Id);

            // Assert
            _cartService.Items.Count.Should().Be(countAfterAdd - 1);
            _cartService.Items.Should().NotContain(i => i.ProductId == product.Id);
        }

        [Fact]
        public void Test_UpdateQuantity_ChangesQuantity()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 2);

            // Act
            _cartService.UpdateQuantity(product.Id, 5);

            // Assert
            var item = _cartService.Items.First(i => i.ProductId == product.Id);
            item.Quantity.Should().Be(5);
        }

        [Fact]
        public void Test_UpdateQuantity_ToZero_RemovesItem()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 2);

            // Act
            _cartService.UpdateQuantity(product.Id, 0);

            // Assert
            _cartService.Items.Should().NotContain(i => i.ProductId == product.Id);
        }

        [Fact]
        public void Test_Clear_RemovesAllItems()
        {
            // Arrange
            var product1 = TestDataFactory.CreateValidProduct();
            var product2 = TestDataFactory.CreateValidProduct();
            _cartService.Add(product1, 1);
            _cartService.Add(product2, 2);

            // Act
            _cartService.Clear();

            // Assert
            _cartService.Items.Should().BeEmpty();
        }

        [Fact]
        public void Test_Total_CalculatesCorrectly()
        {
            // Arrange
            _cartService.Clear();
            var product1 = TestDataFactory.CreateValidProduct(precio: 10.00m);
            var product2 = TestDataFactory.CreateValidProduct(precio: 20.00m);
            _cartService.Add(product1, 2); // 20.00
            _cartService.Add(product2, 3); // 60.00

            // Act
            var total = _cartService.Total;

            // Assert
            total.Should().Be(80.00m);
        }

        [Fact]
        public void Test_Count_ReturnsCorrectCount()
        {
            // Arrange
            _cartService.Clear();
            var product1 = TestDataFactory.CreateValidProduct();
            var product2 = TestDataFactory.CreateValidProduct();
            _cartService.Add(product1, 2);
            _cartService.Add(product2, 3);

            // Act
            var count = _cartService.Count;

            // Assert
            count.Should().Be(5); // 2 + 3
        }

        [Fact]
        public void Test_CartChanged_EventRaised_OnAdd()
        {
            // Arrange
            bool eventRaised = false;
            _cartService.CartChanged += (sender, args) => eventRaised = true;
            var product = TestDataFactory.CreateValidProduct();

            // Act
            _cartService.Add(product, 1);

            // Assert
            eventRaised.Should().BeTrue();
        }

        [Fact]
        public void Test_CartChanged_EventRaised_OnRemove()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 1);
            bool eventRaised = false;
            _cartService.CartChanged += (sender, args) => eventRaised = true;

            // Act
            _cartService.Remove(product.Id);

            // Assert
            eventRaised.Should().BeTrue();
        }

        [Fact]
        public async Task Test_SaveLocalAsync_PersistsCart()
        {
            // Arrange
            _cartService.Clear();
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 2);

            // Act
            await _cartService.SaveLocalAsync();

            // Assert - We can't verify Preferences directly in unit tests
            // but we can verify the method completes without error
            _cartService.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Test_LoadLocalAsync_CompletesWithoutError()
        {
            // Arrange & Act
            await _cartService.LoadLocalAsync();

            // Assert - Method should complete without throwing
            // The actual persistence is tested in integration tests
            Assert.True(true);
        }

        [Fact]
        public void Test_ContainsProduct_ReturnsTrueWhenExists()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 1);

            // Act
            var contains = _cartService.ContainsProduct(product.Id);

            // Assert
            contains.Should().BeTrue();
        }

        [Fact]
        public void Test_ContainsProduct_ReturnsFalseWhenNotExists()
        {
            // Arrange
            var randomId = Guid.NewGuid();

            // Act
            var contains = _cartService.ContainsProduct(randomId);

            // Assert
            contains.Should().BeFalse();
        }
    }
}
