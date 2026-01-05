using SQLite;
using System;

namespace apppasteleriav04.Models.Local
{
    /// <summary>
    /// Local SQLite entity for order items
    /// </summary>
    [Table("order_items")]
    public class LocalOrderItem
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("local_order_id")]
        [Indexed]
        public int LocalOrderId { get; set; }

        [Column("remote_order_id")]
        public string? RemoteOrderId { get; set; }

        [Column("product_id")]
        public string ProductId { get; set; } = string.Empty;

        [Column("product_name")]
        public string ProductName { get; set; } = string.Empty;

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("synced")]
        public bool Synced { get; set; } = false;
    }
}
