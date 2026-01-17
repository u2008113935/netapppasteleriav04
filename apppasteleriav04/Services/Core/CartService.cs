using apppasteleriav04.Models.Domain;
using apppasteleriav04.Models.Local;
using apppasteleriav04.Data.Local.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


namespace apppasteleriav04.Services.Core
{
    // Servicio Singleton para gestionar el carrito con persistencia SQLite
    public class CartService : INotifyPropertyChanged
    {
        public static CartService Instance { get; } = new CartService();

        // Repositorio local para operaciones de carrito SQLite 
        private readonly LocalCartRepository _cartRepository;

        // Usuario actual (null = anónimo)
        private Guid? _currentUserId;

        // Colección observable de items en el carrito
        public ObservableCollection<CartItem> Items { get; } = new ObservableCollection<CartItem>();

        // Eventos de notificación de cambios
        public event PropertyChangedEventHandler? PropertyChanged;

        // Evento cuando el carrito cambia (items agregados, removidos o modificados)
        public event EventHandler? CartChanged;

        // Total del carrito (suma precio * cantidad)
        public decimal Total => Items.Sum(i => i.Price * i.Quantity);

        // Cantidad total de items en el carrito
        public int Count => Items.Sum(i => i.Quantity);
        
        // Método para obtener la cantidad de items
        public int GetItemCount() => Count;

        // Key para almacenamiento local
        //const string CartStorageKey = "local_cart_v1";

        //Constructor privado para implementar el patrón singleton
        //Se ejecuta una sola vez al acceder a Instance
        private CartService()
        {
            _cartRepository = new LocalCartRepository();
            Items.CollectionChanged += Items_CollectionChanged;

            Debug.WriteLine("[CartService] Instancia creada");

            // Cargar carrito al iniciar desde la base de datos SQLite
            _ = LoadFromDatabaseAsync();
        }


        //Manejo de eventos
        #region Event Handlers

        //Metodo para manejar los cambios en la coleccion de items
        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Nuevos items agregados
            if (e.NewItems != null)
            {
                foreach (var ni in e.NewItems.OfType<CartItem>())
                {
                    ni.PropertyChanged += CartItem_PropertyChanged;
                }
            }

            // Items elimiandos 
            if (e.OldItems != null)
            {
                foreach (var oi in e.OldItems.OfType<CartItem>())
                {
                    oi.PropertyChanged -= CartItem_PropertyChanged;
                }              
            }

