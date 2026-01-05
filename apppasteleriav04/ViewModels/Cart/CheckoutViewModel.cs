using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Cart
{
    public class CheckoutViewModel : BaseViewModel
    {
        private readonly CartService _cart = CartService.Instance;
        private readonly SupabaseService _supabase = SupabaseService.Instance;
        private const decimal DefaultShipping = 1500m;

        private string _address = string.Empty;
        private bool _isDelivery = true;
        private int _paymentMethodIndex = 0;
        private decimal _subtotal;
        private decimal _shippingCost;
        private decimal _total;
        private string _cardHolder = string.Empty;
        private string _cardNumber = string.Empty;
        private bool _showCardDetails;

        public event EventHandler? OrderCompleted;
        public event EventHandler? AuthenticationRequired;

        /// <summary>
        /// Gets the cart items
        /// </summary>
        public ObservableCollection<CartItem> Items => _cart.Items;

        /// <summary>
        /// Gets or sets the delivery address
        /// </summary>
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        /// <summary>
        /// Gets or sets whether delivery is enabled
        /// </summary>
        public bool IsDelivery
        {
            get => _isDelivery;
            set
            {
                if (SetProperty(ref _isDelivery, value))
                {
                    UpdateAmounts();
                }
            }
        }

        /// <summary>
        /// Gets or sets the payment method index
        /// </summary>
        public int PaymentMethodIndex
        {
            get => _paymentMethodIndex;
            set
            {
                if (SetProperty(ref _paymentMethodIndex, value))
                {
                    ShowCardDetails = (value == 1 || value == 2);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to show card details
        /// </summary>
        public bool ShowCardDetails
        {
            get => _showCardDetails;
            set => SetProperty(ref _showCardDetails, value);
        }

        /// <summary>
        /// Gets or sets the card holder name
        /// </summary>
        public string CardHolder
        {
            get => _cardHolder;
            set => SetProperty(ref _cardHolder, value);
        }

        /// <summary>
        /// Gets or sets the card number
        /// </summary>
        public string CardNumber
        {
            get => _cardNumber;
            set => SetProperty(ref _cardNumber, value);
        }

        /// <summary>
        /// Gets or sets the subtotal amount
        /// </summary>
        public decimal Subtotal
        {
            get => _subtotal;
            private set => SetProperty(ref _subtotal, value);
        }

        /// <summary>
        /// Gets or sets the shipping cost
        /// </summary>
        public decimal ShippingCost
        {
            get => _shippingCost;
            private set => SetProperty(ref _shippingCost, value);
        }

        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        public decimal Total
        {
            get => _total;
            private set => SetProperty(ref _total, value);
        }

        /// <summary>
        /// Command to place the order
        /// </summary>
        public ICommand PlaceOrderCommand { get; }

        /// <summary>
        /// Command to go back
        /// </summary>
        public ICommand GoBackCommand { get; }

        public CheckoutViewModel()
        {
            Title = "Checkout";
            PlaceOrderCommand = new AsyncRelayCommand(PlaceOrderAsync);
            GoBackCommand = new RelayCommand(() => { }); // Navigation handled by view

            _cart.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_cart.Total) || e.PropertyName == nameof(_cart.Count))
                {
                    UpdateAmounts();
                }
            };

            UpdateAmounts();
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool CheckAuthentication()
        {
            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] CheckAuthentication");
            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] UserId: {AuthService.Instance.UserId}");
            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] UserEmail: {AuthService.Instance.UserEmail}");

            if (!AuthService.Instance.IsAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] Usuario NO autenticado");
                AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] Usuario autenticado: {AuthService.Instance.UserEmail}");
            return true;
        }

        private void UpdateAmounts()
        {
            Subtotal = _cart.Total;
            ShippingCost = IsDelivery ? DefaultShipping : 0m;
            Total = Subtotal + ShippingCost;
        }

        private async Task PlaceOrderAsync()
        {
            // Verify authentication
            if (!AuthService.Instance.IsAuthenticated)
            {
                ErrorMessage = "Tu sesión ha expirado. Inicia sesión nuevamente.";
                AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                return;
            }

            // Validate cart
            if (!_cart.Items.Any())
            {
                ErrorMessage = "Agrega productos antes de continuar.";
                return;
            }

            // Validate delivery address
            if (IsDelivery && string.IsNullOrWhiteSpace(Address))
            {
                ErrorMessage = "Por favor ingresa la dirección de entrega.";
                return;
            }

            // Validate card details
            if ((PaymentMethodIndex == 1 || PaymentMethodIndex == 2) &&
                (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(CardHolder)))
            {
                ErrorMessage = "Ingresa los datos de la tarjeta.";
                return;
            }

            // Get userId
            var userIdStr = AuthService.Instance.UserId;
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                ErrorMessage = "Debes iniciar sesión para completar el pedido.";
                AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Set token
                var token = await AuthService.Instance.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _supabase.SetUserToken(token);
                    System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] Token configurado correctamente");
                }

                // Prepare order items
                var itemsPayload = _cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();

                // Create order
                var createdOrder = await _supabase.CreateOrderAsync(userId, itemsPayload);

                if (createdOrder == null)
                {
                    throw new Exception("El servicio no devolvió información del pedido creado.");
                }

                // Clear cart
                _cart.Clear();

                // Notify completion
                OrderCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"No se pudo crear el pedido: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] Error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
