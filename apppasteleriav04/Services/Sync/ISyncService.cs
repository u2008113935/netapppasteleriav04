using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apppasteleriav04.Services.Sync
{
    /// <summary>
    /// Interface for synchronization service that handles offline/online data sync
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Event fired when sync status changes
        /// </summary>
        event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;

        /// <summary>
        /// Gets the number of pending items waiting to be synced
        /// </summary>
        int PendingSyncCount { get; }

        /// <summary>
        /// Indicates if a sync operation is currently in progress
        /// </summary>
        bool IsSyncing { get; }

        /// <summary>
        /// Syncs all pending items in the sync queue
        /// </summary>
        Task<SyncResult> SyncPendingAsync();

        /// <summary>
        /// Syncs products from server to local cache
        /// </summary>
        Task<bool> SyncProductsAsync();

        /// <summary>
        /// Adds an order to the sync queue
        /// </summary>
        Task<bool> EnqueueOrderAsync(int localOrderId);

        /// <summary>
        /// Gets the count of pending sync items
        /// </summary>
        Task<int> GetPendingCountAsync();

        /// <summary>
        /// Starts automatic sync monitoring (syncs when connectivity is restored)
        /// </summary>
        void StartAutoSync();

        /// <summary>
        /// Stops automatic sync monitoring
        /// </summary>
        void StopAutoSync();
    }

    /// <summary>
    /// Event args for sync status changes
    /// </summary>
    public class SyncStatusChangedEventArgs : EventArgs
    {
        public int PendingCount { get; set; }
        public bool IsSyncing { get; set; }
        public string? Message { get; set; }

        public SyncStatusChangedEventArgs(int pendingCount, bool isSyncing, string? message = null)
        {
            PendingCount = pendingCount;
            IsSyncing = isSyncing;
            Message = message;
        }
    }

    /// <summary>
    /// Result of a sync operation
    /// </summary>
    public class SyncResult
    {
        public bool Success { get; set; }
        public int ItemsSynced { get; set; }
        public int ItemsFailed { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
