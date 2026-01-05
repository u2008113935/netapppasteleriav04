using apppasteleriav04.Models.Domain;
using apppasteleriav04.Tests.Helpers;
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;

namespace apppasteleriav04.Tests.UnitTests.Models
{
    public class OrderTests
    {
        [Fact]
        public void Test_Order_DefaultStatus_IsPendiente()
        {
            // Arrange & Act
            var order = TestDataFactory.CreateOrder();

            // Assert
            order.Status.Should().Be("Pendiente");
        }

        [Fact]
        public void Test_Order_Items_DefaultsToEmptyList()
        {
            // Arrange & Act
            var order = new Order();

            // Assert
            order.Items.Should().NotBeNull();
            order.Items.Should().BeEmpty();
        }

        [Fact]
        public void Test_Order_WithItems_HasCorrectCount()
        {
            // Arrange
            var order = TestDataFactory.CreateOrder();
            order.Items = TestDataFactory.CreateOrderItems(3, order.Id);

            // Act
            var itemCount = order.Items.Count;

            // Assert
            itemCount.Should().Be(3);
        }

        [Fact]
        public void Test_Order_CreatedAt_IsSet()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;
            
            // Act
            var order = TestDataFactory.CreateOrder();
            var afterCreation = DateTime.UtcNow;

            // Assert
            order.CreatedAt.Should().BeOnOrAfter(beforeCreation);
            order.CreatedAt.Should().BeOnOrBefore(afterCreation);
        }

        [Fact]
        public void Test_Order_WithCustomStatus_ReturnsCustomStatus()
        {
            // Arrange & Act
            var order = TestDataFactory.CreateOrder(status: "Completado");

            // Assert
            order.Status.Should().Be("Completado");
        }

        [Fact]
        public void Test_Order_Properties_AreSettable()
        {
            // Arrange
            var order = new Order();
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            // Act
            order.Id = orderId;
            order.UserId = userId;
            order.Total = 150.50m;
            order.Status = "En Proceso";
            order.CreatedAt = DateTime.UtcNow;

            // Assert
            order.Id.Should().Be(orderId);
            order.UserId.Should().Be(userId);
            order.Total.Should().Be(150.50m);
            order.Status.Should().Be("En Proceso");
            order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
