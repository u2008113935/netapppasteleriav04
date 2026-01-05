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
            if (!AuthService.Instance.IsAuthenticated)
            {
                ErrorMessage = "Debe iniciar sesión para ver sus pedidos";
                return;
            }

            IsLoading = true;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var userId = AuthService.Instance.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    ErrorMessage = "Usuario no válido";
                    return;
                }

                var orders = await SupabaseService.Instance.GetUserOrdersAsync(Guid.Parse(userId));
                
                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar pedidos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
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
    }
}
