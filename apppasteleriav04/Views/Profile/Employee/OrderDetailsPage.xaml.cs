using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.Views.Profile.Employee;

public partial class OrderDetailsPage : ContentPage
{
	private Order? _currentOrder;

	public OrderDetailsPage()
	{
		InitializeComponent();
	}

	public OrderDetailsPage(Order order) : this()
	{
		LoadOrderDetails(order);
	}

	private void LoadOrderDetails(Order order)
	{
		_currentOrder = order;

		if (order == null) return;

		// Update UI with order details - use short ID format for readability
		var shortId = order.Id.ToString("N")[..8].ToUpper();
		LblOrderId.Text = $"Pedido #{shortId}";
		LblStatus.Text = order.Status;
		LblOrderDate.Text = $"Fecha: {order.CreatedAt:dd/MM/yyyy HH:mm}";
		LblTotal.Text = $"S/ {order.Total:F2}";

		// Update customer information (placeholder for now)
		LblCustomerName.Text = $"Cliente ID: {order.UserId.ToString("N")[..8].ToUpper()}";
		LblCustomerEmail.Text = "Email no disponible";
		LblCustomerPhone.Text = "Teléfono no disponible";

		// Load order items
		ItemsCollectionView.ItemsSource = order.Items;

		// Show/hide delivery info based on order status
		bool hasDeliveryInfo = order.RepartidorAsignado.HasValue;
		DeliveryInfoFrame.IsVisible = hasDeliveryInfo;
		BtnAssignDelivery.IsVisible = !hasDeliveryInfo && (order.Status == "listo" || order.Status == "pendiente");
		BtnViewTracking.IsVisible = hasDeliveryInfo && order.Status == "en_camino";

		if (hasDeliveryInfo)
		{
			LblDeliveryPerson.Text = order.RepartidorAsignado?.ToString("N")[..8].ToUpper() ?? "No asignado";
			LblDeliveryStatus.Text = order.Status;
			LblEstimatedTime.Text = order.HoraEstimadaLlegada?.ToString("HH:mm") ?? "--";
		}
	}

	private async void OnUpdateStatusClicked(object sender, EventArgs e)
	{
		if (_currentOrder == null) return;

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
				_ => _currentOrder.Status
			};

			// TODO: Update via EmployeeService
			_currentOrder.Status = newStatus;
			LblStatus.Text = newStatus;

			await DisplayAlert("Éxito", $"Estado actualizado a: {action}", "OK");
		}
	}

	private async void OnAssignDeliveryClicked(object sender, EventArgs e)
	{
		if (_currentOrder == null) return;

		// TODO: Show list of available delivery persons
		await DisplayAlert("Asignar Repartidor", "Función de asignación de repartidor", "OK");
	}

	private async void OnViewTrackingClicked(object sender, EventArgs e)
	{
		if (_currentOrder == null) return;

		// TODO: Navigate to tracking page
		await DisplayAlert("Seguimiento", "Navegando a seguimiento en tiempo real...", "OK");
	}
}
