using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Services.Sync;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Sync
{
    /// <summary>
    /// Example ViewModel demonstrating offline sync functionality
    /// Shows pending sync count and provides manual sync trigger
    /// </summary>
    public class OfflineSyncViewModel : BaseViewModel
    {
        private readonly ISyncService _syncService;
        private readonly SupabaseService _supabaseService;
        private int _pendingSyncCount;
        private string _syncStatus = "Ready";
        private DateTime? _lastSyncTime;

        public int PendingSyncCount
        {
            get => _pendingSyncCount;
            set => SetProperty(ref _pendingSyncCount, value);
        }

        public string SyncStatus
        {
            get => _syncStatus;
            set => SetProperty(ref _syncStatus, value);
        }

        public DateTime? LastSyncTime
        {
            get => _lastSyncTime;
            set => SetProperty(ref _lastSyncTime, value);
        }

        public string LastSyncDisplay => LastSyncTime.HasValue 
            ? $"Last sync: {LastSyncTime.Value.ToLocalTime():g}" 
            : "Never synced";

        public ICommand SyncNowCommand { get; }
        public ICommand RefreshCountCommand { get; }

        public OfflineSyncViewModel(ISyncService? syncService = null, SupabaseService? supabaseService = null)
        {
            _syncService = syncService ?? new SyncService();
            _supabaseService = supabaseService ?? SupabaseService.Instance;

            // Subscribe to sync status changes
            _syncService.SyncStatusChanged += OnSyncStatusChanged;

            // Create commands
            SyncNowCommand = new AsyncRelayCommand(SyncNowAsync, () => !IsBusy);
            RefreshCountCommand = new AsyncRelayCommand(RefreshPendingCountAsync, () => !IsBusy);

            // Initial load
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await RefreshPendingCountAsync();
        }

        private void OnSyncStatusChanged(object? sender, SyncStatusChangedEventArgs e)
        {
            PendingSyncCount = e.PendingCount;
            IsBusy = e.IsSyncing;

            if (e.IsSyncing)
            {
                SyncStatus = "Syncing...";
            }
            else
            {
                SyncStatus = e.PendingCount > 0 
                    ? $"{e.PendingCount} items pending" 
                    : "All synced";
            }

            if (e.Message != null)
            {
                Debug.WriteLine($"[OfflineSyncViewModel] {e.Message}");
            }

            // Refresh command can execute states
            (SyncNowCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (RefreshCountCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task SyncNowAsync()
        {
            try
            {
                IsBusy = true;
                SyncStatus = "Starting sync...";
                ErrorMessage = string.Empty;

                Debug.WriteLine("[OfflineSyncViewModel] Manual sync triggered");

                var result = await _syncService.SyncPendingAsync();

                if (result.Success)
                {
                    SyncStatus = $"Synced {result.ItemsSynced} items successfully";
                    LastSyncTime = DateTime.UtcNow;
                    Debug.WriteLine($"[OfflineSyncViewModel] Sync completed: {result.ItemsSynced} items");
                }
                else
                {
                    SyncStatus = $"Sync failed: {result.ItemsFailed} errors";
                    ErrorMessage = string.Join("\n", result.Errors);
                    Debug.WriteLine($"[OfflineSyncViewModel] Sync failed: {string.Join(", ", result.Errors)}");
                }

                await RefreshPendingCountAsync();
            }
            catch (Exception ex)
            {
                SyncStatus = "Sync error";
                ErrorMessage = ex.Message;
                Debug.WriteLine($"[OfflineSyncViewModel] Sync error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshPendingCountAsync()
        {
            try
            {
                PendingSyncCount = await _syncService.GetPendingCountAsync();
                SyncStatus = PendingSyncCount > 0 
                    ? $"{PendingSyncCount} items pending" 
                    : "All synced";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OfflineSyncViewModel] Error refreshing count: {ex}");
            }
        }

        /// <summary>
        /// Example method showing how to use offline-aware product loading
        /// </summary>
        public async Task<List<Product>> LoadProductsWithOfflineSupportAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var products = await _supabaseService.GetProductsWithOfflineSupportAsync();

                Debug.WriteLine($"[OfflineSyncViewModel] Loaded {products.Count} products (offline-aware)");
                return products;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading products: {ex.Message}";
                Debug.WriteLine($"[OfflineSyncViewModel] Error loading products: {ex}");
                return new List<Product>();
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Example method showing how to create an order with offline support
        /// </summary>
        public async Task<Order?> CreateOrderWithOfflineSupportAsync(Guid userId, List<OrderItem> items)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var order = await _supabaseService.CreateOrderWithOfflineSupportAsync(userId, items);

                if (order != null)
                {
                    Debug.WriteLine($"[OfflineSyncViewModel] Order created: {order.Id}");
                    await RefreshPendingCountAsync();
                }
                else
                {
                    ErrorMessage = "Failed to create order";
                }

                return order;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating order: {ex.Message}";
                Debug.WriteLine($"[OfflineSyncViewModel] Error creating order: {ex}");
                return null;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
