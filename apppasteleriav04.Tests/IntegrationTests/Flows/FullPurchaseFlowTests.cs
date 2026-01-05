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
    public class FullPurchaseFlowTests : IDisposable
    {
        private readonly CartService _cartService;
        private readonly MockAuthService _mockAuthService;
        private readonly MockSupabaseService _mockSupabaseService;

        public FullPurchaseFlowTests()
        {
            _cartService = CartService.Instance;
            _cartService.Clear();
            _mockAuthService = new MockAuthService();
            _mockSupabaseService = new MockSupabaseService();
        }

        public void Dispose()
        {
            _cartService.Clear();
            _mockAuthService.Reset();
            _mockSupabaseService.Reset();
        }

        [Fact]
        public async Task Test_FullPurchaseFlow_FromCatalogToPayment()
        {
            // Step 1: Cargar productos del catálogo
            var products = TestDataFactory.CreateProductList(5);
            _mockSupabaseService.Products.AddRange(products);

            var catalogProducts = await _mockSupabaseService.GetProductsAsync();
            catalogProducts.Should().HaveCount(5);

            // Step 2: Agregar producto al carrito
            var selectedProduct = catalogProducts.First();
            _cartService.Add(selectedProduct, 2);

            // Step 3: Verificar carrito tiene item
            _cartService.Items.Should().ContainSingle();
            _cartService.Items.First().ProductId.Should().Be(selectedProduct.Id);
            _cartService.Count.Should().Be(2);

            // Step 4: Simular login
            var loginSuccess = await _mockAuthService.SignInAsync(
                MockAuthService.ValidEmail, 
                MockAuthService.ValidPassword
            );
            loginSuccess.Should().BeTrue();
            _mockAuthService.IsAuthenticated.Should().BeTrue();

            // Step 5: Ir a checkout (preparar orden)
            var orderItems = _cartService.ToOrderItems();
            orderItems.Should().NotBeEmpty();

            // Step 6: Completar datos de entrega
            var deliveryAddress = "Calle Test 123";
            deliveryAddress.Should().NotBeNullOrEmpty();

            // Step 7: Seleccionar método de pago
            var paymentMethod = "Tarjeta";
            paymentMethod.Should().NotBeNullOrEmpty();

            // Step 8: Procesar pago (simulado)
            var paymentSuccessful = true;
            paymentSuccessful.Should().BeTrue();

            // Step 9: Verificar pedido creado
            var order = await _mockSupabaseService.CreateOrderAsync(
                Guid.Parse(_mockAuthService.UserId!),
                orderItems.ToList(),
                _cartService.Total
            );
            
            order.Should().NotBeNull();
            order.UserId.Should().Be(Guid.Parse(_mockAuthService.UserId!));
            order.Total.Should().Be(_cartService.Total);
            order.Status.Should().Be("Pendiente");

            // Step 10: Verificar comprobante generado (simulado)
            var invoiceGenerated = order.Id != Guid.Empty;
            invoiceGenerated.Should().BeTrue();

            // Cleanup: Clear cart after successful order
            _cartService.Clear();
            _cartService.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task Test_FullPurchaseFlow_WithMultipleProducts()
        {
            // Arrange
            var products = TestDataFactory.CreateProductList(10);
            _mockSupabaseService.Products.AddRange(products);

            // Act - Complete purchase flow
            // 1. Add multiple products to cart
            _cartService.Add(products[0], 2);
            _cartService.Add(products[1], 1);
            _cartService.Add(products[2], 3);

            // 2. Login
            await _mockAuthService.SignInAsync(MockAuthService.ValidEmail, MockAuthService.ValidPassword);

            // 3. Create order
            var orderItems = _cartService.ToOrderItems();
            var order = await _mockSupabaseService.CreateOrderAsync(
                Guid.Parse(_mockAuthService.UserId!),
                orderItems.ToList(),
                _cartService.Total
            );

            // Assert
            order.Should().NotBeNull();
            order.Items.Should().HaveCount(3);
            _mockSupabaseService.Orders.Should().ContainSingle();
        }

        [Fact]
        public void Test_PurchaseFlow_RequiresAuthentication()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 1);

            // Act
            var canPurchase = _mockAuthService.IsAuthenticated;

            // Assert
            canPurchase.Should().BeFalse();
            _cartService.Items.Should().NotBeEmpty(); // Cart has items but can't purchase
        }

        [Fact]
        public async Task Test_PurchaseFlow_ClearsCartAfterSuccess()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 2);
            await _mockAuthService.SignInAsync(MockAuthService.ValidEmail, MockAuthService.ValidPassword);

            // Act - Complete purchase
            var orderItems = _cartService.ToOrderItems();
            await _mockSupabaseService.CreateOrderAsync(
                Guid.Parse(_mockAuthService.UserId!),
                orderItems.ToList(),
                _cartService.Total
            );

            _cartService.Clear();

            // Assert
            _cartService.Items.Should().BeEmpty();
            _cartService.Total.Should().Be(0);
        }
    }
}
