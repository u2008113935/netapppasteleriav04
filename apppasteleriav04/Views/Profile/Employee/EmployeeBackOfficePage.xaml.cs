using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Profile.Employee;

namespace apppasteleriav04.Views.Profile.Employee;

public partial class EmployeeBackOfficePage : ContentPage
{
    private readonly EmployeeBackOfficeViewModel _viewModel;

    public EmployeeBackOfficePage()
    {
        InitializeComponent();
        _viewModel = new EmployeeBackOfficeViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadOrdersAsync();
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            // Reset all buttons to default style
            BtnAllOrders.BackgroundColor = Colors.LightGray;
            BtnAllOrders.TextColor = Colors.Black;
            BtnPendingOrders.BackgroundColor = Colors.LightGray;
            BtnPendingOrders.TextColor = Colors.Black;
            BtnInProgressOrders.BackgroundColor = Colors.LightGray;
            BtnInProgressOrders.TextColor = Colors.Black;
            BtnCompletedOrders.BackgroundColor = Colors.LightGray;
            BtnCompletedOrders.TextColor = Colors.Black;

            // Highlight selected button
            button.BackgroundColor = Color.FromArgb("#2196F3");
            button.TextColor = Colors.White;

            // Update filter based on button
            if (button == BtnAllOrders)
            {
                _viewModel.FilterStatus = "all";
                OrdersCollectionView.ItemsSource = _viewModel.AllOrders;
            }
            else if (button == BtnPendingOrders)
            {
                _viewModel.FilterStatus = "pendiente";
                OrdersCollectionView.ItemsSource = _viewModel.PendingOrders;
            }
            else if (button == BtnInProgressOrders)
            {
                _viewModel.FilterStatus = "en_proceso";
                OrdersCollectionView.ItemsSource = _viewModel.InProgressOrders;
            }
            else if (button == BtnCompletedOrders)
            {
                _viewModel.FilterStatus = "completado";
                OrdersCollectionView.ItemsSource = _viewModel.CompletedOrders;
            }
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await _viewModel.LoadOrdersAsync();
        RefreshOrders.IsRefreshing = false;
    }

    private void OnOrderSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Order order)
        {
            _viewModel.SelectedOrder = order;
        }
    }

    private async void OnViewOrderDetails(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Order order)
        {
            // TODO: Navigate to OrderDetailsPage
            await DisplayAlert("Detalles", $"Ver detalles del pedido {order.Id}", "OK");
        }
    }

    private async void OnUpdateStatus(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Order order)
        {
            var action = await DisplayActionSheet(
                "Actualizar Estado",
                "Cancelar",
                null,
                "Pendiente",
                "En Preparación",
                "Listo",
                "En Camino",
                "Entregado");

            if (!string.IsNullOrEmpty(action) && action != "Cancelar")
            {
                string newStatus = action switch
                {
                    "Pendiente" => "pendiente",
                    "En Preparación" => "en_preparacion",
                    "Listo" => "listo",
                    "En Camino" => "en_camino",
                    "Entregado" => "entregado",
                    _ => order.Status
                };

                _viewModel.SelectedOrder = order;
                await _viewModel.UpdateOrderStatusAsync(newStatus);
            }
        }
    }
}