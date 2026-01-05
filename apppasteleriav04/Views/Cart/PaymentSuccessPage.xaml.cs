namespace apppasteleriav04.Views.Cart;

public partial class PaymentSuccessPage : ContentPage
{
    public PaymentSuccessPage()
    {
        InitializeComponent();
    }

    private async void OnViewInvoiceClicked(object sender, EventArgs e)
    {
        // TODO: Navigate to invoice page
        await Shell.Current.DisplayAlert("Comprobante", "Generando comprobante...", "OK");
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        // Navigate to catalog/home
        await Shell.Current.GoToAsync("//catalog");
    }
}
