using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace apppasteleriav04.ViewModels.Profile.Employee
{
    public class EmployeeDeliveryViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Order> _ordersToDeliver;
        private Order? _currentOrder;
        private bool _isLoading;
        private bool _isDelivering;
        private string _filterStatus = "all";

        public ObservableCollection<Order> OrdersToDeliver
        {
            get => _ordersToDeliver;
            set
            {
                if (_ordersToDeliver == value) return;
                _ordersToDeliver = value;
                OnPropertyChanged();
            }
        }

        public Order? CurrentOrder
        {
            get => _currentOrder;
            set
            {
                if (_currentOrder == value) return;
                _currentOrder = value;
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
        public ICommand StartDeliveryCommand { get; }
        public ICommand MarkDeliveredCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public EmployeeDeliveryViewModel()
        {
            _ordersToDeliver = new ObservableCollection<Order>();

            LoadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            StartDeliveryCommand = new Command<Order>(async (order) => await StartDeliveryAsync(order));
            MarkDeliveredCommand = new Command(async () => await MarkOrderDeliveredAsync());
            ViewDetailsCommand = new Command<Order>(async (order) => await ViewOrderDetailsAsync(order));
        }

        public async Task LoadOrdersAsync()
        {
            IsLoading = true;
            try
            {

                //await Task.Delay(100);
                //OrdersToDeliver.Clear();

                // Llamar a EmployeeService para cargar pedidos listos
                var orders = await EmployeeService.Instance.GetOrdersByStatusAsync("listo");
                // O también pueden estar "en_camino"
                var inDelivery = await EmployeeService.Instance.GetOrdersByStatusAsync("en_camino");

                OrdersToDeliver.Clear();
                foreach (var order in orders.Concat(inDelivery))
                {
                    OrdersToDeliver.Add(order);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeliveryViewModel] Error:  {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task StartDeliveryAsync(Order order)
        {
            if (order == null) return;

            try
            {
                CurrentOrder = order;
                IsDelivering = true;

                // TODO: Update order status to "en_camino" via EmployeeService
                //await Task.Delay(100);

                // Actualizar estado a "en_camino"
                bool success = await EmployeeService.Instance.UpdateOrderStatusAsync(order.Id, "en_camino");

                if (!success)
                    System.Diagnostics.Debug.WriteLine($"[DeliveryViewModel] Error updating status");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeliveryViewModel] Error: {ex.Message}");
            }
        }

        public async Task MarkOrderDeliveredAsync()
        {
            if (CurrentOrder == null) return;

            try
            {
                // TODO: Update order status to "entregado" via EmployeeService
                //await Task.Delay(100);
                //IsDelivering = false;
                //CurrentOrder = null;
                //await LoadOrdersAsync();

                // Actualizar estado a "entregado"
                bool success = await EmployeeService.Instance.UpdateOrderStatusAsync(CurrentOrder.Id, "entregado");

                if (success)
                {
                    IsDelivering = false;
                    CurrentOrder = null;
                    await LoadOrdersAsync();
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeliveryViewModel] Error marking order delivered: {ex.Message}");
            }
        }

        private async Task ViewOrderDetailsAsync(Order order)
        {
            if (order == null) return;

            CurrentOrder = order;
            // TODO: Navigate to OrderDetailsPage
            await Task.CompletedTask;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}