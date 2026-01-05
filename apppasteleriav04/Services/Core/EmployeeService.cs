using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.Services.Core
{
    /// <summary>
    /// Service for employee-related operations (backoffice, kitchen, delivery)
    /// </summary>
    public class EmployeeService
    {
        private static readonly Lazy<EmployeeService> _instance = new Lazy<EmployeeService>(() => new EmployeeService());
        public static EmployeeService Instance => _instance.Value;

        private readonly HttpClient _http;
        private readonly string _url;
        private readonly string _anon;
        private readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private EmployeeService()
        {
            _url = SupabaseConfig.SUPABASE_URL?.TrimEnd('/') ?? string.Empty;
            _anon = SupabaseConfig.SUPABASE_ANON_KEY ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_url) || string.IsNullOrWhiteSpace(_anon))
            {
                Debug.WriteLine("[EmployeeService] WARNING: URL or ANON_KEY not configured!");
            }

            _http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            if (!string.IsNullOrWhiteSpace(_anon))
                _http.DefaultRequestHeaders.Add("apikey", _anon);

            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Debug.WriteLine($"[EmployeeService] Initialized with URL: {_url}");
        }

        public void SetUserToken(string? token)
        {
            _http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Debug.WriteLine($"[EmployeeService] Token configured");
            }
            else
            {
                Debug.WriteLine("[EmployeeService] Token removed (null)");
            }
        }

        /// <summary>
        /// Get employee profile by user ID
        /// </summary>
        public async Task<Employee?> GetEmployeeProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine($"[EmployeeService] GetEmployeeProfileAsync: userId={userId}");

                var resp = await _http.GetAsync($"{_url}/rest/v1/employees?userid=eq.{userId}&select=*", cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);

                Debug.WriteLine($"[EmployeeService] GetEmployeeProfileAsync: status={resp.StatusCode}");

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[EmployeeService] GetEmployeeProfileAsync failed: {resp.StatusCode} - {json}");
                    return null;
                }

                var list = JsonSerializer.Deserialize<List<Employee>>(json, _jsonOpts);
                var employee = (list != null && list.Count > 0) ? list[0] : null;

                if (employee != null)
                    Debug.WriteLine($"[EmployeeService] Employee found - {employee.FullName}, Role: {employee.Role}");
                else
                    Debug.WriteLine("[EmployeeService] Employee not found");

                return employee;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EmployeeService] GetEmployeeProfileAsync error: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Get orders filtered by status
        /// </summary>
        public async Task<List<Order>> GetOrdersByStatusAsync(string? status = null, int? limit = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append($"{_url}/rest/v1/pedidos?select=*");

                if (!string.IsNullOrWhiteSpace(status))
                    sb.Append($"&status=eq.{Uri.EscapeDataString(status)}");

                sb.Append("&order=created_at.desc");

                if (limit.HasValue && limit.Value > 0)
                    sb.Append($"&limit={limit.Value}");

                var url = sb.ToString();
                Debug.WriteLine($"[EmployeeService] GetOrdersByStatusAsync: requesting {url}");

                using var resp = await _http.GetAsync(url, cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                Debug.WriteLine($"[EmployeeService] GetOrdersByStatusAsync: status={resp.StatusCode}");

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[EmployeeService] GetOrdersByStatusAsync failed: {resp.StatusCode} - {json}");
                    return new List<Order>();
                }

                var orders = JsonSerializer.Deserialize<List<Order>>(json, _jsonOpts) ?? new List<Order>();

                Debug.WriteLine($"[EmployeeService] GetOrdersByStatusAsync: {orders.Count} orders found");
                return orders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EmployeeService] GetOrdersByStatusAsync error: {ex}");
                return new List<Order>();
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine($"[EmployeeService] UpdateOrderStatusAsync: orderId={orderId}, newStatus={newStatus}");

                var payload = new
                {
                    status = newStatus
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var req = new HttpRequestMessage(HttpMethod.Patch, $"{_url}/rest/v1/pedidos?id=eq.{orderId}") { Content = content };
                req.Headers.Add("Prefer", "return=representation");

                var resp = await _http.SendAsync(req, cancellationToken);
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[EmployeeService] UpdateOrderStatusAsync failed: {resp.StatusCode} - {body}");
                    return false;
                }

                Debug.WriteLine("[EmployeeService] Order status updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EmployeeService] UpdateOrderStatusAsync error: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Assign delivery person to an order
        /// </summary>
        public async Task<bool> AssignDeliveryPersonAsync(Guid orderId, Guid deliveryPersonId, CancellationToken cancellationToken = default)
        {
            try
            {
                Debug.WriteLine($"[EmployeeService] AssignDeliveryPersonAsync: orderId={orderId}, deliveryPersonId={deliveryPersonId}");

                var payload = new
                {
                    repartidor_asignado = deliveryPersonId
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var req = new HttpRequestMessage(HttpMethod.Patch, $"{_url}/rest/v1/pedidos?id=eq.{orderId}") { Content = content };
                req.Headers.Add("Prefer", "return=representation");

                var resp = await _http.SendAsync(req, cancellationToken);
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[EmployeeService] AssignDeliveryPersonAsync failed: {resp.StatusCode} - {body}");
                    return false;
                }

                Debug.WriteLine("[EmployeeService] Delivery person assigned successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EmployeeService] AssignDeliveryPersonAsync error: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Get orders assigned to a specific delivery person
        /// </summary>
        public async Task<List<Order>> GetOrdersByDeliveryPersonAsync(Guid deliveryPersonId, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{_url}/rest/v1/pedidos?repartidor_asignado=eq.{deliveryPersonId}&status=neq.entregado&order=created_at.desc";
                Debug.WriteLine($"[EmployeeService] GetOrdersByDeliveryPersonAsync: requesting {url}");

                using var resp = await _http.GetAsync(url, cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                Debug.WriteLine($"[EmployeeService] GetOrdersByDeliveryPersonAsync: status={resp.StatusCode}");

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[EmployeeService] GetOrdersByDeliveryPersonAsync failed: {resp.StatusCode} - {json}");
                    return new List<Order>();
                }

                var orders = JsonSerializer.Deserialize<List<Order>>(json, _jsonOpts) ?? new List<Order>();

                Debug.WriteLine($"[EmployeeService] GetOrdersByDeliveryPersonAsync: {orders.Count} orders found");
                return orders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EmployeeService] GetOrdersByDeliveryPersonAsync error: {ex}");
                return new List<Order>();
            }
        }
    }
}
