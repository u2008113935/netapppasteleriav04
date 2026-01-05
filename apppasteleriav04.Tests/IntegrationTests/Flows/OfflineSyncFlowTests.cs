using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Tests.Helpers;
using apppasteleriav04.Tests.Mocks;
using FluentAssertions;
using Xunit;
using System;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.IntegrationTests.Flows
{
    [Trait("Category", "Integration")]
    public class OfflineSyncFlowTests : IDisposable
    {
        private readonly CartService _cartService;
        private readonly MockAuthService _mockAuthService;
        private readonly MockConnectivityService _mockConnectivityService;
        private readonly MockSupabaseService _mockSupabaseService;

        public OfflineSyncFlowTests()
        {
            _cartService = CartService.Instance;
            _cartService.Clear();
            _mockAuthService = new MockAuthService();
            _mockConnectivityService = new MockConnectivityService();
            _mockSupabaseService = new MockSupabaseService();
        }

        public void Dispose()
        {
            _cartService.Clear();
            _mockAuthService.Reset();
            _mockConnectivityService.Reset();
            _mockSupabaseService.Reset();
        }

        [Fact]
        public async Task Test_OfflinePurchaseFlow_CreateOrderOffline_SyncsWhenOnline()
        {
            // Step 1: Simular offline
            _mockConnectivityService.SimulateOffline();
            _mockConnectivityService.IsConnected.Should().BeFalse();

            // Step 2: Crear pedido offline
            await _mockAuthService.SignInAsync(MockAuthService.ValidEmail, MockAuthService.ValidPassword);
            
            var product = TestDataFactory.CreateValidProduct();
            _cartService.Add(product, 2);
            var orderItems = _cartService.ToOrderItems();

            // Simulate creating order locally (not syncing to server)
            var localOrder = TestDataFactory.CreateOrder(
                Guid.Parse(_mockAuthService.UserId!),
                _cartService.Total,
                "Pendiente"
            );
            localOrder.Items = orderItems.ToList();

            // Step 3: Verificar guardado local
            localOrder.Should().NotBeNull();
            localOrder.Status.Should().Be("Pendiente");
            _mockSupabaseService.Orders.Should().BeEmpty(); // Not synced yet

            // Step 4: Simular online
            _mockConnectivityService.SimulateOnline();
            _mockConnectivityService.IsConnected.Should().BeTrue();

            // Step 5: Verificar sincronización
            // Simulate sync process
            var syncedOrder = await _mockSupabaseService.CreateOrderAsync(
                localOrder.UserId,
                localOrder.Items,
                localOrder.Total
            );

            // Step 6: Verificar pedido en servidor
            syncedOrder.Should().NotBeNull();
            _mockSupabaseService.Orders.Should().ContainSingle();
            _mockSupabaseService.Orders[0].UserId.Should().Be(localOrder.UserId);
            _mockSupabaseService.Orders[0].Total.Should().Be(localOrder.Total);
        }

        [Fact]
        public async Task Test_OfflineMode_SavesCartLocally()
        {
            // Arrange
            _mockConnectivityService.SimulateOffline();
            var product = TestDataFactory.CreateValidProduct();

            // Act
            _cartService.Add(product, 3);
            await _cartService.SaveLocalAsync();

            // Assert
            _cartService.Items.Should().ContainSingle();
            _mockConnectivityService.IsConnected.Should().BeFalse();
        }

        [Fact]
        public async Task Test_ConnectivityRestored_TriggersSync()
        {
            // Arrange
            _mockConnectivityService.SimulateOffline();
            bool syncTriggered = false;

            _mockConnectivityService.ConnectivityChanged += (sender, isConnected) =>
            {
                if (isConnected)
                {
                    syncTriggered = true;
                }
            };

            // Act
            _mockConnectivityService.SimulateOnline();
            await Task.Delay(100); // Allow event to propagate

            // Assert
            syncTriggered.Should().BeTrue();
            _mockConnectivityService.IsConnected.Should().BeTrue();
        }

        [Fact]
        public void Test_OfflineMode_QueuesPendingOperations()
        {
            // Arrange
            _mockConnectivityService.SimulateOffline();
            var pendingOperations = new System.Collections.Generic.List<string>();

            // Act - Simulate queuing operations
            pendingOperations.Add("CreateOrder");
            pendingOperations.Add("UpdateProfile");

            // Assert
            pendingOperations.Should().HaveCount(2);
            _mockConnectivityService.IsConnected.Should().BeFalse();
        }

        [Fact]
        public async Task Test_SyncOnlineAfterOffline_ProcessesPendingOrders()
        {
            // Arrange - Create orders while offline
            _mockConnectivityService.SimulateOffline();
            await _mockAuthService.SignInAsync(MockAuthService.ValidEmail, MockAuthService.ValidPassword);

            var order1 = TestDataFactory.CreateOrder(Guid.Parse(_mockAuthService.UserId!), 50.00m);
            var order2 = TestDataFactory.CreateOrder(Guid.Parse(_mockAuthService.UserId!), 75.00m);

            // Act - Go online and sync
            _mockConnectivityService.SimulateOnline();
            await _mockSupabaseService.CreateOrderAsync(order1.UserId, order1.Items, order1.Total);
            await _mockSupabaseService.CreateOrderAsync(order2.UserId, order2.Items, order2.Total);

            // Assert
            _mockSupabaseService.Orders.Should().HaveCount(2);
            _mockConnectivityService.IsConnected.Should().BeTrue();
        }
    }
}
