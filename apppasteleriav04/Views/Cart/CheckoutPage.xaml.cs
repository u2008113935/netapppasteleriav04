using System;
using Microsoft.Maui.Controls;
using apppasteleriav04.ViewModels.Cart;
using System.Diagnostics;

namespace apppasteleriav04.Views.Cart
{
    public partial class CheckoutPage : ContentPage
    {
        // ATRIBUTOS (datos privados)
        private readonly CheckoutViewModel _viewModel;

        // PROPIEDADES (ACCESO PUBLICO A LOS DATOS)
        public CheckoutPage()
        {
            InitializeComponent();
            _viewModel = new CheckoutViewModel();
            BindingContext = _viewModel;

            // Subscribe to events
            _viewModel.OrderCompleted += OnOrderCompleted;
            _viewModel.AuthenticationRequired += OnAuthenticationRequired;

            // Set default payment method
            if (PaymentPicker?.Items?.Count > 0)
                PaymentPicker.SelectedIndex = 0;
        }

        //CONSTRUCTORES | METODOS | FUNCIONES

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Check authentication
            if (!_viewModel.CheckAuthentication())
            {
                return;
            }
        }

        private async void OnOrderCompleted(object? sender, Guid orderId)
        {
            System.Diagnostics.Debug.WriteLine($"[CheckoutPage] OnOrderCompleted disparado - OrderId: {orderId}");

            try
            {
                var msg = $"Pedido creado correctamente\n\nID: {orderId.ToString().Substring(0, 8)}.. .\nTotal: S/ {_viewModel.Total:N2}";

                System.Diagnostics.Debug.WriteLine("[CheckoutPage] Mostrando alerta de éxito");
                await DisplayAlert("¡Pedido Confirmado!", msg, "Ver mis pedidos");

                System.Diagnostics.Debug.WriteLine("[CheckoutPage] Navegando a //orders");
                await Shell.Current.GoToAsync("//orders");

                System.Diagnostics.Debug.WriteLine("[CheckoutPage] Navegación completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CheckoutPage] Error en OnOrderCompleted: {ex.Message}");
                await DisplayAlert("Error", "Pedido creado pero hubo un error al navegar", "OK");
            }
        }

        private async void OnAuthenticationRequired(object? sender, EventArgs e)
        {
            await DisplayAlert(
                "Sesión Requerida",
                "Debes iniciar sesión para continuar con el pedido.",
                "OK");

            // Go back first then navigate to login
            await Shell.Current.GoToAsync("..");
            await Shell.Current.GoToAsync("login?returnTo=checkout");
        }

        async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}