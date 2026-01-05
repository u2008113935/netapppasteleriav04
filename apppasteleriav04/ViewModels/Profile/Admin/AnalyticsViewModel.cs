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

        public Analytics? Analytics { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public bool IsLoading { get; set; }

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
                UpdateKPIs();
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

    }
}