            // Si hubo un reset (Clear), desuscribir todos
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // en reset no hay OldItems, desuscribimos de todo por seguridad
                // (si los items ya referenciados implementan INotifyPropertyChanged)
                // No tenemos acceso directo a los antiguos, así que solo notificamos
            }
            // Notificar cambios derivables
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Total));

            // Disparar evento de cambio de carrito
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        // Maneja cambios en las propiedades de los items del carrito
        private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Si cambia la cantidad o el precio, recalcular Count/Total
            if (e.PropertyName == nameof(CartItem.Quantity) || e.PropertyName == nameof(CartItem.Price))
            {
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(Total));
                CartChanged?.Invoke(this, EventArgs.Empty);

                // Actualizar cantidad en la base de datos asíncronamente SQLite
                if (sender is CartItem item)
                {
                    _ = UpdateQuantityInDatabaseAsync(item);
                }
            }
        }

        #endregion

        #region Public Methods

        // Metodos publicos como el API del servicio
        // Agrega un producto al carrito (versión asíncrona, es decir en SQLite)
        // Flujo Offline-First:
        // 1. Validar datos
        // 2. Guardar en BD local es decir en SQLite
        // 3. Actualizar UI (Items Collection)

        //Metodo para agregar un producto al carrito de forma asincronica, es decir en SQLite
        public async Task AddAsync(Product product, int qty = 1)
        {
            // Validacion 1: Producto no nulo, cantidad positiva
            if (product == null) throw new ArgumentNullException(nameof(product));

            // Validacion 2: Cantidad minima 1
            if (qty <= 0) qty = 1;

            try
            {
                Debug.WriteLine($"[CartService] Agregando {qty}x {product.Nombre}");

                //Paso 1: Crear el item del carrito local, es decir en SQLite
                // Crear item local
                var localItem = new LocalCartItem
                {
                    UserId = _currentUserId,        // Puede ser null para anónimo
                    ProductId = product.Id,
                    ProductName = product.Nombre,
                    ImagePath = product.ImagenPath ?? string.Empty,
                    Price = product.Precio ?? 0m,
                    Quantity = qty
                };

                // Paso 2: Guardar el item en la base de datos local SQLite
                // Guardar en BD
                await _cartRepository.AddToCartAsync(localItem);

                // Paso 3: Actualizar la colección en memoria UI
                // Actualizar colección en memoria
                var existing = Items.FirstOrDefault(i => i.ProductId == product.Id);

                if (existing != null)
                {
                    existing.Quantity += qty;
                }
                else
                {
                    //No existe entonces crear nuevo item
                    var item = new CartItem
                    {
                        ProductId = product.Id,
                        Nombre = product.Nombre,
                        ImagenPath = product.ImagenPath,
                        Price = product.Precio ?? 0m,
                        Quantity = qty
                    };
                    Items.Add(item);
                }

                Debug.WriteLine($"[CartService] Producto agregado.  Total items: {Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CartService] Error en AddAsync: {ex.Message}");
                throw;
            }
        }

        // Agregar un item al carrito, si existe aumentar
        public void Add(Product product, int qty = 1)
        {
            _ = AddAsync(product, qty);  // Llamar a la versión asíncrona
            /*
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (qty <= 0) qty = 1;

            var existing = Items.FirstOrDefault(i => i.ProductId == product.Id);

            if (existing != null)
            {
                existing.Quantity += qty; // CartItem notificará; CartService escuchará y actualizará Count/Total
            }
            else
            {
                var item = new CartItem
                {
                    ProductId = product.Id,
                    Nombre = product.Nombre,
                    ImagenPath = product.ImagenPath,
                    Price = product.Precio ?? 0m,
                    Quantity = qty
                };
                Items.Add(item);
                // Items.CollectionChanged hará el resto (suscribirá y notificará)
            }
            System.Diagnostics.Debug.WriteLine($"CartService.Add -> Items.Count={Items.Count}");
            */
        }

        /// Elimina un producto del carrito (versión asíncrona)        
        public async Task RemoveAsync(Guid productId)
        {
            try
            {
                Debug.WriteLine($"[CartService] Eliminando producto {productId}");

                var item = Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    // Eliminar de BD
                    var localItems = await _cartRepository.GetCartItemsAsync(_currentUserId);
                    var localItem = localItems.FirstOrDefault(i => i.ProductId == productId);

                    if (localItem != null)
                    {
                        await _cartRepository.RemoveFromCartAsync(localItem.Id);
                    }

                    // Eliminar de colección
                    Items.Remove(item);
                    Debug.WriteLine($"[CartService] Producto eliminado. Total items: {Count}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CartService] Error en RemoveAsync: {ex.Message}");
                throw;
            }
        }

        // Remueve un producto del carrito
        public void Remove(Guid productId)
        {
            _ = RemoveAsync(productId);
            /*
            var existing = Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                Items.Remove(existing);
                // Items.CollectionChanged notificará Count/Total
            }
            */
        }

        public async Task ClearAsync()
        {
            try
            {
                Debug.WriteLine($"[CartService] Limpiando carrito");

                await _cartRepository.ClearCartAsync(_currentUserId);
                Items.Clear();

                Debug.WriteLine($"[CartService] Carrito limpiado.");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CartService] Error en ClearAsync: {ex.Message}");
                throw;
            }
        }

        public void Clear()
        {
            _ = ClearAsync();
        }


        // Carga el carrito desde la base de datos SQLite para el usuario actual (o anónimo)
        public async Task LoadFromDatabaseAsync(Guid? userId = null)
        {            
                try
                {
                    Debug.WriteLine($"[CartService] Cargando carrito para usuario:  {userId?.ToString() ?? "anónimo"}");

                    _currentUserId = userId;
                    var localItems = await _cartRepository.GetCartItemsAsync(userId);

                    Items.Clear();
                    foreach (var local in localItems)
                    {
                        Items.Add(new CartItem
                        {
                            ProductId = local.ProductId,
                            Nombre = local.ProductName,
                            ImagenPath = local.ImagePath,
                            Price = local.Price,
                            Quantity = local.Quantity
                        });
                    }

                    Debug.WriteLine($"[CartService] {Items.Count} items cargados");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CartService] Error en LoadFromDatabaseAsync:  {ex.Message}");
                }
        }

        public async Task MigrateAnonymousCartAsync(Guid userId)
        {
            try
            {
                Debug.WriteLine($"[CartService] Migrando carrito anónimo a usuario {userId}");

                await _cartRepository.MigrateAnonymousCartAsync(userId);
                await LoadFromDatabaseAsync(userId);

                Debug.WriteLine("[CartService] Migración completada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CartService] Error en MigrateAnonymousCartAsync: {ex.Message}");
                throw;
            }
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Actualiza la cantidad de un item en la base de datos
        /// </summary>
        private async Task UpdateQuantityInDatabaseAsync(CartItem item)
        {
            try
            {
                var localItems = await _cartRepository.GetCartItemsAsync(_currentUserId);
                var localItem = localItems.FirstOrDefault(i => i.ProductId == item.ProductId);

                if (localItem != null)
                {
                    await _cartRepository.UpdateQuantityAsync(localItem.Id, item.Quantity);
                    Debug.WriteLine($"[CartService] Cantidad actualizada en BD:  {item.Quantity}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CartService] Error actualizando cantidad en BD: {ex.Message}");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        // Actualizar la cantidad de un producto en el carrito
        /*
        public void UpdateQuantity(Guid productId, int qty)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing == null) return;

            if (qty <= 0)
            {
                Items.Remove(existing);
            }
            else
            {
                existing.Quantity = qty; // CartItem.PropertyChanged -> CartService reagirá
            }
        }
        */

        public OrderItem[] ToOrderItems()
        {
            return Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.Empty,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToArray();
        }

        public bool ContainsProduct(Guid productId) => Items.Any(i => i.ProductId == productId);

        

        // -------------------------
        // Persistencia local (Preferences)
        // -------------------------

        // Guarda el carrito en Preferences (serializado JSON)
        /*
        public async Task SaveLocalAsync()
        {
            try
            {
                var arr = Items.ToArray();
                var json = JsonSerializer.Serialize(arr);
                Preferences.Set(CartStorageKey, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CartService.SaveLocalAsync error: {ex.Message}");
            }
            await Task.CompletedTask;
        }
        */

        // Carga el carrito desde Preferences (reemplaza el contenido actual)
        /*
        public async Task LoadLocalAsync()
        {
            try
            {
                var json = Preferences.Get(CartStorageKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var items = JsonSerializer.Deserialize<CartItem[]>(json);
                    if (items != null)
                    {
                        Items.Clear();
                        foreach (var it in items) Items.Add(it);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CartService.LoadLocalAsync error: {ex.Message}");
            }
            await Task.CompletedTask;
        }
        */

        // Merge: suma cantidades si producto existe, o añade nuevo.
        /*
        public void MergeFrom(CartItem[] other)
        {
            if (other == null || other.Length == 0) return;
            foreach (var it in other)
            {
                var existing = Items.FirstOrDefault(i => i.ProductId == it.ProductId);
                if (existing != null)
                    existing.Quantity += it.Quantity;
                else
                    Items.Add(it);
            }

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Total));

            // Opcional: guardar después de merge
            _ = SaveLocalAsync();

            // Notificar cambio de carrito
            CartChanged?.Invoke(this, EventArgs.Empty);
        }
        */
    }
}