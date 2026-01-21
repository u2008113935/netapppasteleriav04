using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Sync;
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

namespace apppasteleriav04.Services.Core
{
    /// <summary>
    /// Modelo de respuesta para autenticacion
    /// </summary>
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
    }

    /// <summary>
    /// Servicio ligero para llamadas REST a Supabase (tables:  productos, pedidos, pedido_items, order_locations, profiles).
    /// Implementa metodos comunes usados por MainPage, OrderPage y LiveTrackingPage.
    /// </summary>
    public class SupabaseService
    {
        public static SupabaseService Instance { get; } = new SupabaseService();

        internal readonly HttpClient _http;
        readonly string _url;
        readonly string _anon;
        readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        private static readonly JsonSerializerOptions _syncJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public SupabaseService()
        {
            _url = SupabaseConfig.SUPABASE_URL?.TrimEnd('/') ?? string.Empty;
            _anon = SupabaseConfig.SUPABASE_ANON_KEY ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_url) || string.IsNullOrWhiteSpace(_anon))
            {
                Debug.WriteLine("[SupabaseService] ADVERTENCIA: URL o ANON_KEY no configurados!");
            }

            _http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            if (!string.IsNullOrWhiteSpace(_anon))
                _http.DefaultRequestHeaders.Add("apikey", _anon);

            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Debug.WriteLine($"[SupabaseService] Inicializado con URL:  {_url}");
        }

        // Permite setear el token del usuario (Authorization:  Bearer ...)
        public void SetUserToken(string? token)
        {
            _http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Debug.WriteLine($"[SupabaseService] Token configurado:  {token.Substring(0, Math.Min(20, token.Length))}...");
            }
            else
            {
                Debug.WriteLine("[SupabaseService] Token eliminado (null)");
            }
        }

        public AuthUser GetCurrentUser()
        {
            return new AuthUser
            {
                Id = AuthService.Instance.UserId,
                Email = AuthService.Instance.UserEmail
            };
        }

        #region Authentication

        /// <summary>
        /// Autenticar usuario con email y contrasena
        /// </summary>
        public async Task<AuthResponse> SignInAsync(string email, string password)
        {
            try
            {
                Debug.WriteLine($"[SupabaseService] SignInAsync: intentando con {email}");

                if (string.IsNullOrWhiteSpace(_url) || string.IsNullOrWhiteSpace(_anon))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Error = "Supabase no esta configurado correctamente"
                    };
                }

                var payload = new
                {
                    email = email,
                    password = password
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var req = new HttpRequestMessage(HttpMethod.Post, $"{_url}/auth/v1/token?grant_type=password")
                {
                    Content = content
                };

                // Asegurar que el apikey esta en la request
                if (!req.Headers.Contains("apikey"))
                    req.Headers.Add("apikey", _anon);

                var resp = await _http.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                Debug.WriteLine($"[SupabaseService] SignInAsync:  status={resp.StatusCode}");
                Debug.WriteLine($"[SupabaseService] SignInAsync:  response={json.Substring(0, Math.Min(300, json.Length))}...");

                if (!resp.IsSuccessStatusCode)
                {
                    // Intentar extraer mensaje de error de Supabase
                    string errorMessage = $"Error de autenticacion: {resp.StatusCode}";
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<JsonElement>(json, _jsonOpts);
                        if (errorObj.TryGetProperty("error_description", out var errDesc))
                            errorMessage = errDesc.GetString() ?? errorMessage;
                        else if (errorObj.TryGetProperty("msg", out var msg))
                            errorMessage = msg.GetString() ?? errorMessage;
                        else if (errorObj.TryGetProperty("message", out var message))
                            errorMessage = message.GetString() ?? errorMessage;
                        else if (errorObj.TryGetProperty("error", out var error))
                            errorMessage = error.GetString() ?? errorMessage;
                    }
                    catch { }

                    Debug.WriteLine($"[SupabaseService] SignInAsync error: {errorMessage}");

                    return new AuthResponse
                    {
                        Success = false,
                        Error = errorMessage
                    };
                }

                var result = JsonSerializer.Deserialize<JsonElement>(json, _jsonOpts);

                // Extraer datos de la respuesta
                string? accessToken = null;
                string? refreshToken = null;
                Guid? userId = null;
                string? userEmail = null;

                // Extraer access_token
                if (result.TryGetProperty("access_token", out var atProp))
                    accessToken = atProp.GetString();

                // Extraer refresh_token
                if (result.TryGetProperty("refresh_token", out var rtProp))
                    refreshToken = rtProp.GetString();

                // El usuario viene dentro de "user"
                if (result.TryGetProperty("user", out var userObj))
                {
                    if (userObj.TryGetProperty("id", out var idProp))
                    {
                        var idStr = idProp.GetString();
                        if (!string.IsNullOrEmpty(idStr) && Guid.TryParse(idStr, out var parsedId))
                            userId = parsedId;
                    }

                    if (userObj.TryGetProperty("email", out var emailProp))
                        userEmail = emailProp.GetString();
                }

                Debug.WriteLine($"[SupabaseService] SignInAsync exitoso:");
                Debug.WriteLine($"[SupabaseService] - UserId: {userId}");
                Debug.WriteLine($"[SupabaseService] - Email: {userEmail}");
                Debug.WriteLine($"[SupabaseService] - AccessToken presente: {!string.IsNullOrEmpty(accessToken)}");
                Debug.WriteLine($"[SupabaseService] - RefreshToken presente: {!string.IsNullOrEmpty(refreshToken)}");

                return new AuthResponse
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserId = userId,
                    Email = userEmail
                };
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"[SupabaseService] SignInAsync HTTP error: {httpEx.Message}");
                return new AuthResponse
                {
                    Success = false,
                    Error = $"Error de conexion:  {httpEx.Message}"
                };
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("[SupabaseService] SignInAsync:  timeout");
                return new AuthResponse
                {
                    Success = false,
                    Error = "Tiempo de espera agotado.  Verifica tu conexion."
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] SignInAsync exception: {ex}");
                return new AuthResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        #endregion

        #region Products

        #region Products

        public async Task<List<Product>> GetProductsAsync(int? limit = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{_url}/rest/v1/productos?select=*";
                if (limit.HasValue && limit.Value > 0)
                    url += $"&limit={limit.Value}";

                Debug.WriteLine($"[SupabaseService] GetProductsAsync: requesting {url}");
                using var resp = await _http.GetAsync(url, cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                Debug.WriteLine($"[SupabaseService] GetProductsAsync: status={resp.StatusCode}; bodyLength={(json?.Length ?? 0)}");

                // Si hay error 401 (token invalido/expirado), reintentar SIN token
                // Los productos son PUBLICOS, no requieren autenticacion
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine("[SupabaseService] GetProductsAsync: Error 401 - Reintentando sin token...");

                    // Crear una nueva peticion SIN el header Authorization
                    using var httpNoAuth = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                    httpNoAuth.DefaultRequestHeaders.Add("apikey", _anon);
                    httpNoAuth.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    using var retryResp = await httpNoAuth.GetAsync(url, cancellationToken);
                    json = await retryResp.Content.ReadAsStringAsync(cancellationToken);

                    Debug.WriteLine($"[SupabaseService] GetProductsAsync (sin auth): status={retryResp.StatusCode}");

                    if (!retryResp.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"[SupabaseService] GetProductsAsync (sin auth) failed: {retryResp.StatusCode} - {json}");
                        return new List<Product>();
                    }
                }
                else if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[SupabaseService] GetProductsAsync failed:  {resp.StatusCode} - {json}");
                    return new List<Product>();
                }

                var products = JsonSerializer.Deserialize<List<Product>>(json, _jsonOpts) ?? new List<Product>();

                // Validar y filtrar productos invalidos
                var validProducts = new List<Product>();
                foreach (var p in products)
                {
                    try
                    {
                        var errs = p.Validate();
                        if (errs == null || errs.Count == 0)
                            validProducts.Add(p);
                        else
                            Debug.WriteLine($"[SupabaseService] Producto invalido (id={p?.Id}): {string.Join("; ", errs)}");
                    }
                    catch (Exception vEx)
                    {
                        Debug.WriteLine($"[SupabaseService] Error validando producto:  {vEx}");
                    }
                }

                // Normalizar imagenes y fallback
                NormalizeProductImages(validProducts);

                Debug.WriteLine($"[SupabaseService] GetProductsAsync: {validProducts.Count} productos validos");
                return validProducts;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[SupabaseService] GetProductsAsync: cancelled");
                return new List<Product>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] GetProductsAsync error:  {ex}");
                return new List<Product>();
            }
        }

        public async Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{_url}/rest/v1/productos? id=eq.{id}&select=*";
                using var resp = await _http.GetAsync(url, cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);

                Debug.WriteLine($"[SupabaseService] GetProductAsync:  status={resp.StatusCode}; bodyLength={(json?.Length ?? 0)}");

                // Si hay error 401, reintentar sin token
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine("[SupabaseService] GetProductAsync:  Error 401 - Reintentando sin token...");

                    using var httpNoAuth = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                    httpNoAuth.DefaultRequestHeaders.Add("apikey", _anon);
                    httpNoAuth.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    using var retryResp = await httpNoAuth.GetAsync(url, cancellationToken);
                    json = await retryResp.Content.ReadAsStringAsync(cancellationToken);

                    if (!retryResp.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"[SupabaseService] GetProductAsync (sin auth) failed: {retryResp.StatusCode} - {json}");
                        return null;
                    }
                }
                else if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[SupabaseService] GetProductAsync failed: {resp.StatusCode} - {json}");
                    return null;
                }

                var list = JsonSerializer.Deserialize<List<Product>>(json, _jsonOpts);
                var product = (list != null && list.Count > 0) ? list[0] : null;

                if (product != null)
                    product.ImagenPath = string.IsNullOrWhiteSpace(ImageHelper.Normalize(product.ImagenPath))
                        ? ImageHelper.DefaultPlaceholder
                        : ImageHelper.Normalize(product.ImagenPath);

                return product;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] GetProductAsync error: {ex}");
                return null;
            }
        }

        void NormalizeProductImages(IEnumerable<Product>? products)
        {
            if (products == null) return;

            foreach (var p in products)
            {
                try
                {
                    var normalized = ImageHelper.Normalize(p.ImagenPath);
                    p.ImagenPath = string.IsNullOrWhiteSpace(normalized)
                        ? ImageHelper.DefaultPlaceholder
                        : normalized;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SupabaseService] NormalizeProductImages error for {p?.Nombre}: {ex}");
                    p.ImagenPath = ImageHelper.DefaultPlaceholder;
                }
            }
        }

        #endregion

        #endregion

        #region Orders & Items

        // Crear pedido + items
        public async Task<Order> CreateOrderAsync(Guid userid, List<OrderItem> items, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"[SupabaseService] CreateOrderAsync: userid={userid}, items={items.Count}");

            var total = items.Sum(it => it.Price * it.Quantity);

            var orderPayload = new
            {
                userid = userid,
                total = total,
                status = "pendiente",
                created_at = DateTime.UtcNow
            };

            var orderContent = new StringContent(JsonSerializer.Serialize(orderPayload), Encoding.UTF8, "application/json");
            using var orderReq = new HttpRequestMessage(HttpMethod.Post, $"{_url}/rest/v1/pedidos") { Content = orderContent };
            orderReq.Headers.Add("Prefer", "return=representation");

            var resp = await _http.SendAsync(orderReq, cancellationToken);
            var createdOrderJson = await resp.Content.ReadAsStringAsync(cancellationToken);

            Debug.WriteLine($"[SupabaseService] CreateOrderAsync (order): status={resp.StatusCode}");

            if (!resp.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[SupabaseService] CreateOrderAsync (order) failed: {resp.StatusCode} - {createdOrderJson}");
                throw new Exception($"Error creando pedido: {createdOrderJson}");
            }

            var created = JsonSerializer.Deserialize<List<Order>>(createdOrderJson, _jsonOpts);
            if (created == null || created.Count == 0)
                throw new Exception("No se pudo crear el pedido.");

            var createdOrder = created[0];
            Debug.WriteLine($"[SupabaseService] Pedido creado con ID: {createdOrder.Id}");

            // Insertar items en batch
            var itemsPayload = items.Select(it => new
            {
                pedido_id = createdOrder.Id,
                producto_id = it.ProductId,
                cantidad = it.Quantity,
                precio = it.Price
            }).ToList();

            var itemsContent = new StringContent(JsonSerializer.Serialize(itemsPayload), Encoding.UTF8, "application/json");
            using var itemsReq = new HttpRequestMessage(HttpMethod.Post, $"{_url}/rest/v1/pedido_items") { Content = itemsContent };
            itemsReq.Headers.Add("Prefer", "return=representation");

            var respItems = await _http.SendAsync(itemsReq, cancellationToken);
            var respItemsBody = await respItems.Content.ReadAsStringAsync(cancellationToken);

            Debug.WriteLine($"[SupabaseService] CreateOrderAsync (items): status={respItems.StatusCode}");

            if (!respItems.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[SupabaseService] CreateOrderAsync (items) failed: {respItems.StatusCode} - {respItemsBody}");
                throw new Exception($"Error creando items del pedido: {respItemsBody}");
            }

            Debug.WriteLine($"[SupabaseService] Pedido completo creado exitosamente");
            return createdOrder;
        }

        // Obtener ordenes con filtros generales
        public async Task<List<Order>> GetOrdersAsync(Guid? userId = null, string? status = null, bool includeItems = false, int? limit = null, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine("==========================================");
            Debug.WriteLine("[SupabaseService] GetOrdersAsync INICIADO");
            Debug.WriteLine("==========================================");

            try
            {
                // Construir URL
                var sb = new StringBuilder();
                sb.Append($"{_url}/rest/v1/pedidos?select=*");

                if (userId.HasValue)
                {
                    sb.Append($"&userid=eq.{userId.Value}");
                    Debug.WriteLine($"[SupabaseService] Filtrando por UserId: {userId.Value}");
                }
                else
                {
                    Debug.WriteLine("[SupabaseService] Sin filtro de userId - buscando TODOS los pedidos");
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    sb.Append($"&status=eq.{Uri.EscapeDataString(status)}");
                    Debug.WriteLine($"[SupabaseService] Filtrando por Status: {status}");
                }

                // Ordenar por fecha descendente
                sb.Append("&order=created_at.desc");

                if (limit.HasValue && limit.Value > 0)
                {
                    sb.Append($"&limit={limit.Value}");
                    Debug.WriteLine($"[SupabaseService] Limitando a:  {limit.Value} resultados");
                }

                var url = sb.ToString();
                Debug.WriteLine($"[SupabaseService] URL COMPLETA: {url}");

                // Mostrar headers
                Debug.WriteLine($"[SupabaseService] HEADERS:");
                Debug.WriteLine($"[SupabaseService]   - apikey: {(_anon?.Substring(0, Math.Min(20, _anon.Length)) ?? "NULL")}...");
                Debug.WriteLine($"[SupabaseService]   - Authorization: {(_http.DefaultRequestHeaders.Authorization != null ? $"Bearer {_http.DefaultRequestHeaders.Authorization.Parameter?.Substring(0, 20)}..." : "NULL")}");

                // Hacer petición
                Debug.WriteLine($"[SupabaseService] Enviando petición GET...");
                using var resp = await _http.GetAsync(url, cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);

                Debug.WriteLine($"[SupabaseService] ==========================================");
                Debug.WriteLine($"[SupabaseService] RESPUESTA HTTP:");
                Debug.WriteLine($"[SupabaseService]   - Status Code: {resp.StatusCode} ({(int)resp.StatusCode})");
                Debug.WriteLine($"[SupabaseService]   - Body Length: {json?.Length ?? 0} caracteres");
                Debug.WriteLine($"[SupabaseService]   - Body Content:  {json}");
                Debug.WriteLine($"[SupabaseService] ==========================================");

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[SupabaseService] ERROR HTTP: {resp.StatusCode}");
                    Debug.WriteLine($"[SupabaseService] Response: {json}");
                    return new List<Order>();
                }

                // Deserializar
                Debug.WriteLine($"[SupabaseService] Deserializando respuesta JSON...");
                var orders = JsonSerializer.Deserialize<List<Order>>(json, _jsonOpts) ?? new List<Order>();
                Debug.WriteLine($"[SupabaseService] Órdenes deserializadas:  {orders.Count}");

                // Mostrar cada orden
                if (orders.Count > 0)
                {
                    Debug.WriteLine($"[SupabaseService] ==========================================");
                    Debug.WriteLine($"[SupabaseService] DETALLE DE ÓRDENES ENCONTRADAS:");
                    for (int i = 0; i < orders.Count; i++)
                    {
                        var order = orders[i];
                        Debug.WriteLine($"[SupabaseService] Orden #{i + 1}:");
                        Debug.WriteLine($"[SupabaseService]   - Id: {order.Id}");
                        Debug.WriteLine($"[SupabaseService]   - UserId: {order.UserId}");
                        Debug.WriteLine($"[SupabaseService]   - Total: S/ {order.Total}");
                        Debug.WriteLine($"[SupabaseService]   - Status: {order.Status}");
                        Debug.WriteLine($"[SupabaseService]   - Created: {order.CreatedAt}");
                        Debug.WriteLine($"[SupabaseService]   ---");
                    }
                    Debug.WriteLine($"[SupabaseService] ==========================================");
                }
                else
                {
                    Debug.WriteLine($"[SupabaseService] No se encontraron órdenes en la respuesta");
                }

                // Cargar items si se solicitó
                if (includeItems && orders.Count > 0)
                {
                    Debug.WriteLine($"[SupabaseService] Cargando items para {orders.Count} órdenes.. .");

                    foreach (var o in orders)
                    {
                        try
                        {
                            Debug.WriteLine($"[SupabaseService] Cargando items para orden {o.Id}...");
                            o.Items = await GetOrderItemsAsync(o.Id, cancellationToken);
                            Debug.WriteLine($"[SupabaseService]   Cargados {o.Items.Count} items");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[SupabaseService] Error cargando items para orden {o.Id}:  {ex.Message}");
                        }
                    }
                }
                else if (includeItems && orders.Count == 0)
                {
                    Debug.WriteLine($"[SupabaseService] No hay órdenes para cargar items");
                }

                Debug.WriteLine($"[SupabaseService] TOTAL FINAL: {orders.Count} órdenes");
                Debug.WriteLine($"[SupabaseService] ==========================================");
                return orders;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[SupabaseService] Operación cancelada");
                return new List<Order>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] ==========================================");
                Debug.WriteLine($"[SupabaseService] EXCEPCIÓN EN GetOrdersAsync:");
                Debug.WriteLine($"[SupabaseService] Mensaje: {ex.Message}");
                Debug.WriteLine($"[SupabaseService] StackTrace: {ex.StackTrace}");
                Debug.WriteLine($"[SupabaseService] ==========================================");
                return new List<Order>();
            }
        }

        // Obtener ordenes de un usuario (nombre claro)
        public async Task<List<Order>> GetOrdersByUserAsync(Guid userId, bool includeItems = false, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine("==========================================");
            Debug.WriteLine($"[SupabaseService] GetOrdersByUserAsync llamado");
            Debug.WriteLine($"[SupabaseService] UserId solicitado: {userId}");
            Debug.WriteLine($"[SupabaseService] IncludeItems: {includeItems}");
            Debug.WriteLine("==========================================");

            var result = await GetOrdersAsync(userId: userId, includeItems: includeItems, cancellationToken: cancellationToken);

            Debug.WriteLine($"[SupabaseService] GetOrdersByUserAsync retornando {result.Count} órdenes");
            Debug.WriteLine("==========================================");

            return result;
        }

        // CORREGIDO:  Obtener UNA orden especifica por ID
        public async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine("==========================================");
            Debug.WriteLine($"[SupabaseService] GetOrderAsync INICIADO");
            Debug.WriteLine($"[SupabaseService] OrderId solicitado: {orderId}");
            Debug.WriteLine("==========================================");

            try
            {
                // URL sin espacios
                var url = $"{_url}/rest/v1/pedidos?idpedido=eq.{orderId}&select=*";
                Debug.WriteLine($"[SupabaseService] URL: {url}");

                var resp = await _http.GetAsync(url, cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);

                Debug.WriteLine($"[SupabaseService] GetOrderAsync:  status={resp.StatusCode}; bodyLength={(json?.Length ?? 0)}");
                Debug.WriteLine($"[SupabaseService] Response body: {json}");

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[SupabaseService] GetOrderAsync failed: {resp.StatusCode} - {json}");
                    return null;
                }

                var list = JsonSerializer.Deserialize<List<Order>>(json, _jsonOpts);
                var order = (list != null && list.Count > 0) ? list[0] : null;

                if (order != null)
                {
                    Debug.WriteLine($"[SupabaseService] Orden encontrada: {order.Id}");

                    try
                    {
                        Debug.WriteLine($"[SupabaseService] Cargando items para la orden.. .");
                        order.Items = await GetOrderItemsAsync(order.Id, cancellationToken);
                        Debug.WriteLine($"[SupabaseService] Items cargados: {order.Items.Count}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SupabaseService] Error cargando items:  {ex.Message}");
                    }
                }
                else
                {
                    Debug.WriteLine($"[SupabaseService] Orden no encontrada");
                }

                Debug.WriteLine("==========================================");
                return order;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] GetOrderAsync error: {ex.Message}");
                Debug.WriteLine($"[SupabaseService] StackTrace: {ex.StackTrace}");
                Debug.WriteLine("==========================================");
                return null;
            }
        }

        // Obtener items de una orden especifica
        public async Task<List<OrderItem>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                // CORREGIDO: URL sin espacios
                var url = $"{_url}/rest/v1/pedido_items?pedido_id=eq.{orderId}&select=*";
                Debug.WriteLine($"[SupabaseService] GetOrderItemsAsync URL: {url}");

                var resp = await _http.GetAsync(url, cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);

                Debug.WriteLine($"[SupabaseService] GetOrderItemsAsync: status={resp.StatusCode}; bodyLength={(json?.Length ?? 0)}");

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[SupabaseService] GetOrderItemsAsync failed: {resp.StatusCode} - {json}");
                    return new List<OrderItem>();
                }

                var items = JsonSerializer.Deserialize<List<OrderItem>>(json, _jsonOpts) ?? new List<OrderItem>();
                Debug.WriteLine($"[SupabaseService] Items deserializados: {items.Count}");

                return items;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] GetOrderItemsAsync error: {ex}");
                return new List<OrderItem>();
            }
        }

        #endregion

        #region Locations (tracking)

        public async Task<List<OrderLocation>> GetOrderLocationsAsync(Guid orderId, int limit = 100, CancellationToken cancellationToken = default)
        {
            try
            {
                var resp = await _http.GetAsync($"{_url}/rest/v1/order_locations?pedido_id=eq.{orderId}&order=registrado_en. desc&limit={limit}", cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                Debug.WriteLine($"[SupabaseService] GetOrderLocationsAsync:  status={resp.StatusCode}; bodyLength={(json?.Length ?? 0)}");

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[SupabaseService] GetOrderLocationsAsync failed: {resp.StatusCode} - {json}");
                    return new List<OrderLocation>();
                }

                var locs = JsonSerializer.Deserialize<List<OrderLocation>>(json, _jsonOpts) ?? new List<OrderLocation>();
                return locs;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] GetOrderLocationsAsync error: {ex}");
                return new List<OrderLocation>();
            }
        }

        // Insertar registro de ubicacion (historico)
        public async Task<bool> InsertOrderLocationAsync(OrderLocation loc, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new[] { new
                {
                    pedido_id = loc.PedidoId,
                    latitud = loc. Latitud,
                    longitud = loc.Longitud,
                    registrado_en = loc. RegistradoEn,
                    dispositivo_id = loc.DispositivoId,
                    velocidad = loc. Velocidad,
                    rumbo = loc. Rumbo
                } };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var req = new HttpRequestMessage(HttpMethod.Post, $"{_url}/rest/v1/order_locations") { Content = content };
                req.Headers.Add("Prefer", "return=representation");

                var resp = await _http.SendAsync(req, cancellationToken);
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[SupabaseService] InsertOrderLocationAsync failed: {resp.StatusCode} - {body}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] InsertOrderLocationAsync error: {ex}");
                return false;
            }
        }

        // Actualizar ubicacion actual en la fila pedidos (PATCH)
        public async Task<bool> UpdateOrderLocationAsync(Guid orderId, double latitude, double longitude, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    latitud_actual = latitude,
                    longitud_actual = longitude,
                    seguimiento_habilitado = enableTracking
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var req = new HttpRequestMessage(HttpMethod.Patch, $"{_url}/rest/v1/pedidos? id=eq.{orderId}") { Content = content };
                req.Headers.Add("Prefer", "return=representation");

                var resp = await _http.SendAsync(req, cancellationToken);
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[SupabaseService] UpdateOrderLocationAsync failed:  {resp.StatusCode} - {body}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] UpdateOrderLocationAsync error: {ex}");
                return false;
            }
        }

        #endregion

        #region Profiles / Auth helpers

        public async Task<UserProfile?> GetProfileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userIdString = id.ToString().Trim();
                Debug.WriteLine($"[SupabaseService] GetProfileAsync:  buscando perfil para id={userIdString}");

                var resp = await _http.GetAsync($"{_url}/rest/v1/profiles?id=eq.{id}&select=*", cancellationToken);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);

                Debug.WriteLine($"[SupabaseService] GetProfileAsync:  status={resp.StatusCode}");

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[SupabaseService] GetProfileAsync failed: {resp.StatusCode} - {json}");
                    return null;
                }

                var list = JsonSerializer.Deserialize<List<UserProfile>>(json, _jsonOpts);
                var profile = (list != null && list.Count > 0) ? list[0] : null;

                if (profile != null)
                    Debug.WriteLine($"[SupabaseService] GetProfileAsync: perfil encontrado - {profile.FullName}");
                else
                    Debug.WriteLine("[SupabaseService] GetProfileAsync:  perfil no encontrado");

                return profile;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] GetProfileAsync exception: {ex}");
                return null;
            }
        }

        #endregion

        #region Offline Support

        public async Task<List<Product>> GetProductsWithOfflineSupportAsync()
        {
            try
            {
                // Check connectivity using MAUI API directly
                var isConnected = Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess == Microsoft.Maui.Networking.NetworkAccess.Internet;
                
                if (isConnected)
                {
                    // Online: Fetch from server and cache locally
                    Debug.WriteLine("[SupabaseService] Online - Fetching products from server");
                    var products = await GetProductsAsync();
                    
                    // Cache in SQLite
                    var productRepo = new apppasteleriav04.Data.Local.Repositories.LocalProductRepository();
                    foreach (var product in products)
                    {
                        var localProduct = new apppasteleriav04.Models.Local.LocalProduct
                        {
                            Id = product.Id,
                            Name = product.Nombre ?? string.Empty,
                            Description = product.Descripcion ?? string.Empty,
                            Price = product.Precio ?? 0m,
                            ImageUrl = product.ImagenPath ?? string.Empty,
                            Category = product.Categoria ?? string.Empty,
                            IsAvailable = true,
                            LastSyncedAt = DateTime.UtcNow,
                            IsSynced = true
                        };
                        await productRepo.SaveAsync(localProduct);
                    }
                    
                    Debug.WriteLine($"[SupabaseService] Cached {products.Count} products locally");
                    return products;
                }
                else
                {
                    // Offline: Return from SQLite cache
                    Debug.WriteLine("[SupabaseService] Offline - Loading products from cache");
                    var productRepo = new apppasteleriav04.Data.Local.Repositories.LocalProductRepository();
                    var localProducts = await productRepo.GetAllAsync();
                    
                    var products = localProducts.Select(lp => new Product
                    {
                        Id = lp.Id,
                        Nombre = lp.Name,
                        Descripcion = lp.Description,
                        Precio = lp.Price,
                        ImagenPath = lp.ImageUrl,
                        Categoria = lp.Category
                    }).ToList();
                    
                    Debug.WriteLine($"[SupabaseService] Loaded {products.Count} products from cache");
                    return products;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] GetProductsWithOfflineSupportAsync error: {ex}");
                
                // Fallback to cache on error
                try
                {
                    var productRepo = new apppasteleriav04.Data.Local.Repositories.LocalProductRepository();
                    var localProducts = await productRepo.GetAllAsync();
                    return localProducts.Select(lp => new Product
                    {
                        Id = lp.Id,
                        Nombre = lp.Name,
                        Descripcion = lp.Description,
                        Precio = lp.Price,
                        ImagenPath = lp.ImageUrl,
                        Categoria = lp.Category
                    }).ToList();
                }
                catch
                {
                    return new List<Product>();
                }
            }
        }

        public async Task<Order?> CreateOrderWithOfflineSupportAsync(Guid userId, List<CartItem> items, ISyncService? syncService = null)
        {
            try
            {
                // Check connectivity using MAUI API directly
                var isConnected = Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess == Microsoft.Maui.Networking.NetworkAccess.Internet;
                
                if (isConnected)
                {
                    // Online: Create directly on server
                    Debug.WriteLine("[SupabaseService] Online - Creating order on server");
                    var orderItems = items.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList();
                    
                    return await CreateOrderAsync(userId, orderItems);
                }
                else
                {
                    // Offline: Save locally and enqueue for sync
                    Debug.WriteLine("[SupabaseService] Offline - Saving order locally");
                    
                    var orderId = Guid.NewGuid();
                    var total = items.Sum(i => i.Subtotal);
                    
                    var localOrder = new apppasteleriav04.Models.Local.LocalOrder
                    {
                        Id = orderId,
                        UserId = userId,
                        Total = total,
                        Status = "pendiente",
                        CreatedAt = DateTime.UtcNow,
                        IsSynced = false
                    };
                    
                    var localOrderItems = items.Select(item => new apppasteleriav04.Models.Local.LocalOrderItem
                    {
                        OrderId = orderId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName ?? string.Empty,
                        UnitPrice = item.Price,
                        Quantity = item.Quantity,
                        Subtotal = item.Subtotal
                    }).ToList();
                    
                    // Save to local database
                    var orderRepo = new apppasteleriav04.Data.Local.Repositories.LocalOrderRepository();
                    await orderRepo.SaveWithItemsAsync(localOrder, localOrderItems);
                    
                    // Enqueue for sync if syncService is provided
                    if (syncService != null)
                    {
                        var orderPayload = new
                        {
                            UserId = userId,
                            Total = total,
                            Items = localOrderItems.Select(i => new
                            {
                                ProductId = i.ProductId,
                                ProductName = i.ProductName,
                                Quantity = i.Quantity,
                                UnitPrice = i.UnitPrice,
                                Subtotal = i.Subtotal
                            }).ToList()
                        };
                        
                        await syncService.EnqueueAsync("order", orderId, "create", JsonSerializer.Serialize(orderPayload, _syncJsonOptions));
                        Debug.WriteLine($"[SupabaseService] Order {orderId} enqueued for sync");
                    }
                    else
                    {
                        Debug.WriteLine($"[SupabaseService] Order {orderId} saved locally (sync service not available)");
                    }
                    
                    // Return a placeholder Order object
                    return new Order
                    {
                        Id = orderId,
                        UserId = userId,
                        Total = total,
                        Status = "pendiente",
                        CreatedAt = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SupabaseService] CreateOrderWithOfflineSupportAsync error: {ex}");
                return null;
            }
        }

        #endregion
    }
}