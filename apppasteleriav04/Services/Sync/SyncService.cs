using apppasteleriav04.Data.Local.Database;
using apppasteleriav04.Models.Local;
using apppasteleriav04.Services.Connectivity;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Models.Domain;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace apppasteleriav04.Services.Sync
{
    public class SyncService : ISyncService
    {
        private readonly IConnectivityService _connectivityService;
        private SQLiteAsyncConnection Database => AppDatabase.Instance.Database;
        private bool _isSyncing = false;
        private int _pendingSyncCount = 0;

        public event EventHandler<SyncProgressEventArgs>? SyncProgress;

        public int PendingSyncCount => _pendingSyncCount;
        public bool IsSyncing => _isSyncing;

        public SyncService(IConnectivityService connectivityService)
        {
            _connectivityService = connectivityService;
            _connectivityService.ConnectivityChanged += OnConnectivityChanged;
            
            // Initial count - using Task.Run to avoid fire-and-forget in constructor
            Task.Run(async () => await UpdatePendingCountAsync());
        }

        private async void OnConnectivityChanged(object? sender, bool isConnected)
        {
            Debug.WriteLine($"[SyncService] Connectivity changed: {isConnected}");
            if (isConnected)
            {
                // Auto-sync when connection is restored
                await SyncPendingAsync();
            }
        }

        private async Task UpdatePendingCountAsync()
        {
            try
            {
                _pendingSyncCount = await Database.Table<SyncQueueItem>().CountAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] UpdatePendingCountAsync error: {ex}");
                _pendingSyncCount = 0;
            }
        }

        public async Task EnqueueAsync(string entityType, Guid entityId, string action, string payloadJson)
        {
            var item = new SyncQueueItem
            {
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                PayloadJson = payloadJson,
                CreatedAt = DateTime.UtcNow,
                Priority = 0,
                RetryCount = 0
            };

            await Database.InsertAsync(item);
            await UpdatePendingCountAsync();
            
            Debug.WriteLine($"[SyncService] Enqueued {action} for {entityType} {entityId}");
        }

        public async Task SyncPendingAsync()
        {
            if (_isSyncing)
            {
                Debug.WriteLine("[SyncService] Already syncing, skipping...");
                return;
            }

            if (!_connectivityService.IsConnected)
            {
                Debug.WriteLine("[SyncService] No connection, skipping sync...");
                return;
            }

            _isSyncing = true;

            try
            {
                var pendingItems = await Database.Table<SyncQueueItem>()
                    .OrderBy(i => i.Priority)
                    .ThenBy(i => i.CreatedAt)
                    .ToListAsync();

                Debug.WriteLine($"[SyncService] Found {pendingItems.Count} items to sync");

                var totalItems = pendingItems.Count;
                var processedItems = 0;
                var failedItems = 0;

                foreach (var item in pendingItems)
                {
                    try
                    {
                        RaiseSyncProgress(totalItems, processedItems, failedItems, item.EntityType, false, null);

                        var success = await ProcessSyncItemAsync(item);
                        
                        if (success)
                        {
                            await Database.DeleteAsync(item);
                            processedItems++;
                            Debug.WriteLine($"[SyncService] Successfully synced {item.EntityType} {item.EntityId}");
                        }
                        else
                        {
                            // Increment retry count
                            item.RetryCount++;
                            item.LastAttemptAt = DateTime.UtcNow;
                            
                            // Remove after 5 retries
                            if (item.RetryCount >= 5)
                            {
                                item.LastError = "Max retries exceeded";
                                await Database.DeleteAsync(item);
                                failedItems++;
                            }
                            else
                            {
                                await Database.UpdateAsync(item);
                                failedItems++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SyncService] Error syncing item: {ex.Message}");
                        item.RetryCount++;
                        item.LastAttemptAt = DateTime.UtcNow;
                        item.LastError = ex.Message;
                        await Database.UpdateAsync(item);
                        failedItems++;
                    }
                }

                RaiseSyncProgress(totalItems, processedItems, failedItems, null, true, null);
                Debug.WriteLine($"[SyncService] Sync complete. Processed: {processedItems}, Failed: {failedItems}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] SyncPendingAsync error: {ex}");
                RaiseSyncProgress(0, 0, 0, null, true, ex.Message);
            }
            finally
            {
                _isSyncing = false;
                await UpdatePendingCountAsync();
            }
        }

        private async Task<bool> ProcessSyncItemAsync(SyncQueueItem item)
        {
            try
            {
                if (item.EntityType == "order" && item.Action == "create")
                {
                    // Deserialize order data
                    var orderData = JsonSerializer.Deserialize<OrderSyncPayload>(item.PayloadJson);
                    if (orderData == null) return false;

                    // Create order items from payload
                    var orderItems = orderData.Items.Select(i => new OrderItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.UnitPrice
                    }).ToList();

                    // Send to Supabase
                    var createdOrder = await SupabaseService.Instance.CreateOrderAsync(
                        orderData.UserId,
                        orderItems
                    );

                    if (createdOrder != null)
                    {
                        // Mark local order as synced
                        var localOrder = await Database.Table<LocalOrder>()
                            .Where(o => o.Id == item.EntityId)
                            .FirstOrDefaultAsync();

                        if (localOrder != null)
                        {
                            localOrder.IsSynced = true;
                            localOrder.SyncedAt = DateTime.UtcNow;
                            await Database.UpdateAsync(localOrder);
                        }

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] ProcessSyncItemAsync error: {ex}");
                return false;
            }
        }

        private void RaiseSyncProgress(int total, int processed, int failed, string? currentEntity, bool isComplete, string? errorMessage)
        {
            SyncProgress?.Invoke(this, new SyncProgressEventArgs
            {
                TotalItems = total,
                ProcessedItems = processed,
                FailedItems = failed,
                CurrentEntity = currentEntity,
                IsComplete = isComplete,
                ErrorMessage = errorMessage
            });
        }
    }

    // Helper class for order sync payload
    internal class OrderSyncPayload
    {
        public Guid UserId { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemPayload> Items { get; set; } = new();
    }

    internal class OrderItemPayload
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}
