using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Tests.Helpers;
using FluentAssertions;
using Xunit;
using System;
using System.Linq;

namespace apppasteleriav04.Tests.IntegrationTests.Flows
{
    [Trait("Category", "Integration")]
    public class CartToCheckoutFlowTests : IDisposable
    {
        private readonly CartService _cartService;

        public CartToCheckoutFlowTests()
        {
            _cartService = CartService.Instance;
            _cartService.Clear();
        }

        public void Dispose()
        {
            _cartService.Clear();
        }

        [Fact]
        public void Test_CartWithItems_ProceedToCheckout_ValidatesCart()
        {
            // Arrange - Prepare cart
            var product1 = TestDataFactory.CreateValidProduct("Torta", 45.00m);
            var product2 = TestDataFactory.CreateValidProduct("Pastel", 30.00m);
            _cartService.Add(product1, 2);
            _cartService.Add(product2, 1);

            // Act - Prepare for checkout
            var items = _cartService.Items.ToList();
            var total = _cartService.Total;

            // Assert - Verify cart is ready for checkout
            items.Should().NotBeEmpty();
            items.Count.Should().Be(2);
            total.Should().Be(120.00m); // (45*2) + (30*1)
        }

        [Fact]
        public void Test_EmptyCart_CannotProceedToCheckout()
        {
            // Arrange
            _cartService.Clear();

            // Act
            var canCheckout = _cartService.Items.Count > 0;

            // Assert
            canCheckout.Should().BeFalse();
        }

        [Fact]
        public void Test_ConvertCartToOrderItems_CreatesCorrectItems()
        {
            // Arrange
            var product1 = TestDataFactory.CreateValidProduct("Item 1", 10.00m);
            var product2 = TestDataFactory.CreateValidProduct("Item 2", 20.00m);
            _cartService.Add(product1, 3);
            _cartService.Add(product2, 2);

            // Act - Convert to order items
            var orderItems = _cartService.ToOrderItems();

            // Assert
            orderItems.Should().NotBeNull();
            orderItems.Length.Should().Be(2);
            
            var item1 = orderItems.FirstOrDefault(i => i.ProductId == product1.Id);
            item1.Should().NotBeNull();
            item1!.Quantity.Should().Be(3);
            item1.Price.Should().Be(10.00m);

            var item2 = orderItems.FirstOrDefault(i => i.ProductId == product2.Id);
            item2.Should().NotBeNull();
            item2!.Quantity.Should().Be(2);
            item2.Price.Should().Be(20.00m);
        }

        [Fact]
        public void Test_CheckoutFlow_CalculatesSubtotalAndShipping()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct("Producto", 25.00m);
            _cartService.Add(product, 4);

            // Act - Simulate checkout calculations
            var subtotal = _cartService.Total;
            var shippingCost = 10.00m; // Fixed shipping for test
            var total = subtotal + shippingCost;

            // Assert
            subtotal.Should().Be(100.00m);
            total.Should().Be(110.00m);
        }
    }
}
