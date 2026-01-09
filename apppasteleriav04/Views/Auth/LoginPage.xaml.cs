using System;
using Microsoft.Maui.Controls;
using apppasteleriav04.ViewModels.Auth;
using apppasteleriav04.Services.Core; 


namespace apppasteleriav04.Views.Auth
{
    [QueryProperty(nameof(ReturnTo), "returnTo")]
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;

        //MVVM: Propiedad para navegación responsabilidad de la View
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
                // Cargar carrito tras login (si el ViewModel no lo hace)
                try
                {
                    await CartService.Instance.LoadLocalAsync();
                    System.Diagnostics.Debug.WriteLine("[LoginPage] Carrito cargado tras login");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginPage] Error cargando carrito:  {ex.Message}");
                }

                // Mostrar mensaje de éxito
                var message = "Sesión iniciada correctamente";

                // Agregar mensaje de carrito restaurado si existe
                if (!string.IsNullOrEmpty(_viewModel.CartRestoredMessage))
                {
                    message += $"\n\n{_viewModel.CartRestoredMessage}";
                }

                await DisplayAlert("Éxito", message, "OK");

                // MVVM: Navegación (responsabilidad de la View)
                await NavigateAfterLoginAsync();
            }
            else
            {
                await DisplayAlert("Error", e.Message, "OK");
            }
        }

        // Método separado para navegación (clean code)
        private async Task NavigateAfterLoginAsync()
        {
            switch (ReturnTo?.ToLower())
            {
                case "cart":
                case "checkout":
                    // Si viene del carrito o checkout, ir directo a checkout
                    await Shell.Current.GoToAsync("checkout");
                    break;

                case "profile":
                    await Shell.Current.GoToAsync("//profile");
                    break;

                default:
                    // Default:  ir al catálogo
                    await Shell.Current.GoToAsync("//catalog");
                    break;
            }
        }
        
        async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("register");
            //await DisplayAlert("Crear cuenta", "Funcionalidad de registro pendiente.", "OK");
        }
    }
}