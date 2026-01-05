using System;
using Microsoft.Maui.Controls;
using apppasteleriav04.Services.Core;

namespace apppasteleriav04.Views.Auth
{
    [QueryProperty(nameof(ReturnTo), "returnTo")]
    public partial class LoginPage : ContentPage
    {
        public string ReturnTo { get; set; }

        public LoginPage()
        {
            InitializeComponent();
        }

        async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Ingrese correo y contrasena", "OK");
                return;
            }

            LoadingIndicator.IsVisible = true;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[LoginPage] Intentando login con:  {email}");

                bool success = await AuthService.Instance.SignInAsync(email, password);

                System.Diagnostics.Debug.WriteLine($"[LoginPage] Resultado del login: {success}");
                System.Diagnostics.Debug.WriteLine($"[LoginPage] IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
                System.Diagnostics.Debug.WriteLine($"[LoginPage] UserEmail: {AuthService.Instance.UserEmail}");
                System.Diagnostics.Debug.WriteLine($"[LoginPage] UserId: {AuthService.Instance.UserId}");

                if (success && AuthService.Instance.IsAuthenticated)
                {
                    await DisplayAlert("Exito", "Sesion iniciada correctamente", "OK");

                    // Navegar segun returnTo usando Shell
                    if (ReturnTo == "cart")
                    {
                        // Volver al carrito (el usuario puede confirmar y luego ir a checkout)
                        await Shell.Current.GoToAsync("//cart");
                    }
                    else if (ReturnTo == "checkout")
                    {
                        // Ir directo a checkout
                        await Shell.Current.GoToAsync("//cart");
                        await Shell.Current.GoToAsync("checkout");
                    }
                    else if (ReturnTo == "profile")
                    {
                        await Shell.Current.GoToAsync("//profile");
                    }
                    else
                    {
                        // Comportamiento por defecto:  ir al catalogo
                        await Shell.Current.GoToAsync("//catalog");
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
                await DisplayAlert("Error", $"Error de conexion: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }

        async void OnRegisterClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Crear cuenta", "Funcionalidad de registro pendiente.", "OK");
        }
    }
}