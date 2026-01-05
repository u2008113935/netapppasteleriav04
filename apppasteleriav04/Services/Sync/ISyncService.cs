using System;
using System.Threading.Tasks;

namespace apppasteleriav04.Services.Sync
{
    public interface ISyncService
    {
        int PendingSyncCount { get; }
        bool IsSyncing { get; }
        event EventHandler<SyncProgressEventArgs> SyncProgress;
        Task SyncPendingAsync();
        Task EnqueueAsync(string entityType, Guid entityId, string action, string payloadJson);
    }
}
