using SQLite;
using System;

namespace apppasteleriav04.Models.Local
{
    [Table("products")]
    public class LocalProduct
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public DateTime LastSyncedAt { get; set; }
        public bool IsSynced { get; set; } = true;
    }
}
