using System;
using Microsoft.Maui.Controls;
using apppasteleriav04.ViewModels.Auth;

namespace apppasteleriav04.Views.Auth
{
    [QueryProperty(nameof(ReturnTo), "returnTo")]
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;
        public string ReturnTo { get; set; } = string.Empty;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            BindingContext = _viewModel;

            // Subscribe to login completion event
            _viewModel.LoginCompleted += OnLoginCompleted;
        }

        private async void OnLoginCompleted(object? sender, LoginCompletedEventArgs e)
        {
            if (e.Success)
            {
                await DisplayAlert("Éxito", "Sesión iniciada correctamente", "OK");

                // Navigate based on returnTo parameter
                if (ReturnTo == "cart")
                {
                    await Shell.Current.GoToAsync("//cart");
                }
                else if (ReturnTo == "checkout")
                {
                    await Shell.Current.GoToAsync("//cart");
                    await Shell.Current.GoToAsync("checkout");
                }
                else if (ReturnTo == "profile")
                {
                    await Shell.Current.GoToAsync("//profile");
                }
                else
                {
                    // Default: go to catalog
                    await Shell.Current.GoToAsync("//catalog");
                }
            }
            else
            {
                await DisplayAlert("Error", e.Message, "OK");
            }
        }

        async void OnRegisterClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Crear cuenta", "Funcionalidad de registro pendiente.", "OK");
        }
    }
}