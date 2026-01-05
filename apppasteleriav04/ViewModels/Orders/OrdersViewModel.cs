using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Orders
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase = SupabaseService.Instance;
        private ObservableCollection<Order> _orders = new();
        private Order? _selectedOrder;
        private bool _isLoading;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Gets the collection of orders
        /// </summary>
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        /// <summary>
        /// Gets or sets the selected order
        /// </summary>
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether orders are being loaded
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Command to load orders
        /// </summary>
        public ICommand LoadOrdersCommand { get; }

        /// <summary>
        /// Command to view order details
        /// </summary>
        public ICommand ViewOrderDetailsCommand { get; }

        public OrdersViewModel()
        {
            Title = "Mis Pedidos";
            LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
            ViewOrderDetailsCommand = new RelayCommand<Order>(ViewOrderDetails);
        }

        /// <summary>
        /// Load orders for the current user
        /// </summary>
        public async Task LoadOrdersAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Cancel previous operation if any
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                // Get userId from AuthService
                string? userIdStr = AuthService.Instance?.UserId;
                
                if (string.IsNullOrWhiteSpace(userIdStr))
                {
                    // Try to read from SecureStorage
                    try
                    {
                        userIdStr = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("auth_user_id");
                    }
                    catch { /* ignore if not exists */ }
                }

                if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userGuid))
                {
                    ErrorMessage = "No se encontró usuario autenticado.";
                    Orders.Clear();
                    return;
                }

                Debug.WriteLine($"[OrdersViewModel] Loading orders for user: {userGuid}");

                // Load orders from service
                var orders = await _supabase.GetOrdersByUserAsync(userGuid, includeItems: true, cancellationToken: _cts.Token);

                // Update collection on UI thread
                await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Orders.Clear();
                    if (orders != null)
                    {
                        foreach (var order in orders.OrderByDescending(o => o.CreatedAt))
                        {
                            Orders.Add(order);
                        }
                    }
                });

                Debug.WriteLine($"[OrdersViewModel] Loaded {Orders.Count} orders");
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[OrdersViewModel] LoadOrdersAsync cancelled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrdersViewModel] Error loading orders: {ex}");
                ErrorMessage = "No se pudieron cargar los pedidos. Revisa la conexión.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// View details of an order
        /// </summary>
        private void ViewOrderDetails(Order? order)
        {
            if (order != null)
            {
                SelectedOrder = order;
                Debug.WriteLine($"[OrdersViewModel] Viewing order details: {order.Id}");
                // Navigation will be handled by the view
            }
        }

        /// <summary>
        /// Cancel any ongoing operations
        /// </summary>
        public void CancelOperations()
        {
            _cts?.Cancel();
            _cts = null;
        }
    }
}
