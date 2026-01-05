using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using apppasteleriav04.Data.Local.Database;
using apppasteleriav04.Data.Local.Repositories;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Models.Local;
using apppasteleriav04.Services.Connectivity;
using apppasteleriav04.Services.Core;

namespace apppasteleriav04.Services.Sync
{
    /// <summary>
    /// Service responsible for synchronizing data between local SQLite and remote Supabase
    /// </summary>
    public class SyncService : ISyncService
    {
        private readonly AppDatabase _database;
        private readonly LocalProductRepository _productRepo;
        private readonly LocalOrderRepository _orderRepo;
        private readonly IConnectivityService _connectivityService;
        private readonly SupabaseService _supabaseService;
        private bool _isAutoSyncEnabled;
        private int _pendingSyncCount;
        private bool _isSyncing;

        public event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;

        public int PendingSyncCount => _pendingSyncCount;
        public bool IsSyncing => _isSyncing;

        public SyncService(
            IConnectivityService? connectivityService = null,
            SupabaseService? supabaseService = null)
        {
            _database = AppDatabase.Instance;
            _productRepo = new LocalProductRepository();
            _orderRepo = new LocalOrderRepository();
            _connectivityService = connectivityService ?? new ConnectivityService();
            _supabaseService = supabaseService ?? SupabaseService.Instance;

            Debug.WriteLine("[SyncService] Initialized");
        }

        public void StartAutoSync()
        {
            if (_isAutoSyncEnabled)
                return;

            Debug.WriteLine("[SyncService] Starting auto-sync monitoring");

            _connectivityService.ConnectivityChanged += OnConnectivityChanged;
            _connectivityService.StartMonitoring();
            _isAutoSyncEnabled = true;

            // Initial sync if connected
            if (_connectivityService.IsConnected)
            {
                _ = Task.Run(async () => await SyncPendingAsync());
            }
        }

        public void StopAutoSync()
        {
            if (!_isAutoSyncEnabled)
                return;

            Debug.WriteLine("[SyncService] Stopping auto-sync monitoring");

            _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
            _connectivityService.StopMonitoring();
            _isAutoSyncEnabled = false;
        }

