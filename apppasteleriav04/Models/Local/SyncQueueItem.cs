using SQLite;
using System;

namespace apppasteleriav04.Models.Local
{
    [Table("sync_queue")]
    public class SyncQueueItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty; // "order", "product"
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty; // "create", "update", "delete"
        public string PayloadJson { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
        public int RetryCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public string? LastError { get; set; }
    }
}
