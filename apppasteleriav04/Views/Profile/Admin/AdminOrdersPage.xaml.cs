using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Profile.Admin;

namespace apppasteleriav04.Views.Profile.Admin
{
    public partial class AdminOrdersPage : ContentPage
    {
        private readonly AdminOrdersViewModel _viewModel;

        public AdminOrdersPage()
        {
            InitializeComponent();
            _viewModel = new AdminOrdersViewModel();
            BindingContext = _viewModel;
            
            StatusPicker.SelectedIndex = 0; // "Todos"
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            await _viewModel.LoadOrdersAsync();
            
            OrderCountLabel.Text = $"Mostrando {_viewModel.Orders.Count} pedidos";

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private async void OnStatusFilterChanged(object sender, EventArgs e)
        {
            if (StatusPicker.SelectedItem is string status)
            {
                _viewModel.FilterStatus = status;
                await LoadOrders();
            }
        }

        private void OnViewDetailsClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Order order)
            {
                _viewModel.ViewDetailsCommand.Execute(order);
            }
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            await _viewModel.ExportOrdersAsync();
            await DisplayAlert("Exportar", "Pedidos exportados exitosamente", "OK");
        }
    }
}
