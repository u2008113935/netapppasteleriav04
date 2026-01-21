using System;
using Microsoft.Maui.Controls;
using apppasteleriav04.ViewModels.Auth;
using apppasteleriav04.Services.Core;
using System.Threading.Tasks;

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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Desuscribir para evitar fugas si la página se instancia de nuevo
            _viewModel.LoginCompleted -= OnLoginCompleted;
        }

        private async void OnLoginCompleted(object? sender, LoginCompletedEventArgs e)
        {
            if (e.Success)
            {
                // Ya no llamamos a LoadLocalAsync (método comentado).
                // El ViewModel realiza la migración y carga del carrito (LoadCartAfterLoginAsync).
                // Aquí solo mostramos mensajes y navegamos.
                
                var message = "Sesión iniciada correctamente";

                /*
                try
                {
                    //await CartService.Instance.LoadLocalAsync();
                    System.Diagnostics.Debug.WriteLine("[LoginPage] Carrito cargado tras login");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginPage] Error cargando carrito:  {ex.Message}");
                }
                */
                // Agregar mensaje de carrito restaurado si existe (lo establece el ViewModel)
                if (!string.IsNullOrEmpty(_viewModel.CartRestoredMessage))
                {
                    message += $"\n\n{_viewModel.CartRestoredMessage}";
                }

                await DisplayAlert("Éxito", message, "OK");

                // MVVM: Navegación (responsabilidad de la View)
                await NavigateAfterLoginAsync();

                // Mostrar mensaje de éxito
                //var message = "Sesión iniciada correctamente";

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

        // Método actualizado para navegar después del rol
        private async Task NavigateAfterLoginAsync()
        {
            //Obetener el rol del usuario autenticado
            var userRole = AuthService.Instance.UserRole;
            var userEmail = AuthService.Instance.UserEmail;

            System.Diagnostics.Debug.WriteLine($"[LoginPage] Navegando usuario: {userEmail} (Rol: {userRole})");


            // Si viene de un parametro especifico (cart, checkout, profile) navegar ahi
            if (!string.IsNullOrEmpty(ReturnTo))
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

            // Navegar segun el rol
            if(AuthService.Instance.IsEmployee())
            {
                System.Diagnostics.Debug.WriteLine($"[LoginPage] Empleado detectado (Rol: {userRole})");

                //Navegar segun el tipo de empleado
                if (AuthService.Instance.IsCocina())
                {
                    await Shell.Current.GoToAsync("employee-kitchen");
                }
                else if (AuthService.Instance.IsReparto())
                {
                    await Shell.Current.GoToAsync("employee-delivery");
                }
                else if (AuthService.Instance.IsBackoffice())
                {
                    await Shell.Current.GoToAsync("employee-backoffice");
                }
                else 
                {                     
                    // Default empleado:  ir al dashboard general
                    await Shell.Current.GoToAsync("employee-dashboard");
                }
            }
            else if (AuthService.Instance.IsCliente())
            {
                System.Diagnostics.Debug.WriteLine($"[LoginPage] Cliente detectado (Rol: {userRole})");
                //Cliente: ir al catalogo
                await Shell.Current.GoToAsync("//catalog");
            }            
            else
            {
                System.Diagnostics.Debug.WriteLine($"[LoginPage] Rol desconocido, navegando a default");
                await Shell.Current.GoToAsync("//catalog");
            }
        }
        
        async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("register");
            //await DisplayAlert("Crear cuenta", "Funcionalidad de registro pendiente.", "OK");
        }
    }
}