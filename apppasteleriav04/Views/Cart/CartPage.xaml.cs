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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CartPage] Error guardando carrito local: {ex.Message}");
            }

            // Verificar autenticación
            if (!AuthService.Instance.IsAuthenticated)
            {
                bool goToLogin = await DisplayAlert(
                    "Iniciar Sesion Requerido",
                    "Debes iniciar sesion para realizar tu pedido.",
                    "Ir a Login",
                    "Cancelar");

                if (goToLogin)
                {
                    await Shell.Current.GoToAsync("login? returnTo=cart");
                }
                return;
            }

            // Si está autenticado, ir a Checkout
            await Shell.Current.GoToAsync("checkout");
        }
    }
}