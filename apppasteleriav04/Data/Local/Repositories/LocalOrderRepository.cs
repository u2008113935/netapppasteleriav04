using apppasteleriav04.Data.Local.Database;
using apppasteleriav04.Models.Local;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace apppasteleriav04.Data.Local.Repositories
{
    // Repositorio para manejar las órdenes locales
    public class LocalOrderRepository : ILocalRepository<LocalOrder>
    {
        private SQLiteAsyncConnection Database => AppDatabase.Instance.Database; // Acceso a la conexión de la base de datos
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Semáforo para sincronización 

        #region Metodos CRUD basicos

        // Obtiene todas las órdenes locales         
        public async Task<List<LocalOrder>> GetAllAsync()
        {
            try
            {
                // Esperar para entrar en la sección crítica
                var orders =  await Database.Table <LocalOrder>() 
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                Debug.WriteLine($"[LocalOrderRepository] {orders.Count} pedidos encontrados");
                return orders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error en GetAllAsync: {ex.Message}");
                throw;
            }
            
        }

        // Obtiene una orden local por su ID
        public async Task<LocalOrder?> GetByIdAsync(Guid id)
        {
            return await Database.Table<LocalOrder>()
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveAsync(LocalOrder item)
        {
            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();

            var existing = await GetByIdAsync(item.Id);
            if (existing != null)
                return await Database.UpdateAsync(item);
            else
                return await Database.InsertAsync(item);
        }

        public async Task<int> DeleteAsync(LocalOrder item)
        {
            return await Database.DeleteAsync(item);
        }

        public async Task<int> DeleteAllAsync()
        {
            return await Database.DeleteAllAsync<LocalOrder>();
        }

        public async Task<List<LocalOrder>> GetUnsyncedOrdersAsync()
        {
            return await Database.Table<LocalOrder>()
                .Where(o => !o.IsSynced)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> SaveWithItemsAsync(LocalOrder order, List<LocalOrderItem> items)
        {
            if (order.Id == Guid.Empty)
                order.Id = Guid.NewGuid();

            // Save order
            var result = await SaveAsync(order);

            // Save items
            foreach (var item in items)
            {
                item.OrderId = order.Id;
                await Database.InsertAsync(item);
            }

            return result;
        }

        public async Task<List<LocalOrderItem>> GetOrderItemsAsync(Guid orderId)
        {
            return await Database.Table<LocalOrderItem>()
                .Where(i => i.OrderId == orderId)
                .ToListAsync();
        }
    }
}
