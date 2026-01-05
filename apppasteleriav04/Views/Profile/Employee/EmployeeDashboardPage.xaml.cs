namespace apppasteleriav04.Views.Profile.Employee;

public partial class EmployeeDashboardPage : ContentPage
{
	public EmployeeDashboardPage()
	{
		InitializeComponent();
	}

	private async void OnViewOrdersTapped(object sender, EventArgs e)
	{
		// TODO: Navigate to EmployeeOrdersPage
		await DisplayAlert("Pedidos", "Navegando a vista de pedidos...", "OK");
	}

	private async void OnKitchenTapped(object sender, EventArgs e)
	{
		// TODO: Navigate to KitchenPage
		await DisplayAlert("Cocina", "Navegando a vista de cocina...", "OK");
	}

	private async void OnDeliveryTapped(object sender, EventArgs e)
	{
		// TODO: Navigate to DeliveryPage (to be created)
		await DisplayAlert("Reparto", "Navegando a vista de reparto...", "OK");
	}

	private async void OnSyncTapped(object sender, EventArgs e)
	{
		// Navigate to existing sync page
		await Shell.Current.GoToAsync("//profile");
	}
}