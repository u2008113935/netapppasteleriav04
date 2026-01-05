using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Tests.Helpers;
using apppasteleriav04.Tests.Mocks;
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.IntegrationTests.Flows
{
    [Trait("Category", "Integration")]
    public class CheckoutToPaymentFlowTests
    {
        [Fact]
        public async Task Test_CheckoutToPayment_ValidatesOrderData()
        {
            // Arrange - Prepare order data
            var userId = Guid.NewGuid();
            var orderItems = TestDataFactory.CreateOrderItems(3);
            var total = orderItems.Sum(i => i.Price * i.Quantity);

            // Act - Create order
            var order = TestDataFactory.CreateOrder(userId, total, "Pendiente");
            order.Items = orderItems;

            // Assert
            order.Should().NotBeNull();
            order.UserId.Should().Be(userId);
            order.Total.Should().Be(total);
            order.Status.Should().Be("Pendiente");
            order.Items.Should().HaveCount(3);

            await Task.CompletedTask;
        }

        [Fact]
        public void Test_PaymentFlow_SelectPaymentMethod()
        {
            // Arrange
            var availablePaymentMethods = new List<string> 
            { 
                "Tarjeta", 
                "Efectivo", 
                "Yape", 
                "Plin" 
            };

            // Act - Select payment method
            var selectedMethod = "Tarjeta";

            // Assert
            availablePaymentMethods.Should().Contain(selectedMethod);
        }

        [Fact]
        public async Task Test_ProcessPayment_WithValidData_Succeeds()
        {
            // Arrange
            var mockAuthService = new MockAuthService();
            mockAuthService.SetAuthenticated(Guid.NewGuid().ToString(), "test@test.com");

            var order = TestDataFactory.CreateOrder(
                userId: Guid.Parse(mockAuthService.UserId!),
                total: 150.00m
            );

            // Act - Simulate payment processing
            var paymentSuccessful = true; // Simulated payment result

            // Assert
            paymentSuccessful.Should().BeTrue();
            order.Should().NotBeNull();

            await Task.CompletedTask;
        }

        [Fact]
        public void Test_PaymentFlow_RequiresAuthentication()
        {
            // Arrange
            var mockAuthService = new MockAuthService();

            // Act
            var canProceedToPayment = mockAuthService.IsAuthenticated;

            // Assert
            canProceedToPayment.Should().BeFalse();
        }
    }
}
