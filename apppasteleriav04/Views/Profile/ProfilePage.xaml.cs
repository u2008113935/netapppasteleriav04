using System;
using Microsoft.Maui.Controls;
using apppasteleriav04.ViewModels.Profile;

namespace apppasteleriav04.Views.Profile
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _viewModel;

        public ProfilePage()
        {
            InitializeComponent();
            _viewModel = new ProfileViewModel();
            BindingContext = _viewModel;

            // Subscribe to events
            _viewModel.AuthenticationRequired += OnAuthenticationRequired;
            _viewModel.LogoutCompleted += OnLogoutCompleted;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            System.Diagnostics.Debug.WriteLine($"[ProfilePage] OnAppearing");

            // Check authentication
            if (!_viewModel.CheckAuthentication())
            {
                return;
            }

            await _viewModel.LoadProfileAsync();
        }

        private async void OnAuthenticationRequired(object? sender, EventArgs e)
        {
            bool goToLogin = await DisplayAlert(
                "Iniciar Sesión",
                "Debes iniciar sesión para ver tu perfil.",
                "Ir a Login",
                "Cancelar");

            if (goToLogin)
            {
                await Shell.Current.GoToAsync("login?returnTo=profile");
            }
        }

        private async void OnLogoutCompleted(object? sender, EventArgs e)
        {
            await DisplayAlert("Sesión cerrada", "Has cerrado sesión correctamente", "OK");
            await Shell.Current.GoToAsync("//catalog");
        }
    }
}