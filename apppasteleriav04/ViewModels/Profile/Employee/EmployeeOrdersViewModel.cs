using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.ViewModels.Profile.Employee
{
    public class EmployeeOrdersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Order> _pendingOrders;
        private ObservableCollection<Order> _inProgressOrders;
        private ObservableCollection<Order> _completedOrders;
        private Order? _selectedOrder;
        private string _filterStatus;
        private bool _isLoading;
        private bool _isRefreshing;

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

        public string FilterStatus
        {
            get => _filterStatus;
            set
            {
                if (_filterStatus == value) return;
                _filterStatus = value;
                OnPropertyChanged();
                
                // Trigger load asynchronously but handle exceptions
                Task.Run(async () =>
                {
                    try
                    {
                        await LoadOrdersAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[EmployeeOrdersViewModel] Error in FilterStatus load: {ex.Message}");
                    }
                });
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

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing == value) return;
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadOrdersCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        public EmployeeOrdersViewModel()
        {
            _pendingOrders = new ObservableCollection<Order>();
            _inProgressOrders = new ObservableCollection<Order>();
            _completedOrders = new ObservableCollection<Order>();
            _filterStatus = "all";

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
                // TODO: Load orders from EmployeeService based on FilterStatus
                await Task.Delay(100);

                // Mock data for now
                PendingOrders.Clear();
                InProgressOrders.Clear();
                CompletedOrders.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EmployeeOrdersViewModel] Error loading orders: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"[EmployeeOrdersViewModel] Error updating order status: {ex.Message}");
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
            IsRefreshing = true;
            try
            {
                await LoadOrdersAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
