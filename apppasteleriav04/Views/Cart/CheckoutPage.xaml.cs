using System;
using Microsoft.Maui.Controls;
using apppasteleriav04.ViewModels.Cart;
using System.Diagnostics;

namespace apppasteleriav04.Views.Cart
{
    public partial class CheckoutPage : ContentPage
    {
        private readonly CheckoutViewModel _viewModel;

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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Check authentication
            if (!_viewModel.CheckAuthentication())
            {
                return;
            }
        }

        private async void OnOrderCompleted(object? sender, EventArgs e)
        {
            var msg = $"Pedido creado correctamente.\nTotal: {_viewModel.Total:N2}";
            await DisplayAlert("Pedido confirmado", msg, "OK");
            await Shell.Current.GoToAsync("//orders");
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