using apppasteleriav04.ViewModels.Profile.Admin;

namespace apppasteleriav04.Views.Profile.Admin
{
    public partial class AdminDashboardPage : ContentPage
    {
        private readonly AdminDashboardViewModel _viewModel;

        public AdminDashboardPage()
        {
            InitializeComponent();
            _viewModel = new AdminDashboardViewModel();
            BindingContext = _viewModel;
            
            // Subscribe to property changes to update UI
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            await _viewModel.LoadDashboardAsync();

            TodaySalesLabel.Text = $"${_viewModel.TodaySales:F2}";
            TodayOrdersLabel.Text = _viewModel.TodayOrders.ToString();
            PendingOrdersLabel.Text = _viewModel.PendingOrders.ToString();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.TodaySales))
            {
                TodaySalesLabel.Text = $"${_viewModel.TodaySales:F2}";
            }
            else if (e.PropertyName == nameof(_viewModel.TodayOrders))
            {
                TodayOrdersLabel.Text = _viewModel.TodayOrders.ToString();
            }
            else if (e.PropertyName == nameof(_viewModel.PendingOrders))
            {
                PendingOrdersLabel.Text = _viewModel.PendingOrders.ToString();
            }
        }

        private async void OnProductsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//admin/products");
        }

        private async void OnOrdersClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//admin/orders");
        }

        private async void OnAnalyticsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//admin/analytics");
        }

        private async void OnPromotionsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//admin/promotions");
        }
    }
}
