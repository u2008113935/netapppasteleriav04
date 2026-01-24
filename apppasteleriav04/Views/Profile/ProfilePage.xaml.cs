using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;
using Microsoft.Maui.Controls;
using apppasteleriav04.ViewModels.Profile;



namespace apppasteleriav04.Views.Profile
{
    public partial class ProfilePage : ContentPage
    {
        // Campo privado para el ViewModel
        private readonly ProfileViewModel _viewModel;


        // =================================
        // CONSTRUCTOR
        // =================================

        // Se ejecuta cuando se crea la pagina
        public ProfilePage()
        {
            InitializeComponent();
            _viewModel = new ProfileViewModel();
            BindingContext = _viewModel;

            // Subscribe to events
            _viewModel.AuthenticationRequired += OnAuthenticationRequired;
            _viewModel.LogoutCompleted += OnLogoutCompleted;
        }

        // Se ejecuta cada vez que la pagina aparece
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

        // Dispara el evento cuando se requiere autenticación
        private async void OnAuthenticationRequired(object? sender, EventArgs e)
        {
            // Mostrar alerta para iniciar sesión con dios botones
            bool goToLogin = await DisplayAlert(
                title: "Iniciar Sesión",
                message: "Debes iniciar sesión para ver tu perfil.",
                accept: "Ir a Login",
                cancel: "Cancelar");

            // Si el usuario eligio ir ir a login, navegar a la pagina de login
            if (goToLogin)
            {
                await Shell.Current.GoToAsync("login?returnTo=profile");
            }
        }

        // Dispara el evento cuando se completa el cierre de sesión
        private async void OnLogoutCompleted(object? sender, EventArgs e)
        {
            // Mostrar confirmacion de cierre de sesión
            await DisplayAlert(
                title:"Sesión cerrada", 
                message:"Has cerrado sesión correctamente",
                cancel:"OK");

            // Forzar reload de AppShell para actualizar OnNavigated
            await Shell.Current.GoToAsync("//catalog");
        }
    }
}