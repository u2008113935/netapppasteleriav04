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
        // ATRIBUTOS
        private readonly CartService _cartService;

        public ObservableCollection<CartItem> Items => _cartService.Items;

        private string _address = string.Empty;
        
        // PROPIEDADES
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

        public bool CheckAuthentication()
        {
            return AuthService.Instance.IsAuthenticated;
        }

        private async Task PlaceOrderAsync()
        {
            ErrorMessage = string.Empty;
            System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] PlaceOrderAsync iniciado");

            //Validacion 1 : Usuario autenticado
            if (!AuthService.Instance.IsAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] Usuario NO autenticado");
                AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                return;
            }

            //Validacion 2 : Direccion de entrega si es delivery
            if (IsDelivery && string.IsNullOrWhiteSpace(Address))
            {
                System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] Dirección vacía");
                ErrorMessage = "Por favor ingrese su dirección de entrega";
                return;
            }

            //Validacion 3 : Carrito no vacío
            if (Items.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] Carrito vacío");
                ErrorMessage = "El carrito está vacío";
                return;
            }

            IsBusy = true;
            System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] Iniciando creación de pedido...");

            try
            {
                var userId = Guid.Parse(AuthService.Instance.UserId ?? Guid.Empty.ToString());
                System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] UserId: {userId}");

                // Convert CartItems to OrderItems
                var orderItems = Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] Items a enviar: {orderItems.Count}");
                System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] Total: {Total}");

                // Create order in Supabase
                System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] Llamando a SupabaseService. CreateOrderAsync.. .");
                var createdOrder = await SupabaseService.Instance.CreateOrderAsync(userId, orderItems);
                
                if (createdOrder != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] Pedido creado:  {createdOrder.Id}");

                    //Limpiar carrito
                    _cartService.Clear();
                    System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] Carrito limpiado");

                    OrderCompleted?.Invoke(this, createdOrder.Id);
                    System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] Evento OrderCompleted disparado");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] createdOrder es NULL");
                    ErrorMessage = "No se pudo crear el pedido. Por favor intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] EXCEPCIÓN: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] StackTrace: {ex.StackTrace}");
                ErrorMessage = $"Error al crear el pedido: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] PlaceOrderAsync finalizado");
            }
        }
    }
}
