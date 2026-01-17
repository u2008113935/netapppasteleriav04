using System;
using System.Collections.Generic;
using System.Text;
using apppasteleriav04.Models.Local;
using apppasteleriav04.Data.Local.Database;
using SQLite;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;


namespace apppasteleriav04.Data.Local.Repositories
{
    public class LocalCartRepository
    {
        private SQLiteAsyncConnection Database => AppDatabase.Instance.Database;


        public async Task<List<LocalCartItem>> GetCartItemsAsync(Guid? userId = null)
        {
            try
            {
                if (userId.HasValue)
                {
                    var items = await Database.Table<LocalCartItem>()
                        .Where(c => c.UserId == userId.Value)
                        .ToListAsync();

                    Debug.WriteLine($"[LocalCartRepository] {items.Count} items para usuarios {userId}");
                    return items;
                }
                else
                {
                    //Carrito anomino
                    var items = await Database.Table<LocalCartItem>()
                        .Where(c => c.UserId == null)
                        .ToListAsync();

                    Debug.WriteLine($"[LocalCartRepository] {items.Count} items para usuario anónimo");
                    return items;
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalCartRepository] Error GetCartItemsAsync al obtener los items del carrito: {ex.Message}");
                return new List<LocalCartItem>();
            }
        }

        
        public async Task<int> AddToCartAsync (LocalCartItem item)
        {
            try
            {
                var existing = await Database.Table<LocalCartItem>()
                    .Where(c => c.UserId == item.UserId && c.ProductId == item.ProductId)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    existing.Quantity += item.Quantity;
                    existing.IsSynced = false; // Marca como no sincronizado
                    var result = await Database.UpdateAsync(existing);
                    Debug.WriteLine($"[LocalCartRepository] Cantidad actualizad para porducto {item.ProductId}");

                    return result;

                }
                else
                {

                    item.AddedAt = DateTime.UtcNow;
                    item.IsSynced = false; // Marca como no sincronizado

                    var result = await Database.InsertAsync(item);
                    Debug.WriteLine($"[LocalCartRespository] nuevo item agregaado: {item.ProductName}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalCartRespository] Error en AddToCartAsu¿ync: {ex.Message}");
                throw;
            }
        }

        public async Task<int> UpdateQuantityAsync(int cartItemId, int newQuantity)
        {
            try
            {
                var item = await Database.Table<LocalCartItem>()
                    .Where(c => c.Id == cartItemId)
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    Debug.WriteLine($"[LocalCartRepository] Item {cartItemId} no encontrado");
                    return 0;
                }

                if (newQuantity <= 0)
                {
                    var result = await Database.DeleteAsync(item);
                    Debug.WriteLine($"[LocalCartRepository] Item {cartItemId} eliminado (cantidad = 0)");
                    return result;
                }
                else
                {
                    item.Quantity = newQuantity;
                    item.IsSynced = false;

                    var result = await Database.UpdateAsync(item);
                    Debug.WriteLine($"[LocalCartRepository] Cantidad actualizada a {newQuantity}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalCartRepository] Error en UpdateQuantityAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int> RemoveFromCartAsync(int cartItemId)
        {
            try
            {
                var item = await Database.Table<LocalCartItem>()
                    .Where(c => c.Id == cartItemId)
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    Debug.WriteLine($"[LocalCartRepository] Item {cartItemId} no encontrado para eliminar");
                    return 0;
                }

                var result = await Database.DeleteAsync(item);
                Debug.WriteLine($"[LocalCartRepository] Item {cartItemId} eliminado");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalCartRepository] Error en RemoveFromCartAsync: {ex.Message}");
                throw;
            }
        }
           
        public async Task<int> ClearCartAsync(Guid? userId = null)
        {
            try
            {
                var items = await GetCartItemsAsync(userId);
                int count = 0;

                foreach (var item in items)
                {
                    count += await Database.DeleteAsync(item);
                }

                Debug.WriteLine($"[LocalCartRepository] {count} items eliminados del carrito");
                return count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalCartRepository] Error en ClearCartAsync: {ex.Message}");
                throw;
            }
        }
         
        public async Task<int> MigrateAnonymousCartAsync(Guid userId)
        {
            try
            {
                var anonymousItems = await GetCartItemsAsync(null);
                int migrated = 0;

                foreach (var item in anonymousItems)
                {
                    // Verificar si el usuario ya tiene este producto
                    var existing = await Database.Table<LocalCartItem>()
                        .Where(c => c.ProductId == item.ProductId && c.UserId == userId)
                        .FirstOrDefaultAsync();

                    if (existing != null)
                    {
                        // Combinar cantidades
                        existing.Quantity += item.Quantity;
                        existing.IsSynced = false;
                        await Database.UpdateAsync(existing);
                        await Database.DeleteAsync(item);
                        Debug.WriteLine($"[LocalCartRepository] Combinado producto {item.ProductName}");
                    }
                    else
                    {
                        // Asignar al usuario
                        item.UserId = userId;
                        item.IsSynced = false;
                        migrated += await Database.UpdateAsync(item);
                        Debug.WriteLine($"[LocalCartRepository] Migrado producto {item.ProductName}");
                    }
                }

                Debug.WriteLine($"[LocalCartRepository] {migrated} items migrados de carrito anónimo");
                return migrated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalCartRepository] Error en MigrateAnonymousCartAsync:  {ex.Message}");
                throw;
            }
        }
          
        public async Task<decimal> GetCartTotalAsync(Guid? userId = null)
        {
            try
            {
                var items = await GetCartItemsAsync(userId);
                var total = items.Sum(i => i.Price * i.Quantity);
                Debug.WriteLine($"[LocalCartRepository] Total del carrito: {total: C}");
                return total;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalCartRepository] Error en GetCartTotalAsync: {ex.Message}");
                return 0m;
            }
        }
           
        public async Task<int> GetCartCountAsync(Guid? userId = null)
        {
            try
            {
                var items = await GetCartItemsAsync(userId);
                var count = items.Sum(i => i.Quantity);
                Debug.WriteLine($"[LocalCartRepository] {count} items en carrito");
                return count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalCartRepository] Error en GetCartCountAsync: {ex.Message}");
                return 0;
            }
        }
            
        public async Task<List<LocalCartItem>> GetUnsyncedItemsAsync(Guid? userId = null)
        {
            try
            {
                var query = Database.Table<LocalCartItem>().Where(c => !c.IsSynced);

                if (userId.HasValue)
                {
                    var items = await query.Where(c => c.UserId == userId).ToListAsync();
                    return items;
                }
                else
                {
                    var items = await query.Where(c => c.UserId == null).ToListAsync();
                    return items;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalCartRepository] Error en GetUnsyncedItemsAsync: {ex.Message}");
                return new List<LocalCartItem>();
            }
        }
    }    
}