using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Profile.Employee;

namespace apppasteleriav04.Views.Profile.Employee;

public partial class EmployeeKitchenPage : ContentPage
{
	private readonly EmployeeKitchenViewModel _viewModel;

	public EmployeeKitchenPage()
	{
		InitializeComponent();
		_viewModel = new EmployeeKitchenViewModel();
		BindingContext = _viewModel;
		OrdersCollectionView.ItemsSource = _viewModel.OrdersToPrepare;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadOrdersAsync();
		UpdateOrdersCount();
	}

	private void UpdateOrdersCount()
	{
		var count = _viewModel.OrdersToPrepare?.Count ?? 0;
		LblOrdersCount.Text = count == 1 
			? "1 pedido pendiente" 
			: $"{count} pedidos pendientes";
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

	private async void OnStartPreparingClicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.BindingContext is Order order)
		{
			await _viewModel.StartPreparingAsync(order);
			
			// Show current order panel with short ID format
			CurrentOrderFrame.IsVisible = true;
			var shortId = order.Id.ToString("N")[..8].ToUpper();
			LblCurrentOrderId.Text = $"Pedido #{shortId}";
			
			// TODO: Start timer
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

	private async void OnMarkReadyClicked(object sender, EventArgs e)
	{
		if (_viewModel.CurrentOrder == null) return;

		bool confirm = await DisplayAlert(
			"Confirmar", 
			"¿Marcar este pedido como listo?", 
			"Sí", 
			"No");

		if (confirm)
		{
			await _viewModel.MarkOrderReadyAsync();
			CurrentOrderFrame.IsVisible = false;
			UpdateOrdersCount();
			await DisplayAlert("Éxito", "Pedido marcado como listo", "OK");
		}
	}
}
