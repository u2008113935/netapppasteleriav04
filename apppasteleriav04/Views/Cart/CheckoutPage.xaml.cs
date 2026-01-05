using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Models.Domain;
using System.Diagnostics;

namespace apppasteleriav04.Views.Cart
{
    public partial class CheckoutPage : ContentPage
    {
        readonly CartService _cart = CartService.Instance;
        readonly SupabaseService _supabase = SupabaseService.Instance;
        const decimal DefaultShipping = 1500m;

        public CheckoutPage()
        {
            InitializeComponent();
            CartCollection.ItemsSource = _cart.Items;

            _cart.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_cart.Total) || e.PropertyName == nameof(_cart.Count))
                    UpdateAmounts();
            };

            UpdateAmounts();

            if (PaymentPicker?.Items?.Count > 0)
                PaymentPicker.SelectedIndex = 0;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Debug.WriteLine($"[CheckoutPage] OnAppearing");
            Debug.WriteLine($"[CheckoutPage] IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
            Debug.WriteLine($"[CheckoutPage] UserId: {AuthService.Instance.UserId}");
            Debug.WriteLine($"[CheckoutPage] UserEmail: {AuthService.Instance.UserEmail}");

            // VERIFICAR AUTENTICACION
            if (!AuthService.Instance.IsAuthenticated)
            {
                Debug.WriteLine("[CheckoutPage] Usuario NO autenticado, redirigiendo.. .");

                await DisplayAlert(
                    "Sesion Requerida",
                    "Debes iniciar sesion para continuar con el pedido.",
                    "OK");

                // Ir a login con returnTo=checkout
                await Shell.Current.GoToAsync(".. "); // Volver atras primero
                await Shell.Current.GoToAsync("login? returnTo=checkout");
                return;
            }

            Debug.WriteLine($"[CheckoutPage] Usuario autenticado:  {AuthService.Instance.UserEmail}");
        }

        void UpdateAmounts()
        {
            var subtotal = _cart.Total;
            decimal shipping = DeliverySwitch?.IsToggled == true ? DefaultShipping : 0m;
            var total = subtotal + shipping;

            SubtotalLabel.Text = $"{subtotal:N2}";
            ShippingLabel.Text = $"{shipping: N2}";
            TotalLabel.Text = $"{total:N2}";
        }

        void OnDeliveryToggled(object sender, ToggledEventArgs e)
        {
            UpdateAmounts();
        }

        void OnPaymentMethodChanged(object sender, EventArgs e)
        {
            if (PaymentPicker == null) return;
            var idx = PaymentPicker.SelectedIndex;
            CardDetailsStack.IsVisible = (idx == 1 || idx == 2);
        }

        async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(". .");
        }

        async void OnPlaceOrderClicked(object sender, EventArgs e)
        {
            // Verificar autenticacion nuevamente
            if (!AuthService.Instance.IsAuthenticated)
            {
                await DisplayAlert("Sesion Expirada", "Tu sesion ha expirado.  Inicia sesion nuevamente.", "OK");
                await Shell.Current.GoToAsync("login? returnTo=checkout");
                return;
            }

            if (!_cart.Items.Any())
            {
                await DisplayAlert("Carrito vacio", "Agrega productos antes de continuar.", "OK");
                return;
            }

            if (DeliverySwitch.IsToggled && string.IsNullOrWhiteSpace(AddressEditor.Text))
            {
                await DisplayAlert("Direccion requerida", "Por favor ingresa la direccion de entrega.", "OK");
                return;
            }

            var paymentIndex = PaymentPicker.SelectedIndex;
            if ((paymentIndex == 1 || paymentIndex == 2) &&
                (string.IsNullOrWhiteSpace(CardNumberEntry.Text) || string.IsNullOrWhiteSpace(CardHolderEntry.Text)))
            {
                await DisplayAlert("Pago", "Ingresa los datos de la tarjeta.", "OK");
                return;
            }

            // Obtener userId
            Guid userId;
            var userIdStr = AuthService.Instance.UserId;

            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out userId))
            {
                await DisplayAlert("Sesion requerida", "Debes iniciar sesion para completar el pedido.", "OK");
                await Shell.Current.GoToAsync("login? returnTo=checkout");
                return;
            }

            // Configurar token
            var token = await AuthService.Instance.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _supabase.SetUserToken(token);
                Debug.WriteLine($"[Checkout] Token configurado correctamente");
            }

            var itemsPayload = _cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            PlaceOrderButton.IsEnabled = false;
            CheckoutActivity.IsVisible = true;
            CheckoutActivity.IsRunning = true;

            try
            {
                var createdOrder = await _supabase.CreateOrderAsync(userId, itemsPayload);

                if (createdOrder == null)
                    throw new Exception("El servicio no devolvio informacion del pedido creado.");

                _cart.Clear();

                var msg = $"Pedido {createdOrder?.Id} creado correctamente.\nTotal: {createdOrder?.Total:N2}";
                await DisplayAlert("Pedido confirmado", msg, "OK");

                await Shell.Current.GoToAsync("//orders");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo crear el pedido: {ex.Message}", "OK");
            }
            finally
            {
                PlaceOrderButton.IsEnabled = true;
                CheckoutActivity.IsRunning = false;
                CheckoutActivity.IsVisible = false;
            }
        }
    }
}