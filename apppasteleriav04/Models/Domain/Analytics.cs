using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    /// <summary>
    /// Representa datos analíticos y KPIs del negocio
    /// </summary>
    public class Analytics
    {
        [JsonPropertyName("total_sales")]
        public decimal TotalSales { get; set; }

        [JsonPropertyName("total_orders")]
        public int TotalOrders { get; set; }

        [JsonPropertyName("average_order_value")]
        public decimal AverageOrderValue { get; set; }

        [JsonPropertyName("orders_by_status")]
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();

        [JsonPropertyName("top_products")]
        public List<TopProduct> TopProducts { get; set; } = new List<TopProduct>();

        [JsonPropertyName("sales_by_day")]
        public Dictionary<string, decimal> SalesByDay { get; set; } = new Dictionary<string, decimal>();

        [JsonPropertyName("sales_by_month")]
        public Dictionary<string, decimal> SalesByMonth { get; set; } = new Dictionary<string, decimal>();

        [JsonPropertyName("conversion_rate")]
        public decimal ConversionRate { get; set; }

        [JsonPropertyName("cart_abandonment_rate")]
        public decimal CartAbandonmentRate { get; set; }

        [JsonPropertyName("period_start")]
        public DateTime PeriodStart { get; set; }

        [JsonPropertyName("period_end")]
        public DateTime PeriodEnd { get; set; }
    }

    /// <summary>
    /// Representa un producto más vendido en analytics
    /// </summary>
    public class TopProduct
    {
        [JsonPropertyName("product_id")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("product_name")]
        public string? ProductName { get; set; }

        [JsonPropertyName("quantity_sold")]
        public int QuantitySold { get; set; }

        [JsonPropertyName("total_revenue")]
        public decimal TotalRevenue { get; set; }
    }
}
