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
        // Show contact information
        var result = await Shell.Current.DisplayAlert("Soporte", 
            "¿Cómo desea contactarnos?\n\nEmail: soporte@pasteleriadelicia.com\nTeléfono: (01) 555-1234", 
            "Llamar", "Cerrar");
        
        if (result)
        {
            // Open phone dialer
            try
            {
                PhoneDialer.Open("015551234");
            }
            catch
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo abrir el marcador", "OK");
            }
        }
    }
}
