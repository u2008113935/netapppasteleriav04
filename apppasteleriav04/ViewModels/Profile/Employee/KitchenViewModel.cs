using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.ViewModels.Profile.Employee
{
    public class KitchenViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Order> _ordersToPrepare;
        private Order? _currentOrder;
        private bool _isLoading;
        private bool _isPreparing;

        public ObservableCollection<Order> OrdersToPrepare
        {
            get => _ordersToPrepare;
            set
            {
                if (_ordersToPrepare == value) return;
                _ordersToPrepare = value;
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

        public bool IsPreparing
        {
            get => _isPreparing;
            set
            {
                if (_isPreparing == value) return;
                _isPreparing = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadOrdersCommand { get; }
        public ICommand StartPreparingCommand { get; }
        public ICommand MarkReadyCommand { get; }

        public KitchenViewModel()
        {
            _ordersToPrepare = new ObservableCollection<Order>();

            LoadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            StartPreparingCommand = new Command<Order>(async (order) => await StartPreparingAsync(order));
            MarkReadyCommand = new Command(async () => await MarkOrderReadyAsync());
        }

        private async Task LoadOrdersAsync()
        {
            IsLoading = true;
            try
            {
                // TODO: Load pending orders for kitchen from EmployeeService
                await Task.Delay(100);

                OrdersToPrepare.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KitchenViewModel] Error loading orders: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task StartPreparingAsync(Order order)
        {
            if (order == null) return;

            try
            {
                CurrentOrder = order;
                IsPreparing = true;

                // TODO: Update order status to "en_preparacion" via EmployeeService
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KitchenViewModel] Error starting preparation: {ex.Message}");
            }
        }

        private async Task MarkOrderReadyAsync()
        {
            if (CurrentOrder == null) return;

            try
            {
                // TODO: Update order status to "listo" via EmployeeService
                await Task.Delay(100);

                IsPreparing = false;
                CurrentOrder = null;
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KitchenViewModel] Error marking order ready: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
