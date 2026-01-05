using apppasteleriav04.ViewModels.Profile.Admin;
using System.Collections.Generic;

namespace apppasteleriav04.Views.Profile.Admin
{
    public partial class AnalyticsPage : ContentPage
    {
        private readonly AnalyticsViewModel _viewModel;

        public AnalyticsPage()
        {
            InitializeComponent();
            _viewModel = new AnalyticsViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAnalytics();
        }

        private async Task LoadAnalytics()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            await _viewModel.LoadAnalyticsAsync();

            UpdateKPILabels();
            UpdateOrdersByStatus();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private void UpdateKPILabels()
        {
            TotalRevenueLabel.Text = $"${_viewModel.TotalRevenue:F2}";
            TotalOrdersLabel.Text = _viewModel.TotalOrders.ToString();
            AvgOrderValueLabel.Text = $"${_viewModel.AverageOrderValue:F2}";
        }

        private void UpdateOrdersByStatus()
        {
            if (_viewModel.Analytics?.OrdersByStatus != null)
            {
                var statusList = new List<KeyValuePair<string, int>>(_viewModel.Analytics.OrdersByStatus);
                OrdersByStatusCollection.ItemsSource = statusList;
            }
        }

        private async void OnDateRangeClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string range)
            {
                _viewModel.ChangeDateRangeCommand.Execute(range);
                await LoadAnalytics();
            }
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            _viewModel.ExportReportCommand.Execute(null);
            await DisplayAlert("Exportar", "Reporte exportado exitosamente", "OK");
        }
    }
}
