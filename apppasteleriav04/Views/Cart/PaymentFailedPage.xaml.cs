namespace apppasteleriav04.Views.Cart;

public partial class PaymentFailedPage : ContentPage
{
    public PaymentFailedPage()
    {
        InitializeComponent();
    }

    private async void OnRetryClicked(object sender, EventArgs e)
    {
        // Go back to payment page
        await Shell.Current.GoToAsync("..");
    }

    private async void OnChangeMethodClicked(object sender, EventArgs e)
    {
        // Go back to payment page
        await Shell.Current.GoToAsync("..");
    }

    private async void OnContactSupportClicked(object sender, EventArgs e)
    {
        // TODO: Navigate to support/contact page or show contact info
        await Shell.Current.DisplayAlert("Soporte", 
            "Email: soporte@pasteleriadelicia.com\nTeléfono: (01) 555-1234", 
            "OK");
    }
}
