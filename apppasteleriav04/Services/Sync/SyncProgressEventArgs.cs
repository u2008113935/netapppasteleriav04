using System;

namespace apppasteleriav04.Services.Sync
{
    public class SyncProgressEventArgs : EventArgs
    {
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public int FailedItems { get; set; }
        public string? CurrentEntity { get; set; }
        public bool IsComplete { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
