using apppasteleriav04.ViewModels.Profile.Employee;

namespace apppasteleriav04.Views.Profile.Employee;

public partial class EmployeePage : ContentPage
{
    private readonly EmployeeViewModel _viewModel;

    public EmployeePage()
    {
        InitializeComponent();
        _viewModel = new EmployeeViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadProfileCommand.Execute(null);
    }

    private async void OnKitchenClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("kitchen");
    }

    private async void OnDeliveryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("delivery");
    }

    private async void OnInventoryClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Inventario", "Funcionalidad de inventario próximamente", "OK");
    }

    private async void OnSyncClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("employee-sync");
    }
}