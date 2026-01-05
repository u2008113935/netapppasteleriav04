using apppasteleriav04.ViewModels.Profile.Manager;

namespace apppasteleriav04.Views.Profile.Manager;

public partial class ManagerPage : ContentPage
{
    private readonly ManagerViewModel _viewModel;

    public ManagerPage()
    {
        InitializeComponent();
        _viewModel = new ManagerViewModel();
        BindingContext = _viewModel;

        // Subscribe to navigation events
        _viewModel.NavigateToOrders += OnNavigateToOrders;
        _viewModel.NavigateToAnalytics += OnNavigateToAnalytics;
        _viewModel.NavigateToReports += OnNavigateToReports;
        _viewModel.ReportGenerated += OnReportGenerated;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDashboardAsync();
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update labels if using x:Name instead of bindings
        if (FindByName("TodaySalesLabel") is Label todaySales)
            todaySales.Text = $"S/. {_viewModel.TodaySales:F2}";
        
        if (FindByName("TodayOrdersLabel") is Label todayOrders)
            todayOrders.Text = _viewModel.TodayOrders.ToString();
        
        if (FindByName("PendingOrdersLabel") is Label pendingOrders)
            pendingOrders.Text = _viewModel.PendingOrders.ToString();
    }

    private async void OnNavigateToOrders(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//admin/orders");
    }

    private async void OnNavigateToAnalytics(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//admin/analytics");
    }

    private async void OnNavigateToReports(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//manager/reports");
    }

    private async void OnReportGenerated(object? sender, string report)
    {
        await DisplayAlert("Reporte Generado", "El reporte se ha generado exitosamente", "OK");
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await _viewModel.LoadDashboardAsync();
        UpdateUI();
    }

    private async void OnViewAllOrdersClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//admin/orders");
    }

    private async void OnViewAnalyticsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//admin/analytics");
    }

    private async void OnExportDailyClicked(object sender, EventArgs e)
    {
        _viewModel.ExportDailyReportCommand.Execute(null);
    }

    private async void OnExportWeeklyClicked(object sender, EventArgs e)
    {
        _viewModel.ExportWeeklyReportCommand.Execute(null);
    }

    private async void OnExportMonthlyClicked(object sender, EventArgs e)
    {
        _viewModel.ExportMonthlyReportCommand.Execute(null);
    }
}