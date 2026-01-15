using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace apppasteleriav04.Models.Local
{
    public class LocalCartItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public Guid? UserId { get; set; }

        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
        public bool IsSynced { get; set; } = false;
        public decimal SubTotal => Price * Quantity;

    }
}
