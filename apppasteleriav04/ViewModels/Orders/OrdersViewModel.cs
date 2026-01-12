using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Orders
{
    public class OrdersViewModel : BaseViewModel
    {
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

        public ICommand LoadOrdersCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        public event EventHandler<Order>? OrderSelected;

        public OrdersViewModel()
        {
            Title = "Mis Pedidos";
            LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
            ViewOrderDetailsCommand = new RelayCommand<Order>(ViewOrderDetails);
            RefreshCommand = new AsyncRelayCommand(LoadOrdersAsync);
        }

        public async Task LoadOrdersAsync()
        {
            
            System.Diagnostics.Debug.WriteLine("--------------------------------------------------");
            System.Diagnostics.Debug.WriteLine("[OrdersViewModel] LoadOrdersAsync iniciado");
            System.Diagnostics.Debug.WriteLine("--------------------------------------------------");

            //Check authentication
            if (!AuthService.Instance.IsAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[OrdersViewModel] Usuario no autenticado");
                ErrorMessage = "Debe iniciar sesión para ver sus pedidos";
                return;
            }

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
