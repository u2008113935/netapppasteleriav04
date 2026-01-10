using apppasteleriav04.Services.Core;
using Microsoft.Maui.Controls;
using System;
using System.Linq;

namespace apppasteleriav04.Views.Cart
{
    public partial class CartPage : ContentPage
    {
        readonly CartService _cart = CartService.Instance;

        public CartPage()
        {
            InitializeComponent();
            BindingContext = _cart;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine($"[CartPage] OnAppearing -> Items. Count={_cart.Items.Count}");
            System.Diagnostics.Debug.WriteLine($"[CartPage] IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"[CartPage] UserId: {AuthService.Instance.UserId}");
            System.Diagnostics.Debug.WriteLine($"[CartPage] UserEmail: {AuthService.Instance.UserEmail}");
        }

        void OnIncreaseClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Guid productId)
            {
                var existing = _cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (existing != null) _cart.UpdateQuantity(productId, existing.Quantity + 1);
            }
        }

        void OnDecreaseClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Guid productId)
            {
                var existing = _cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (existing != null) _cart.UpdateQuantity(productId, existing.Quantity - 1);
            }
        }

        void OnRemoveClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Guid productId)
                _cart.Remove(productId);
        }

        async void OnConfirmPurchaseClicked(object sender, EventArgs e)
        {
            if (_cart.Items.Count == 0)
            {
                await DisplayAlert("Carrito vacio", "No hay productos en el carrito.", "OK");
                return;
            }

            // Guardar carrito local
            try
            {
                await _cart.SaveLocalAsync();
                System.Diagnostics.Debug.WriteLine("[CartPage] Carrito guardado localmente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CartPage] Error guardando carrito: {ex.Message}");
            }

            // VERIFICAR AUTENTICACION
            System.Diagnostics.Debug.WriteLine($"[CartPage] Verificando autenticacion...");
            System.Diagnostics.Debug.WriteLine($"[CartPage] IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"[CartPage] AccessToken is null?:  {string.IsNullOrEmpty(AuthService.Instance.AccessToken)}");

            if (!AuthService.Instance.IsAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[CartPage] Usuario NO autenticado, mostrando dialogo.. .");

                bool goToLogin = await DisplayAlert(
                    "Iniciar Sesion Requerido",
                    "Debes iniciar sesion para realizar tu pedido.",
                    "Ir a Login",
                    "Cancelar");

                if (goToLogin)
                {
                    // Navegar a login con parametro returnTo=cart
                    System.Diagnostics.Debug.WriteLine("[CartPage] Navegando a login con returnTo=checkout");
                    await Shell.Current.GoToAsync("login?returnTo=checkout");
                }
                return;
            }

            // Si esta autenticado, ir a Checkout
            System.Diagnostics.Debug.WriteLine("[CartPage] Usuario autenticado, navegando a checkout...");
            await Shell.Current.GoToAsync("checkout");
        }
    }
}