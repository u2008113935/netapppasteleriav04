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
        private readonly CartService _cartService;

        public ObservableCollection<CartItem> Items => _cartService.Items;

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        private bool _isDelivery = true;
        public bool IsDelivery
        {
            get => _isDelivery;
            set
            {
                SetProperty(ref _isDelivery, value);
                OnPropertyChanged(nameof(ShippingCost));
                OnPropertyChanged(nameof(Total));
            }
        }

        private int _paymentMethodIndex = 0;
        public int PaymentMethodIndex
        {
            get => _paymentMethodIndex;
            set => SetProperty(ref _paymentMethodIndex, value);
        }

        public decimal Subtotal => _cartService.Total;

        public decimal ShippingCost => IsDelivery ? 5.00m : 0.00m;

        public decimal Total => Subtotal + ShippingCost;

        public ICommand PlaceOrderCommand { get; }
        public ICommand GoBackCommand { get; }

        public event EventHandler<Guid>? OrderCompleted;
        public event EventHandler? AuthenticationRequired;
        public event EventHandler? GoBackRequested;

        public CheckoutViewModel()
        {
            _cartService = CartService.Instance;
            Title = "Finalizar Compra";

            PlaceOrderCommand = new AsyncRelayCommand(PlaceOrderAsync, () => !IsBusy);
            GoBackCommand = new RelayCommand(() => GoBackRequested?.Invoke(this, EventArgs.Empty));
        }

        private async Task PlaceOrderAsync()
        {
            ErrorMessage = string.Empty;

            if (!AuthService.Instance.IsAuthenticated)
            {
                AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (IsDelivery && string.IsNullOrWhiteSpace(Address))
            {
                ErrorMessage = "Por favor ingrese su dirección de entrega";
                return;
            }

            if (Items.Count == 0)
            {
                ErrorMessage = "El carrito está vacío";
                return;
            }

            IsBusy = true;

            try
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse(AuthService.Instance.UserId ?? Guid.Empty.ToString()),
                    Total = Total,
                    Status = "pendiente",
                    CreatedAt = DateTime.UtcNow
                };

                // Create order in Supabase
                var createdOrder = await SupabaseService.Instance.CreateOrderAsync(order, Items.ToList());

                if (createdOrder != null)
                {
                    _cartService.Clear();
                    OrderCompleted?.Invoke(this, createdOrder.Id);
                }
                else
                {
                    ErrorMessage = "No se pudo crear el pedido. Por favor intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al crear el pedido: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
