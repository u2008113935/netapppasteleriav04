using SQLite;
using System;

namespace apppasteleriav04.Models.Local
{
    [Table("order_items")]
    public class LocalOrderItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}
