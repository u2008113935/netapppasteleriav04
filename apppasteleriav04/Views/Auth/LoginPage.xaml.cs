using System;
using Microsoft.Maui.Controls;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Views.Cart;

namespace apppasteleriav04.Views.Auth
{
    public partial class LoginPage : ContentPage
    {
        readonly string? _returnTo;

        public LoginPage(string? returnTo = null)
        {
            InitializeComponent();
            _returnTo = returnTo;
        }

        // Botón Ingresar
        async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Ingrese correo y contraseña", "OK");
                return;
            }

            LoadingIndicator.IsVisible = true;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[LoginPage] Intentando login con:  {email}");

                bool success = await AuthService.Instance.SignInAsync(email, password);

                System.Diagnostics.Debug.WriteLine($"[LoginPage] Resultado del login: {success}");
                System.Diagnostics.Debug.WriteLine($"[LoginPage] IsAuthenticated: {AuthService.Instance.IsAuthenticated}");


                var token = AuthService.Instance.AccessToken;
                var tokenPreview = string.IsNullOrEmpty(token) ? "null" : token.Substring(0, Math.Min(20, token.Length));
                System.Diagnostics.Debug.WriteLine($"[LoginPage] AccessToken: {tokenPreview}...");

                System.Diagnostics.Debug.WriteLine($"[LoginPage] UserEmail: {AuthService.Instance.UserEmail}");
                System.Diagnostics.Debug.WriteLine($"[LoginPage] UserId: {AuthService.Instance.UserId}");


                if (success)
                {
                    // Esperar un momento para que el AuthService se actualice completamente
                    await Task.Delay(500);

                    // Verificar nuevamente el estado después del delay
                    System.Diagnostics.Debug.WriteLine($"[LoginPage] Después del delay - IsAuthenticated: {AuthService.Instance.IsAuthenticated}");

                    if (AuthService.Instance.IsAuthenticated)
                    {
                        await DisplayAlert("Éxito", "Sesión iniciada correctamente", "OK");

                        // Navegar según returnTo
                        if (_returnTo == "cart")
                        {
                            // Regresar al carrito y luego ir a checkout
                            await Navigation.PopAsync(); // Cierra LoginPage
                            await Navigation.PushAsync(new CheckoutPage()); // Va directo a checkout
                        }
                        else
                        {
                            // Comportamiento por defecto
                            await Navigation.PopAsync();
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Login exitoso pero el estado de autenticación no se actualizó", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Credenciales incorrectas", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginPage] Error en login: {ex}");
                await DisplayAlert("Error", $"Error de conexión: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }

            // Intentar iniciar sesión (AuthService debe implementar la lógica real)
            //var ok = await AuthService.Instance.SignInAsync(email, password);
            //if (!ok)
            //{
            //  await DisplayAlert("Error", "Credenciales inválidas", "OK");
            //  return;
            //}

            // Cargar/mergear carrito local para que el usuario recupere sus productos
            //await CartService.Instance.LoadLocalAsync();

            // Volver a la pantalla previa:
            // Si la pagina fue abierta desde CartPage (returnTo == "cart"), al hacer PopAsync
            // regresará al CartPage existente en la pila. Si prefieres navegar de forma distinta,
            // podemos ajustar la navegación.
            //await Navigation.PopAsync();
        }

        // Botón Crear cuenta (implementación mínima: mostrar mensaje o navegar a RegisterPage)
        async void OnRegisterClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Crear cuenta", "Funcionalidad de registro pendiente (implementa RegisterPage).", "OK");
            // Si tienes una RegisterPage, haz:
            // await Navigation.PushAsync(new RegisterPage());
        }
    }
}