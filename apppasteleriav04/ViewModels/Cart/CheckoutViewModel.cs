using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.Json;
using System.Diagnostics;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Models.Local;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Services.Sync;
using apppasteleriav04.Services.Connectivity;
using apppasteleriav04.Data.Local.Repositories;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Cart
{
    public class CheckoutViewModel : BaseViewModel
    {
        // ATRIBUTOS
        private readonly CartService _cartService;
        private readonly LocalOrderRepository _orderRepository;
        private readonly IConnectivityService _connectivityService;

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

        #region contructor
        //=======================================================
        // CONSTRUCTOR
        //=======================================================
        public CheckoutViewModel()
        {
            _cartService = CartService.Instance;
            _connectivityService = MauiProgram.Services.GetService<IConnectivityService>()
                ?? throw new InvalidOperationException("IConnectivityService not registered");

            //Repositorio de ordenes locales
            _orderRepository = new LocalOrderRepository();

            Title = "Finalizar Compra";
            PlaceOrderCommand = new AsyncRelayCommand(PlaceOrderAsync, () => !IsBusy);
            GoBackCommand = new RelayCommand(() => GoBackRequested?.Invoke(this, EventArgs.Empty));
        }
        #endregion

        #region public methods
        //Verificar autenticacion
        public bool CheckAuthentication()
        {
            return AuthService.Instance.IsAuthenticated;
        }
        #endregion

        #region Private Methods
        //Metodo OFFLINE-FIRST para colocar orden
        //1. Guardar en SQLite
        //2. Marcar para sincronizacion
        //3. Si falla, aplicamos la estrategia de ENCOLAR y luego reintentar.

        //Metodo para colocar orden
        private async Task PlaceOrderAsync()
        {
            ErrorMessage = string.Empty;
            Debug.WriteLine("=================================================================");
            Debug.WriteLine("[CheckoutViewModel] PlaceOrderAsync iniciado");
            Debug.WriteLine("=================================================================");

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
                Debug.WriteLine("[CheckoutViewModel] Carrito vacío");
                ErrorMessage = "El carrito está vacío";
                return;
            }

            IsBusy = true;
            Debug.WriteLine("[CheckoutViewModel] Iniciando creación de pedido...");

            // Obtener UserId
            try
            {
                var userId = Guid.Parse(AuthService.Instance.UserId ?? Guid.Empty.ToString());
                Debug.WriteLine($"[CheckoutViewModel] UserId: {userId}");


                // ═════════════════════════════════════════
                // PASO 1: CREAR ORDEN LOCAL (SIEMPRE PRIMERO)
                // ═════════════════════════════════════════
                Debug.WriteLine("[CheckoutViewModel] Paso 1: Creando orden local...");
                // Crear la orden local
                var localOrder = new LocalOrder
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Total = Total,
                    Status = "pendiente",
                    DeliveryAddress = IsDelivery ? Address : "Recojo en tienda",
                    IsDelivery = IsDelivery,
                    CreatedAt = DateTime.UtcNow,
                    IsSynced = false, // Importante: Marcar como no sincronizado, eso quiere decir que falta sincronizar
                    SyncedAt = null
                };

                // Crear los items locales
                var localItems = Items.Select(item => new LocalOrderItem
                {
                    OrderId = localOrder.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Nombre,
                    UnitPrice = item.Price,
                    Quantity = item.Quantity,
                    Subtotal = item.Price * item.Quantity
                }).ToList();

                Debug.WriteLine($"[CheckoutViewModel] Orden ID:  {localOrder.Id}");
                Debug.WriteLine($"[CheckoutViewModel] Items: {localItems.Count}");
                Debug.WriteLine($"[CheckoutViewModel] Total: S/ {localOrder.Total:N2}");

                // Guardar en SQLite
                await _orderRepository.SaveWithItemsAsync(localOrder, localItems);
                Debug.WriteLine("[CheckoutViewModel] Orden guardada en la base local SQLite");

                // ═════════════════════════════════════════
                // PASO 2: INTENTAR SINCRONIZAR
                // ═════════════════════════════════════════
                // Intentar sincronizar inmediatamente
                bool syncSuccess = false;

                if (_connectivityService.IsConnected)
                {
                    Debug.WriteLine("[CheckoutViewModel] Paso 2: Hay conexión, intentando sincronizar...");

                    try
                    {
                        var orderItems = Items.Select(item => new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        }).ToList();

                        var createdOrder = await SupabaseService.Instance.CreateOrderAsync(userId, orderItems);

                        if (createdOrder != null)
                        {
                            // Marcar como sincronizado
                            localOrder.IsSynced = true;
                            localOrder.SyncedAt = DateTime.UtcNow;
                            await _orderRepository.SaveAsync(localOrder);

                            syncSuccess = true;
                            Debug.WriteLine($"[CheckoutViewModel] Sincronizado con backend:  {createdOrder.Id}");
                        }
                    }
                    catch (Exception syncEx)
                    {
                        Debug.WriteLine($"[CheckoutViewModel] Error sincronizando: {syncEx.Message}");
                        // No lanzar excepción, continuar con sincronización posterior
                    }
                }
                else
                {
                    Debug.WriteLine("[CheckoutViewModel] Sin conexión a internet");
                }

                // ═════════════════════════════════════════
                // PASO 3: ENCOLAR SI NO SE SINCRONIZÓ
                // ═════════════════════════════════════════
                if (!syncSuccess)
                {
                    Debug.WriteLine("[CheckoutViewModel] Encolando para sincronización posterior");
                    // Obtener el servicio de sincronización
                    var syncService = MauiProgram.Services.GetService<ISyncService>();

                    // Encolar la orden para sincronización posterior
                    if (syncService != null)
                    {
                        var payload = JsonSerializer.Serialize(new
                        {
                            order = localOrder,
                            items = localItems
                        });

                        await syncService.EnqueueAsync("order", localOrder.Id, "create", payload);
                        Debug.WriteLine("[CheckoutViewModel] Orden encolada en SyncQueue");
                    }
                }

                // ═════════════════════════════════════════
                // PASO 4: LIMPIAR CARRITO Y NOTIFICAR
                // ═════════════════════════════════════════
                Debug.WriteLine("[CheckoutViewModel] Paso 4: Limpiando carrito y notificando...");

                await _cartService.ClearAsync();
                Debug.WriteLine("[CheckoutViewModel] Carrito limpiado");

                OrderCompleted?.Invoke(this, localOrder.Id);
                Debug.WriteLine("[CheckoutViewModel] Evento OrderCompleted disparado");

                Debug.WriteLine("═══════════════════════════════════════════");
                Debug.WriteLine("[CheckoutViewModel] PlaceOrderAsync COMPLETADO");
                Debug.WriteLine("═══════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutViewModel] ERROR CRÍTICO: {ex.Message}");
                Debug.WriteLine($"[CheckoutViewModel] StackTrace: {ex.StackTrace}");
                ErrorMessage = $"Error al crear el pedido: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("[CheckoutViewModel] PlaceOrderAsync finalizado");
            }
        }
        #endregion
    }
}

/*
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

*/