        private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                Debug.WriteLine("[SyncService] Connectivity restored, starting sync...");
                await SyncPendingAsync();
            }
        }

        public async Task<SyncResult> SyncPendingAsync()
        {
            if (_isSyncing)
            {
                Debug.WriteLine("[SyncService] Sync already in progress, skipping");
                return new SyncResult { Success = false, Errors = new List<string> { "Sync already in progress" } };
            }

            if (!_connectivityService.IsConnected)
            {
                Debug.WriteLine("[SyncService] No internet connection, skipping sync");
                return new SyncResult { Success = false, Errors = new List<string> { "No internet connection" } };
            }

            _isSyncing = true;
            NotifyStatusChanged(null, true);

            var result = new SyncResult();

            try
            {
                Debug.WriteLine("[SyncService] Starting sync of pending items");

                var db = await _database.GetConnectionAsync();
                var pendingItems = await db.Table<SyncQueue>()
                    .Where(q => !q.IsSynced)
                    .OrderBy(q => q.Priority)
                    .ThenBy(q => q.CreatedAt)
                    .ToListAsync();

                Debug.WriteLine($"[SyncService] Found {pendingItems.Count} items to sync");

                foreach (var item in pendingItems)
                {
                    try
                    {
                        bool success = await SyncItemAsync(item);

                        if (success)
                        {
                            item.IsSynced = true;
                            item.SyncedAt = DateTime.UtcNow;
                            item.ErrorMessage = null;
                            await db.UpdateAsync(item);

                            result.ItemsSynced++;
                            Debug.WriteLine($"[SyncService] Successfully synced {item.EntityType} #{item.LocalEntityId}");
                        }
                        else
                        {
                            item.RetryCount++;
                            item.ErrorMessage = "Sync failed";
                            await db.UpdateAsync(item);

                            result.ItemsFailed++;
                            result.Errors.Add($"Failed to sync {item.EntityType} #{item.LocalEntityId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SyncService] Error syncing item {item.Id}: {ex}");
                        item.RetryCount++;
                        item.ErrorMessage = ex.Message;
                        await db.UpdateAsync(item);

                        result.ItemsFailed++;
                        result.Errors.Add($"Error syncing {item.EntityType}: {ex.Message}");
                    }
                }

                result.Success = result.ItemsFailed == 0;
                await UpdatePendingCountAsync();

                Debug.WriteLine($"[SyncService] Sync completed: {result.ItemsSynced} succeeded, {result.ItemsFailed} failed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error during sync: {ex}");
                result.Success = false;
                result.Errors.Add($"Sync error: {ex.Message}");
            }
            finally
            {
                _isSyncing = false;
                NotifyStatusChanged(null, false);
            }

            return result;
        }

        private async Task<bool> SyncItemAsync(SyncQueue item)
        {
            try
            {
                switch (item.EntityType)
                {
                    case "Order":
                        return await SyncOrderAsync(item);

                    case "Transaction":
                    case "Payment":
                        // TODO: Implement transaction/payment sync
                        Debug.WriteLine($"[SyncService] {item.EntityType} sync not yet implemented");
                        return false;

                    default:
                        Debug.WriteLine($"[SyncService] Unknown entity type: {item.EntityType}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error syncing item: {ex}");
                return false;
            }
        }

        private async Task<bool> SyncOrderAsync(SyncQueue syncItem)
        {
            try
            {
                var localOrder = await _orderRepo.GetByIdAsync(syncItem.LocalEntityId);

                if (localOrder == null)
                {
                    Debug.WriteLine($"[SyncService] Local order {syncItem.LocalEntityId} not found");
                    return false;
                }

                // Convert local order items to remote format
                var orderItems = localOrder.Items.Select(item => new OrderItem
                {
                    ProductId = Guid.Parse(item.ProductId),
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList();

                // Create order on server
                var remoteOrder = await _supabaseService.CreateOrderAsync(
                    Guid.Parse(localOrder.UserId),
                    orderItems);

                if (remoteOrder != null && remoteOrder.Id != Guid.Empty)
                {
                    // Mark local order as synced
                    await _orderRepo.MarkAsSyncedAsync(localOrder.Id, remoteOrder.Id.ToString());

                    Debug.WriteLine($"[SyncService] Order {localOrder.Id} synced successfully as remote order {remoteOrder.Id}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error syncing order: {ex}");
                return false;
            }
        }

        public async Task<bool> SyncProductsAsync()
        {
            try
            {
                if (!_connectivityService.IsConnected)
                {
                    Debug.WriteLine("[SyncService] No connection, cannot sync products");
                    return false;
                }

                Debug.WriteLine("[SyncService] Syncing products from server");

                var remoteProducts = await _supabaseService.GetProductsAsync();

                if (remoteProducts == null || remoteProducts.Count == 0)
                {
                    Debug.WriteLine("[SyncService] No products received from server");
                    return false;
                }

                // Convert and save to local database
                foreach (var remoteProduct in remoteProducts)
                {
                    var localProduct = new LocalProduct
                    {
                        Id = remoteProduct.Id.ToString(),
                        Nombre = remoteProduct.Nombre ?? "",
                        Descripcion = remoteProduct.Descripcion,
                        Categoria = remoteProduct.Categoria,
                        ImagenUrl = remoteProduct.ImagenPath,
                        Precio = remoteProduct.Precio ?? 0,
                        LastSynced = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _productRepo.InsertOrReplaceAsync(localProduct);
                }

                Debug.WriteLine($"[SyncService] Successfully synced {remoteProducts.Count} products");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error syncing products: {ex}");
                return false;
            }
        }

        public async Task<bool> EnqueueOrderAsync(int localOrderId)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(localOrderId);

                if (order == null)
                {
                    Debug.WriteLine($"[SyncService] Order {localOrderId} not found");
                    return false;
                }

                var db = await _database.GetConnectionAsync();

                var syncItem = new SyncQueue
                {
                    EntityType = "Order",
                    LocalEntityId = localOrderId,
                    Operation = "INSERT",
                    JsonData = JsonSerializer.Serialize(order),
                    Priority = 1, // High priority for orders
                    CreatedAt = DateTime.UtcNow
                };

                await db.InsertAsync(syncItem);
                await UpdatePendingCountAsync();

                Debug.WriteLine($"[SyncService] Order {localOrderId} added to sync queue");

                // Try to sync immediately if connected
                if (_connectivityService.IsConnected)
                {
                    _ = Task.Run(async () => await SyncPendingAsync());
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error enqueueing order: {ex}");
                return false;
            }
        }

        public async Task<int> GetPendingCountAsync()
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                return await db.Table<SyncQueue>()
                    .Where(q => !q.IsSynced)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error getting pending count: {ex}");
                return 0;
            }
        }

        private async Task UpdatePendingCountAsync()
        {
            _pendingSyncCount = await GetPendingCountAsync();
            NotifyStatusChanged(null, _isSyncing);
        }

        private void NotifyStatusChanged(string? message, bool isSyncing)
        {
            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs(
                _pendingSyncCount,
                isSyncing,
                message));
        }
    }
}
