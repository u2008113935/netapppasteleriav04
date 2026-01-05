using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace apppasteleriav04.ViewModels.Profile.Admin
{
    /// <summary>
    /// ViewModel para el dashboard de administrador
    /// </summary>
    public class AdminDashboardViewModel : BaseViewModel
    {
        private readonly AdminService _adminService;

        // Properties
        public decimal TodaySales { get; set; }
        public int TodayOrders { get; set; }
        public int PendingOrders { get; set; }

        public ObservableCollection<Order> RecentOrders { get; set; } = new ObservableCollection<Order>();
        public ObservableCollection<TopProduct> TopProducts { get; set; } = new ObservableCollection<TopProduct>();

        public bool IsLoading { get; set; }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand ViewAllOrdersCommand { get; }
        public ICommand ViewAnalyticsCommand { get; }

        public AdminDashboardViewModel()
        {
            _adminService = AdminService.Instance;

            RefreshCommand = new Command(async () => await LoadDashboardAsync());
            ViewAllOrdersCommand = new Command(OnViewAllOrders);
            ViewAnalyticsCommand = new Command(OnViewAnalytics);
        }

        public async Task LoadDashboardAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                var dashboardData = await _adminService.GetDashboardDataAsync();

                TodaySales = dashboardData.TodaySales;
                TodayOrders = dashboardData.TodayOrders;
                PendingOrders = dashboardData.PendingOrders;

                RecentOrders.Clear();
                foreach (var order in dashboardData.RecentOrders)
                {
                    RecentOrders.Add(order);
                }

                TopProducts.Clear();
                foreach (var product in dashboardData.TopProducts)
                {
                    TopProducts.Add(product);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminDashboardViewModel] Error loading dashboard: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnViewAllOrders()
        {
            // Navigate to AdminOrdersPage
            Shell.Current.GoToAsync("//admin/orders");
        }

        private void OnViewAnalytics()
        {
            // Navigate to AnalyticsPage
            Shell.Current.GoToAsync("//admin/analytics");
        }
    }
}
