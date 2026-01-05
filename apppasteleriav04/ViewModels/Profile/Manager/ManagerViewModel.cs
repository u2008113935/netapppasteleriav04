using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Profile.Manager
{
    public class ManagerViewModel : BaseViewModel
    {
        private decimal _todaySales;
        public decimal TodaySales
        {
            get => _todaySales;
            set => SetProperty(ref _todaySales, value);
        }

        private int _todayOrders;
        public int TodayOrders
        {
            get => _todayOrders;
            set => SetProperty(ref _todayOrders, value);
        }

        private int _pendingOrders;
        public int PendingOrders
        {
            get => _pendingOrders;
            set => SetProperty(ref _pendingOrders, value);
        }

        private bool _hasUrgentOrders;
        public bool HasUrgentOrders
        {
            get => _hasUrgentOrders;
            set => SetProperty(ref _hasUrgentOrders, value);
        }

        private string _urgentOrdersText = string.Empty;
        public string UrgentOrdersText
        {
            get => _urgentOrdersText;
            set => SetProperty(ref _urgentOrdersText, value);
        }

        private ObservableCollection<Order> _recentOrders = new();
        public ObservableCollection<Order> RecentOrders
        {
            get => _recentOrders;
            set => SetProperty(ref _recentOrders, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ExportDailyReportCommand { get; }
        public ICommand ExportWeeklyReportCommand { get; }
        public ICommand ExportMonthlyReportCommand { get; }

        public event EventHandler? NavigateToOrders;
        public event EventHandler? NavigateToAnalytics;
        public event EventHandler? NavigateToReports;
        public event EventHandler<string>? ReportGenerated;

        public ManagerViewModel()
        {
            Title = "Dashboard Gerente";
            RefreshCommand = new AsyncRelayCommand(LoadDashboardAsync, () => !IsBusy);
            ExportDailyReportCommand = new AsyncRelayCommand(ExportDailyReportAsync, () => !IsBusy);
            ExportWeeklyReportCommand = new AsyncRelayCommand(ExportWeeklyReportAsync, () => !IsBusy);
            ExportMonthlyReportCommand = new AsyncRelayCommand(ExportMonthlyReportAsync, () => !IsBusy);
        }

        public async Task LoadDashboardAsync()
        {
            ErrorMessage = string.Empty;
            IsBusy = true;

            try
            {
                // Simulate loading dashboard data
                await Task.Delay(1000);

                // In a real app, this would load from a service
                TodaySales = 2450.50m;
                TodayOrders = 18;
                PendingOrders = 5;

                // Check for urgent orders (older than 30 minutes)
                HasUrgentOrders = PendingOrders > 3;
                UrgentOrdersText = HasUrgentOrders ? $"Tienes {PendingOrders} pedidos pendientes" : string.Empty;

                // Load recent orders
                RecentOrders.Clear();
                for (int i = 0; i < 5; i++)
                {
                    RecentOrders.Add(new Order
                    {
                        Id = Guid.NewGuid(),
                        Total = 50.00m + (i * 25.50m),
                        Status = i % 2 == 0 ? "Pendiente" : "En preparación",
                        CreatedAt = DateTime.Now.AddMinutes(-i * 15)
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar el dashboard: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExportDailyReportAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Delay(1000);
                ReportGenerated?.Invoke(this, "Reporte Diario");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al generar reporte: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExportWeeklyReportAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Delay(1000);
                ReportGenerated?.Invoke(this, "Reporte Semanal");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al generar reporte: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExportMonthlyReportAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Delay(1000);
                ReportGenerated?.Invoke(this, "Reporte Mensual");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al generar reporte: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
