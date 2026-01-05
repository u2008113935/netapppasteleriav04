using apppasteleriav04.Models.Domain;
using apppasteleriav04.Tests.Helpers;
using FluentAssertions;
using Xunit;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.UnitTests.Models
{
    public class CartItemTests
    {
        [Fact]
        public void Test_CartItem_Subtotal_CalculatesCorrectly()
        {
            // Arrange
            var cartItem = TestDataFactory.CreateCartItem(price: 25.00m, quantity: 3);

            // Act
            var subtotal = cartItem.Subtotal;

            // Assert
            subtotal.Should().Be(75.00m);
        }

        [Fact]
        public async Task Test_CartItem_PropertyChanged_WhenQuantityChanges()
        {
            // Arrange
            var cartItem = TestDataFactory.CreateCartItem(quantity: 1);
            bool quantityChanged = false;
            bool subtotalChanged = false;

            cartItem.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CartItem.Quantity))
                    quantityChanged = true;
                if (e.PropertyName == nameof(CartItem.Subtotal))
                    subtotalChanged = true;
            };

            // Act
            cartItem.Quantity = 5;
            await Task.Delay(50); // Allow events to fire

            // Assert
            quantityChanged.Should().BeTrue();
            subtotalChanged.Should().BeTrue();
        }

        [Fact]
        public async Task Test_CartItem_PropertyChanged_WhenPriceChanges()
        {
            // Arrange
            var cartItem = TestDataFactory.CreateCartItem(price: 10.00m);
            bool priceChanged = false;
            bool subtotalChanged = false;

            cartItem.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CartItem.Price))
                    priceChanged = true;
                if (e.PropertyName == nameof(CartItem.Subtotal))
                    subtotalChanged = true;
            };

            // Act
            cartItem.Price = 20.00m;
            await Task.Delay(50); // Allow events to fire

            // Assert
            priceChanged.Should().BeTrue();
            subtotalChanged.Should().BeTrue();
        }

        [Fact]
        public void Test_CartItem_Subtotal_UpdatesOnQuantityChange()
        {
            // Arrange
            var cartItem = TestDataFactory.CreateCartItem(price: 15.00m, quantity: 2);
            var initialSubtotal = cartItem.Subtotal;

            // Act
            cartItem.Quantity = 5;
            var newSubtotal = cartItem.Subtotal;

            // Assert
            initialSubtotal.Should().Be(30.00m);
            newSubtotal.Should().Be(75.00m);
        }

        [Fact]
        public void Test_CartItem_Subtotal_UpdatesOnPriceChange()
        {
            // Arrange
            var cartItem = TestDataFactory.CreateCartItem(price: 10.00m, quantity: 3);
            var initialSubtotal = cartItem.Subtotal;

            // Act
            cartItem.Price = 20.00m;
            var newSubtotal = cartItem.Subtotal;

            // Assert
            initialSubtotal.Should().Be(30.00m);
            newSubtotal.Should().Be(60.00m);
        }

        [Fact]
        public void Test_CartItem_NoPropertyChanged_WhenSameValue()
        {
            // Arrange
            var cartItem = TestDataFactory.CreateCartItem(price: 10.00m, quantity: 2);
            int eventCount = 0;

            cartItem.PropertyChanged += (sender, e) =>
            {
                eventCount++;
            };

            // Act
            cartItem.Quantity = 2; // Same value
            cartItem.Price = 10.00m; // Same value

            // Assert
            eventCount.Should().Be(0);
        }
    }
}
