using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.ViewModels.Profile.Employee
{
    public class DeliveryViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Order> _assignedOrders;
        private Order? _currentDelivery;
        private bool _isLoading;
        private bool _isDelivering;
        private double _currentLatitude;
        private double _currentLongitude;

        public ObservableCollection<Order> AssignedOrders
        {
            get => _assignedOrders;
            set
            {
                if (_assignedOrders == value) return;
                _assignedOrders = value;
                OnPropertyChanged();
            }
        }

        public Order? CurrentDelivery
        {
            get => _currentDelivery;
            set
            {
                if (_currentDelivery == value) return;
                _currentDelivery = value;
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

        public bool IsDelivering
        {
            get => _isDelivering;
            set
            {
                if (_isDelivering == value) return;
                _isDelivering = value;
                OnPropertyChanged();
            }
        }

        public double CurrentLatitude
        {
            get => _currentLatitude;
            set
            {
                if (_currentLatitude == value) return;
                _currentLatitude = value;
                OnPropertyChanged();
            }
        }

        public double CurrentLongitude
        {
            get => _currentLongitude;
            set
            {
                if (_currentLongitude == value) return;
                _currentLongitude = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadOrdersCommand { get; }
        public ICommand StartDeliveryCommand { get; }
        public ICommand MarkDeliveredCommand { get; }
        public ICommand UpdateLocationCommand { get; }

        public DeliveryViewModel()
        {
            _assignedOrders = new ObservableCollection<Order>();

            LoadOrdersCommand = new Command(async () => await LoadAssignedOrdersAsync());
            StartDeliveryCommand = new Command<Order>(async (order) => await StartDeliveryAsync(order));
            MarkDeliveredCommand = new Command(async () => await MarkDeliveredAsync());
            UpdateLocationCommand = new Command(async () => await UpdateLocationAsync());
        }

        private async Task LoadAssignedOrdersAsync()
        {
            IsLoading = true;
            try
            {
                // TODO: Load assigned orders for delivery person from EmployeeService
                await Task.Delay(100);

                AssignedOrders.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeliveryViewModel] Error loading orders: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task StartDeliveryAsync(Order order)
        {
            if (order == null) return;

            try
            {
                CurrentDelivery = order;
                IsDelivering = true;

                // TODO: Update order status to "en_camino" via EmployeeService
                await Task.Delay(100);

                // Start location tracking in background
                Task.Run(async () =>
                {
                    try
                    {
                        await UpdateLocationAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DeliveryViewModel] Error in background location update: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeliveryViewModel] Error starting delivery: {ex.Message}");
            }
        }

        private async Task MarkDeliveredAsync()
        {
            if (CurrentDelivery == null) return;

            try
            {
                // TODO: Update order status to "entregado" via EmployeeService
                await Task.Delay(100);

                IsDelivering = false;
                CurrentDelivery = null;
                await LoadAssignedOrdersAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeliveryViewModel] Error marking delivered: {ex.Message}");
            }
        }

        private async Task UpdateLocationAsync()
        {
            if (CurrentDelivery == null) return;

            try
            {
                // TODO: Get current location and update via EmployeeService
                await Task.Delay(100);

                // Update location in order tracking
                // CurrentLatitude and CurrentLongitude should be updated from GPS
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeliveryViewModel] Error updating location: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
