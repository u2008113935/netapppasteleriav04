using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.ViewModels.Profile.Employee
{
    public class EmployeeBackOfficeViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Order> _allOrders;
        private ObservableCollection<Order> _pendingOrders;
        private ObservableCollection<Order> _inProgressOrders;
        private ObservableCollection<Order> _completedOrders;
        private Order? _selectedOrder;
        private bool _isLoading;
        private string _filterStatus = "all";

        public ObservableCollection<Order> AllOrders
        {
            get => _allOrders;
            set
            {
                if (_allOrders == value) return;
                _allOrders = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Order> PendingOrders
        {
            get => _pendingOrders;
            set
            {
                if (_pendingOrders == value) return;
                _pendingOrders = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Order> InProgressOrders
        {
            get => _inProgressOrders;
            set
            {
                if (_inProgressOrders == value) return;
                _inProgressOrders = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Order> CompletedOrders
        {
            get => _completedOrders;
            set
            {
                if (_completedOrders == value) return;
                _completedOrders = value;
                OnPropertyChanged();
            }
        }

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder == value) return;
                _selectedOrder = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string FilterStatus
        {
            get => _filterStatus;
            set
            {
                if (_filterStatus == value) return;
                _filterStatus = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadOrdersCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        public EmployeeBackOfficeViewModel()
        {
            _allOrders = new ObservableCollection<Order>();
            _pendingOrders = new ObservableCollection<Order>();
            _inProgressOrders = new ObservableCollection<Order>();
            _completedOrders = new ObservableCollection<Order>();

            LoadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            UpdateStatusCommand = new Command<string>(async (status) => await UpdateOrderStatusAsync(status));
            ViewDetailsCommand = new Command<Order>(async (order) => await ViewOrderDetailsAsync(order));
            RefreshCommand = new Command(async () => await RefreshOrdersAsync());
        }

        public async Task LoadOrdersAsync()
        {
            IsLoading = true;
            try
            {
                // TODO: Load all orders from EmployeeService
                // Separate by status:  pendiente, en_preparacion, listo, en_camino, entregado
                await Task.Delay(100);

                AllOrders.Clear();
                PendingOrders.Clear();
                InProgressOrders.Clear();
                CompletedOrders.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BackOfficeViewModel] Error loading orders:  {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task UpdateOrderStatusAsync(string newStatus)
        {
            if (SelectedOrder == null) return;

            try
            {
                // TODO: Update order status via EmployeeService
                await Task.Delay(100);

                SelectedOrder.Status = newStatus;
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BackOfficeViewModel] Error updating order status: {ex.Message}");
            }
        }

        private async Task ViewOrderDetailsAsync(Order order)
        {
            if (order == null) return;

            SelectedOrder = order;
            // TODO: Navigate to OrderDetailsPage
            await Task.CompletedTask;
        }

        private async Task RefreshOrdersAsync()
        {
            await LoadOrdersAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}