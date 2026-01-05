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
        
        // Set default picker selection
        TypePicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        if (TypePicker.SelectedIndex >= 0)
        {
            var selectedType = TypePicker.Items[TypePicker.SelectedIndex];
            _viewModel.FilterType = selectedType;
        }
    }
}

