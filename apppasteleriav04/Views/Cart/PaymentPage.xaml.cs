using apppasteleriav04.ViewModels.Cart;

namespace apppasteleriav04.Views.Cart;

public partial class PaymentPage : ContentPage
{
    public PaymentPage()
    {
        InitializeComponent();
    }

    public PaymentPage(PaymentViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}