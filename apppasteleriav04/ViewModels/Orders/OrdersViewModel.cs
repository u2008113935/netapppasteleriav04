using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;
using apppasteleriav04.Data.Local.Repositories;
using apppasteleriav04.Services.Connectivity;
using apppasteleriav04.Models.Local;
using System.Linq;

namespace apppasteleriav04.ViewModels.Orders
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly LocalOrderRepository _orderRepository;
        private readonly IConnectivityService _connectivityService;

        private ObservableCollection<Order> _orders = new();
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        private Order? _selectedOrder;
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Indica si está en modo offline (sin conexión a internet)
        // Útil para mostrar banner "Sin conexión" en la UI        
        private bool _isOfflineMode;
        public bool IsOfflineMode
        {
            get => _isOfflineMode;
            set => SetProperty(ref _isOfflineMode, value);
        }

        
        // Cantidad de pedidos pendientes de sincronización
        // Útil para mostrar badge:  "3 pendientes"        
        private int _pendingSyncCount;
        public int PendingSyncCount
        {
            get => _pendingSyncCount;
            set => SetProperty(ref _pendingSyncCount, value);
        }

        public ICommand LoadOrdersCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        public event EventHandler<Order>? OrderSelected;

        public OrdersViewModel()
        {
            // Inicializar repositorio local
            _orderRepository = new LocalOrderRepository();

            // Obtener servicio de conectividad desde DI
            _connectivityService = MauiProgram.Services.GetService<IConnectivityService>()
                ?? throw new InvalidOperationException("IConnectivityService not registered");

            // Suscribirse a cambios en el estado de conectividad
            Title = "Mis Pedidos";
            LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
            ViewOrderDetailsCommand = new RelayCommand<Order>(ViewOrderDetails);
            RefreshCommand = new AsyncRelayCommand(LoadOrdersAsync);
        }

        public async Task LoadOrdersAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine("--------------------------------------------------");
                System.Diagnostics.Debug.WriteLine("[OrdersViewModel] LoadOrdersAsync iniciado");
                System.Diagnostics.Debug.WriteLine("--------------------------------------------------");


                // PASO 1: VALIDAR AUTENTICACIÓN | Check authentication
                if (!AuthService.Instance.IsAuthenticated)
                {
                    System.Diagnostics.Debug.WriteLine("[OrdersViewModel] Usuario no autenticado");
                    ErrorMessage = "Debe iniciar sesión para ver sus pedidos";
                    return;
                }

                var userId = Guid.Parse(AuthService.Instance.UserId ?? Guid.Empty.ToString());
                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] UserId: {userId}");

                // PASO 2: CARGAR DESDE SQLite (SIEMPRE PRIMERO)

                System.Diagnostics.Debug.WriteLine("[OrdersViewModel] PASO 2: Cargando desde SQLite.. .");

                var localOrders = await _orderRepository.GetAllAsync();
                var userLocalOrders = localOrders.Where(o => o.UserId == userId).ToList();

                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] {userLocalOrders.Count} pedidos locales encontrados");

                // Contar pedidos pendientes de sincronización
                PendingSyncCount = userLocalOrders.Count(o => !o.IsSynced);
                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] {PendingSyncCount} pedidos pendientes de sincronización");

                
                // PASO 3: INTENTAR SINCRONIZAR CON BACKEND
                
                var combinedOrders = new System.Collections.Generic.List<Order>();

                if (_connectivityService.IsConnected)
                {
                    IsOfflineMode = false;
                    System.Diagnostics.Debug.WriteLine("[OrdersViewModel] Modo ONLINE - sincronizando con backend");

                    try
                    {
                        // Obtener pedidos del backend (sincronizados)
                        var remoteOrders = await SupabaseService.Instance.GetOrdersByUserAsync(
                            userId,
                            includeItems: true
                        );

                        System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] {remoteOrders.Count} pedidos del backend");

                        // Agregar pedidos remotos (ya sincronizados)
                        combinedOrders.AddRange(remoteOrders);

                        // Agregar pedidos locales NO sincronizados
                        foreach (var localOrder in userLocalOrders.Where(o => !o.IsSynced))
                        {
                            combinedOrders.Add(new Order
                            {
                                Id = localOrder.Id,
                                UserId = localOrder.UserId,
                                Total = localOrder.Total,
                                Status = localOrder.Status + " (pendiente)",  // Indicador visual
                                CreatedAt = localOrder.CreatedAt,
                                Items = new System.Collections.Generic.List<OrderItem>()
                            });
                        }

                        System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] Total combinado: {combinedOrders.Count} pedidos");
                    }
                    catch (Exception syncEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] Error sincronizando: {syncEx.Message}");
                        IsOfflineMode = true;

                        // Si falla la sincronización, mostrar solo datos locales
                        combinedOrders = ConvertLocalOrdersToOrders(userLocalOrders);
                    }
                }
                else
                {
                    IsOfflineMode = true;
                    System.Diagnostics.Debug.WriteLine("[OrdersViewModel] Modo OFFLINE - solo datos locales");

                    // Sin internet, mostrar solo pedidos locales
                    combinedOrders = ConvertLocalOrdersToOrders(userLocalOrders);
                }

                
                // PASO 4: ACTUALIZAR UI
                
                Orders.Clear();

                // Ordenar por fecha descendente (más recientes primero)
                foreach (var order in combinedOrders.OrderByDescending(o => o.CreatedAt))
                {
                    Orders.Add(order);
                    System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] Pedido agregado:");
                    System.Diagnostics.Debug.WriteLine($" - ID: {order.Id}");
                    System.Diagnostics.Debug.WriteLine($" - Total: S/{order.Total}");
                    System.Diagnostics.Debug.WriteLine($" - Status: {order.Status}");
                    System.Diagnostics.Debug.WriteLine($" - CreatedAt:  {order.CreatedAt}");
                }

                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] {Orders.Count} pedidos mostrados en UI");

                if (Orders.Count == 0)
                {
                    ErrorMessage = "No tienes pedidos aún";
                }
                                
                System.Diagnostics.Debug.WriteLine("[OrdersViewModel] LoadOrdersAsync COMPLETADO");
                
            }
            catch (Exception ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] ERROR CRÍTICO: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] StackTrace: {ex.StackTrace}");
                
                ErrorMessage = $"Error al cargar pedidos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
            }
        }

        /*
        //====================================================================================================
            System.Diagnostics.Debug.WriteLine("[OrdersViewModel] Usuario autenticado");
            //Cargar órdenes desde el servicio
            IsLoading = true;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                //Obtener UserId
                var userId = AuthService.Instance.UserId;
                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] UserId RAW: {userId ?? "NULL"}");

                if (string.IsNullOrEmpty(userId))
                {
                    System.Diagnostics.Debug.WriteLine("[OrdersViewModel] UserId no válido o NULL o vacío");
                    ErrorMessage = "Usuario no válido";
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] UserId válido: {userId}");
                System.Diagnostics.Debug.WriteLine("[OrdersViewModel] UserId como GUID: {Guid.Parse(userId)}");


                //Llamar al servicio para obtener órdenes
                System.Diagnostics.Debug.WriteLine("[OrdersViewModel] Llamando a SupabaseService para obtener órdenes");

                //Llamada al servicio y espera de resultados
                var orders = await SupabaseService.Instance.GetOrdersByUserAsync(Guid.Parse(userId), includeItems: true);
                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] Órdenes obtenidas: {orders.Count}");

                //Actualizar colección de órdenes
                Orders.Clear();

                if (orders.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[OrdersViewModel] No se encontraron pedidos para el usuario");
                    ErrorMessage = $"No se encontraron pedidos para el usuario {userId.Substring(0, 8)}...";
                    //return;
                }
                else
                {
                    foreach (var order in orders)
                    {
                        //Agregar orden 
                        Orders.Add(order);
                        System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] Pedido agregado:");
                        System.Diagnostics.Debug.WriteLine($" - ID: {order.Id}");
                        System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] - UserId: {order.UserId}");                        
                        System.Diagnostics.Debug.WriteLine($" - Total: S/{order.Total}");
                        System.Diagnostics.Debug.WriteLine($" - Status: {order.Status}");
                        System.Diagnostics.Debug.WriteLine($" - CreatedAt: {order.CreatedAt}");
                        System.Diagnostics.Debug.WriteLine($" - Items: {order.Items?.Count ?? 0}");
                        //System.Diagnostics.Debug.WriteLine($" - Pedido: {order.Id.ToString().Substring(0, 8)}, " +
                        //  $"Total: S/{order.Total}, Status: {order.Status}");
                    }
                }
                //Final log
                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] Total pedidos: {Orders.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("--------------------------------------------------");
                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] EXCEPCION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[OrdersViewModel] STACKTRACE: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine("--------------------------------------------------");
                ErrorMessage = $"Error al cargar pedidos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
                System.Diagnostics.Debug.WriteLine("[OrdersViewModel] LoadOrdersAsync finalizado");
                System.Diagnostics.Debug.WriteLine("--------------------------------------------------");
            }
        }

        //====================================================================================================

        */

                
        // Convierte pedidos locales a formato de dominio para la UI
        // Agrega indicadores visuales de estado offline        
        private System.Collections.Generic.List<Order> ConvertLocalOrdersToOrders(System.Collections.Generic.List<LocalOrder> localOrders)
        {
            return localOrders.Select(lo => new Order
            {
                Id = lo.Id,
                UserId = lo.UserId,
                Total = lo.Total,
                Status = lo.IsSynced
                    ? lo.Status
                    : lo.Status + " (offline)",  // Indicador visual para pedidos no sincronizados
                CreatedAt = lo.CreatedAt,
                Items = new System.Collections.Generic.List<OrderItem>()
            }).ToList();
        }

        
        private void ViewOrderDetails(Order? order)
        {
            if (order != null)
            {
                SelectedOrder = order;
                OrderSelected?.Invoke(this, order);
            }
        }

        public void CancelOperations()
        {
            // Cancel any pending operations
            IsBusy = false;
            IsLoading = false;
        }
    }
}
