using apppasteleriav04.Models.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace apppasteleriav04.Services.Core
{
    /// <summary>
    /// Servicio para operaciones de administrador
    /// </summary>
    public class AdminService
    {
        private readonly SupabaseService _supabaseService;
        private static AdminService? _instance;
        
        public static AdminService Instance => _instance ??= new AdminService();

        private AdminService()
        {
            _supabaseService = SupabaseService.Instance;
        }

        #region Dashboard

        /// <summary>
        /// Obtiene datos del dashboard para administradores
        /// </summary>
        public async Task<DashboardData> GetDashboardDataAsync()
        {
            try
            {
                var today = DateTime.Today;
                var todayOrders = await GetOrdersByDateAsync(today, today.AddDays(1));
                
                var todaySales = todayOrders.Sum(o => o.Total);
                var todayOrderCount = todayOrders.Count;
                var pendingOrders = todayOrders.Count(o => o.Status == "pending" || o.Status == "processing");
                
                return new DashboardData
                {
                    TodaySales = todaySales,
                    TodayOrders = todayOrderCount,
                    PendingOrders = pendingOrders,
                    RecentOrders = todayOrders.OrderByDescending(o => o.CreatedAt).Take(10).ToList()
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error getting dashboard data: {ex.Message}");
                return new DashboardData();
            }
        }

        #endregion

        #region Products

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        public async Task<Product?> CreateProductAsync(Product product)
        {
            try
            {
                if (product.Id == Guid.Empty)
                    product.Id = Guid.NewGuid();

                var json = JsonSerializer.Serialize(product);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _supabaseService._http.PostAsync(
                    $"{GetBaseUrl()}/rest/v1/productos",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<Product>>();
                    return result?.FirstOrDefault();
                }

                Debug.WriteLine($"[AdminService] Error creating product: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error creating product: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                var json = JsonSerializer.Serialize(product);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _supabaseService._http.PatchAsync(
                    $"{GetBaseUrl()}/rest/v1/productos?idproducto=eq.{product.Id}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error updating product: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Elimina un producto
        /// </summary>
        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            try
            {
                var response = await _supabaseService._http.DeleteAsync(
                    $"{GetBaseUrl()}/rest/v1/productos?idproducto=eq.{productId}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error deleting product: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Orders

        /// <summary>
        /// Obtiene todos los pedidos con filtros opcionales
        /// </summary>
        public async Task<List<Order>> GetAllOrdersAsync(string? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var url = $"{GetBaseUrl()}/rest/v1/pedidos?select=*&order=created_at.desc";

                if (!string.IsNullOrEmpty(status))
                    url += $"&status=eq.{status}";

                if (startDate.HasValue)
                    url += $"&created_at=gte.{startDate.Value:yyyy-MM-dd}";

                if (endDate.HasValue)
                    url += $"&created_at=lte.{endDate.Value:yyyy-MM-dd}";

                var response = await _supabaseService._http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
                    return orders ?? new List<Order>();
                }

                Debug.WriteLine($"[AdminService] Error getting orders: {response.StatusCode}");
                return new List<Order>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error getting orders: {ex.Message}");
                return new List<Order>();
            }
        }

        /// <summary>
        /// Actualiza el estado de un pedido
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            try
            {
                var update = new { status = newStatus };
                var json = JsonSerializer.Serialize(update);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _supabaseService._http.PatchAsync(
                    $"{GetBaseUrl()}/rest/v1/pedidos?idpedido=eq.{orderId}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error updating order status: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Users

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        public async Task<List<UserProfile>> GetAllUsersAsync(string? role = null)
        {
            try
            {
                var url = $"{GetBaseUrl()}/rest/v1/profiles?select=*";

                if (!string.IsNullOrEmpty(role))
                    url += $"&role=eq.{role}";

                var response = await _supabaseService._http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<UserProfile>>();
                    return users ?? new List<UserProfile>();
                }

                Debug.WriteLine($"[AdminService] Error getting users: {response.StatusCode}");
                return new List<UserProfile>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error getting users: {ex.Message}");
                return new List<UserProfile>();
            }
        }

        /// <summary>
        /// Actualiza el rol de un usuario
        /// </summary>
        public async Task<bool> UpdateUserRoleAsync(Guid userId, string newRole)
        {
            try
            {
                var update = new { role = newRole };
                var json = JsonSerializer.Serialize(update);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _supabaseService._http.PatchAsync(
                    $"{GetBaseUrl()}/rest/v1/profiles?id=eq.{userId}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error updating user role: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Obtiene datos analíticos para un rango de fechas
        /// </summary>
        public async Task<Analytics> GetAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var orders = await GetOrdersByDateAsync(startDate, endDate);
                
                var analytics = new Analytics
                {
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TotalOrders = orders.Count,
                    TotalSales = orders.Sum(o => o.Total),
                    AverageOrderValue = orders.Count > 0 ? orders.Average(o => o.Total) : 0,
                    OrdersByStatus = orders.GroupBy(o => o.Status ?? "unknown")
                                           .ToDictionary(g => g.Key, g => g.Count()),
                    SalesByDay = orders.GroupBy(o => o.CreatedAt.Date.ToString("yyyy-MM-dd"))
                                       .ToDictionary(g => g.Key, g => g.Sum(o => o.Total)),
                    ConversionRate = 0, // Would need additional data
                    CartAbandonmentRate = 0 // Would need additional data
                };

                return analytics;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error getting analytics: {ex.Message}");
                return new Analytics();
            }
        }

        #endregion

        #region Promotions

        /// <summary>
        /// Crea una nueva promoción
        /// </summary>
        public async Task<Promotion?> CreatePromotionAsync(Promotion promotion)
        {
            try
            {
                if (promotion.Id == Guid.Empty)
                    promotion.Id = Guid.NewGuid();

                var json = JsonSerializer.Serialize(promotion);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _supabaseService._http.PostAsync(
                    $"{GetBaseUrl()}/rest/v1/promotions",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<Promotion>>();
                    return result?.FirstOrDefault();
                }

                Debug.WriteLine($"[AdminService] Error creating promotion: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error creating promotion: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Actualiza una promoción existente
        /// </summary>
        public async Task<bool> UpdatePromotionAsync(Promotion promotion)
        {
            try
            {
                var json = JsonSerializer.Serialize(promotion);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _supabaseService._http.PatchAsync(
                    $"{GetBaseUrl()}/rest/v1/promotions?id=eq.{promotion.Id}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error updating promotion: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene todas las promociones
        /// </summary>
        public async Task<List<Promotion>> GetPromotionsAsync(bool? activeOnly = null)
        {
            try
            {
                var url = $"{GetBaseUrl()}/rest/v1/promotions?select=*";

                if (activeOnly.HasValue && activeOnly.Value)
                    url += "&is_active=eq.true";

                var response = await _supabaseService._http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var promotions = await response.Content.ReadFromJsonAsync<List<Promotion>>();
                    return promotions ?? new List<Promotion>();
                }

                Debug.WriteLine($"[AdminService] Error getting promotions: {response.StatusCode}");
                return new List<Promotion>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminService] Error getting promotions: {ex.Message}");
                return new List<Promotion>();
            }
        }

        #endregion

        #region Helper Methods

        private string GetBaseUrl()
        {
            return SupabaseConfig.SUPABASE_URL?.TrimEnd('/') ?? string.Empty;
        }

        private async Task<List<Order>> GetOrdersByDateAsync(DateTime startDate, DateTime endDate)
        {
            return await GetAllOrdersAsync(null, startDate, endDate);
        }

        #endregion
    }

    /// <summary>
    /// Datos del dashboard de administrador
    /// </summary>
    public class DashboardData
    {
        public decimal TodaySales { get; set; }
        public int TodayOrders { get; set; }
        public int PendingOrders { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<TopProduct> TopProducts { get; set; } = new List<TopProduct>();
    }
}
