using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace apppasteleriav04.ViewModels.Profile.Admin
{
    /// <summary>
    /// ViewModel para gestión de pedidos del administrador
    /// </summary>
    public class AdminOrdersViewModel : BaseViewModel
    {
        private readonly AdminService _adminService;

        public ObservableCollection<Order> Orders { get; set; } = new ObservableCollection<Order>();

        public Order? SelectedOrder { get; set; }
        public string FilterStatus { get; set; } = "Todos";
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;
        public bool IsLoading { get; set; }

        // Commands
        public ICommand LoadOrdersCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand ExportCommand { get; }

        public AdminOrdersViewModel()
        {
            _adminService = AdminService.Instance;

            LoadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            ViewDetailsCommand = new Command<Order>(OnViewDetails);
            UpdateStatusCommand = new Command<string>(async (status) => await UpdateOrderStatus(status));
            ExportCommand = new Command(async () => await ExportOrdersAsync());
        }

        public async Task LoadOrdersAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                var status = FilterStatus == "Todos" ? null : FilterStatus.ToLower();
                var orders = await _adminService.GetAllOrdersAsync(status, StartDate, EndDate);

                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminOrdersViewModel] Error loading orders: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnViewDetails(Order? order)
        {
            if (order != null)
            {
                SelectedOrder = order;
                // Navigate to order details page
                Shell.Current.GoToAsync($"//admin/orders/details?orderId={order.Id}");
            }
        }

        private async Task UpdateOrderStatus(string? newStatus)
        {
            if (SelectedOrder == null || string.IsNullOrEmpty(newStatus)) return;

            IsLoading = true;
            try
            {
                var success = await _adminService.UpdateOrderStatusAsync(SelectedOrder.Id, newStatus);
                if (success)
                {
                    SelectedOrder.Status = newStatus;
                    await LoadOrdersAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminOrdersViewModel] Error updating order status: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ExportOrdersAsync()
        {
            try
            {
                // Simple CSV export logic
                var csv = "ID,Usuario,Total,Estado,Fecha\n";
                foreach (var order in Orders)
                {
                    csv += $"{order.Id},{order.UserId},{order.Total},{order.Status},{order.CreatedAt:yyyy-MM-dd HH:mm}\n";
                }

                Debug.WriteLine($"[AdminOrdersViewModel] Exported {Orders.Count} orders");
                // In a real app, save to file or share
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminOrdersViewModel] Error exporting orders: {ex.Message}");
            }
        }

    }
}
