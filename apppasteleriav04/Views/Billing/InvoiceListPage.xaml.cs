using apppasteleriav04.ViewModels.Billing;

namespace apppasteleriav04.Views.Billing;

public partial class InvoiceListPage : ContentPage
{
    private readonly InvoiceListViewModel _viewModel;

    public InvoiceListPage()
    {
        InitializeComponent();
        _viewModel = new InvoiceListViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
