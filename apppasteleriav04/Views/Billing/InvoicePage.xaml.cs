using apppasteleriav04.ViewModels.Billing;

namespace apppasteleriav04.Views.Billing;

public partial class InvoicePage : ContentPage
{
    public InvoicePage()
    {
        InitializeComponent();
    }

    public InvoicePage(InvoiceViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}
