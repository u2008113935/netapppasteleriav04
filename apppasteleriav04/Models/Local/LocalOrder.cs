using SQLite;
using System;

namespace apppasteleriav04.Models.Local
{
    [Table("orders")]
    public class LocalOrder
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = "pendiente";
        public string DeliveryAddress { get; set; } = string.Empty;
        public bool IsDelivery { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSynced { get; set; } = false;
        public DateTime? SyncedAt { get; set; }
        public string? SyncError { get; set; }
    }
}
