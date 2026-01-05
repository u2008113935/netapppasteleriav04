using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace apppasteleriav04.ViewModels.Profile.Admin
{
    /// <summary>
    /// ViewModel para visualización de analíticas y KPIs
    /// </summary>
    public class AnalyticsViewModel : BaseViewModel
    {
        private readonly AdminService _adminService;

        private Analytics? _analytics;
        public Analytics? Analytics
        {
            get => _analytics;
            set
            {
                _analytics = value;
                OnPropertyChanged();
                UpdateKPIs();
            }
        }

        private DateTime _startDate = DateTime.Today.AddDays(-30);
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
            }
        }

        private int _totalOrders;
        public int TotalOrders
        {
            get => _totalOrders;
            set
            {
                _totalOrders = value;
                OnPropertyChanged();
            }
        }

        private decimal _averageOrderValue;
        public decimal AverageOrderValue
        {
            get => _averageOrderValue;
            set
            {
                _averageOrderValue = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        // Chart data
        public List<KeyValuePair<string, decimal>> ChartData { get; set; } = new List<KeyValuePair<string, decimal>>();

        // Commands
        public ICommand LoadAnalyticsCommand { get; }
        public ICommand ExportReportCommand { get; }
        public ICommand ChangeDateRangeCommand { get; }

        public AnalyticsViewModel()
        {
            _adminService = AdminService.Instance;

            LoadAnalyticsCommand = new Command(async () => await LoadAnalyticsAsync());
            ExportReportCommand = new Command(OnExportReport);
            ChangeDateRangeCommand = new Command<string>(OnChangeDateRange);
        }

        public async Task LoadAnalyticsAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                Analytics = await _adminService.GetAnalyticsAsync(StartDate, EndDate);
                CalculateKPIs();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AnalyticsViewModel] Error loading analytics: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateKPIs()
        {
            if (Analytics == null) return;

            TotalRevenue = Analytics.TotalSales;
            TotalOrders = Analytics.TotalOrders;
            AverageOrderValue = Analytics.AverageOrderValue;
        }

        public void CalculateKPIs()
        {
            if (Analytics == null) return;

            // Prepare chart data
            ChartData.Clear();
            foreach (var item in Analytics.SalesByDay)
            {
                ChartData.Add(new KeyValuePair<string, decimal>(item.Key, item.Value));
            }

            OnPropertyChanged(nameof(ChartData));
        }

        private void OnChangeDateRange(string? range)
        {
            switch (range)
            {
                case "7days":
                    StartDate = DateTime.Today.AddDays(-7);
                    EndDate = DateTime.Today;
                    break;
                case "30days":
                    StartDate = DateTime.Today.AddDays(-30);
                    EndDate = DateTime.Today;
                    break;
                case "90days":
                    StartDate = DateTime.Today.AddDays(-90);
                    EndDate = DateTime.Today;
                    break;
                case "year":
                    StartDate = DateTime.Today.AddYears(-1);
                    EndDate = DateTime.Today;
                    break;
            }

            _ = LoadAnalyticsAsync();
        }

        private void OnExportReport()
        {
            if (Analytics == null) return;

            try
            {
                var report = $"Analytics Report\n";
                report += $"Period: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}\n";
                report += $"Total Revenue: ${TotalRevenue:F2}\n";
                report += $"Total Orders: {TotalOrders}\n";
                report += $"Average Order Value: ${AverageOrderValue:F2}\n";

                Debug.WriteLine($"[AnalyticsViewModel] Exported report");
                // In a real app, save to file or share
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AnalyticsViewModel] Error exporting report: {ex.Message}");
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            // Implement INotifyPropertyChanged if BaseViewModel doesn't have it
        }
    }
}
