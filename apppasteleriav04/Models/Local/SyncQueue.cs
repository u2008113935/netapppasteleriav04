using SQLite;
using System;

namespace apppasteleriav04.Models.Local
{
    /// <summary>
    /// Queue for tracking entities that need to be synced with the server
    /// </summary>
    [Table("sync_queue")]
    public class SyncQueue
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("entity_type")]
        public string EntityType { get; set; } = string.Empty; // "Order", "Transaction", "Payment"

        [Column("local_entity_id")]
        public int LocalEntityId { get; set; }

        [Column("operation")]
        public string Operation { get; set; } = "INSERT"; // INSERT, UPDATE, DELETE

        [Column("json_data")]
        public string? JsonData { get; set; }

        [Column("is_synced")]
        public bool IsSynced { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("synced_at")]
        public DateTime? SyncedAt { get; set; }

        [Column("retry_count")]
        public int RetryCount { get; set; } = 0;

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("priority")]
        public int Priority { get; set; } = 0; // 0=normal, 1=high, 2=critical
    }
}
