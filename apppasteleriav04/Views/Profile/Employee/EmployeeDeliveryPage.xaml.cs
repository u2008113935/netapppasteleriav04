using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Profile.Employee;

namespace apppasteleriav04.Views.Profile.Employee;

public partial class EmployeeDeliveryPage : ContentPage
{
    private readonly EmployeeDeliveryViewModel _viewModel;

    public EmployeeDeliveryPage()
    {
        InitializeComponent();
        _viewModel = new EmployeeDeliveryViewModel();
        BindingContext = _viewModel;
        OrdersCollectionView.ItemsSource = _viewModel.OrdersToDeliver;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadOrdersAsync();
        UpdateOrdersCount();
    }

    private void UpdateOrdersCount()
    {
        var count = _viewModel.OrdersToDeliver?.Count ?? 0;
        LblOrdersCount.Text = count == 1
            ? "1 pedido para entregar"
            : $"{count} pedidos para entregar";
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await _viewModel.LoadOrdersAsync();
        UpdateOrdersCount();
        RefreshOrders.IsRefreshing = false;
    }

    private void OnOrderSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Order order)
        {
            // Handle selection if needed
        }
    }

    private async void OnStartDeliveryClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Order order)
        {
            await _viewModel.StartDeliveryAsync(order);

            // Show current order panel with short ID format
            CurrentOrderFrame.IsVisible = true;
            var shortId = order.Id.ToString("N")[..8].ToUpper();
            LblCurrentOrderId.Text = $"Pedido #{shortId}";

            UpdateOrdersCount();
        }
    }

    private async void OnViewCurrentOrderClicked(object sender, EventArgs e)
    {
        if (_viewModel.CurrentOrder != null)
        {
            // TODO: Navigate to order details
            await DisplayAlert("Detalles", $"Ver detalles del pedido {_viewModel.CurrentOrder.Id}", "OK");
        }
    }

    private async void OnMarkDeliveredClicked(object sender, EventArgs e)
    {
        if (_viewModel.CurrentOrder == null) return;

        bool confirm = await DisplayAlert(
            "Confirmar",
            "¿Confirmar entrega de este pedido?",
            "Sí",
            "No");

        if (confirm)
        {
            await _viewModel.MarkOrderDeliveredAsync();
            CurrentOrderFrame.IsVisible = false;
            UpdateOrdersCount();
            await DisplayAlert("Éxito", "Pedido marcado como entregado", "OK");
        }
    }
}