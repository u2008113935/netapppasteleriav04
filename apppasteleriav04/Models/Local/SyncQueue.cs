using System;
using System.ComponentModel.DataAnnotations;

namespace apppasteleriav04.Models.Local
{
    public class SyncQueue
    {
        [Key]
        public int Id { get; set; }

        public string EntityType { get; set; } = string.Empty; // "Order", "Transaction", "Payment"

        public int LocalEntityId { get; set; }

        public string Operation { get; set; } = "INSERT"; // INSERT, UPDATE, DELETE

        public string? JsonData { get; set; }

        public bool IsSynced { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SyncedAt { get; set; }

        public int RetryCount { get; set; } = 0;

        public string? ErrorMessage { get; set; }

        public int Priority { get; set; } = 0; // 0=normal, 1=high, 2=critical
    }
}