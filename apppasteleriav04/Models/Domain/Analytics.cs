using System;
using System.Collections.Generic;

namespace apppasteleriav04.Models.Domain
{
    public class Analytics
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        
        public decimal TotalSales { get; set; }
        
        public int TotalOrders { get; set; }
        
        public decimal AverageOrderValue { get; set; }
        
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
        
        public List<TopProduct> TopProducts { get; set; } = new List<TopProduct>();
        
        public Dictionary<string, decimal> SalesByDay { get; set; } = new Dictionary<string, decimal>();
        
        public Dictionary<string, decimal> SalesByMonth { get; set; } = new Dictionary<string, decimal>();
        
        public decimal ConversionRate { get; set; }
        
        public decimal CartAbandonmentRate { get; set; }
    }

    public class TopProduct
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}
