using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Profile.Employee;

namespace apppasteleriav04.Views.Profile.Employee;

public partial class KitchenPage : ContentPage
{
	private readonly KitchenViewModel _viewModel;

	public KitchenPage()
	{
		InitializeComponent();
		_viewModel = new KitchenViewModel();
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
			
			// Show current order panel
			CurrentOrderFrame.IsVisible = true;
			LblCurrentOrderId.Text = $"Pedido #{order.Id:N}";
			
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
