using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using apppasteleriav04.Data.Local.Database;
using apppasteleriav04.Models.Local;

namespace apppasteleriav04.Data.Local.Repositories
{
    /// <summary>
    /// Repository for managing local orders and order items
    /// </summary>
    public class LocalOrderRepository : ILocalRepository<LocalOrder>
    {
        private readonly AppDatabase _database;

        public LocalOrderRepository()
        {
            _database = AppDatabase.Instance;
        }

        public async Task<List<LocalOrder>> GetAllAsync()
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                var orders = await db.Table<LocalOrder>()
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                // Load items for each order
                foreach (var order in orders)
                {
                    order.Items = await GetOrderItemsAsync(order.Id);
                }

                return orders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error getting all orders: {ex}");
                return new List<LocalOrder>();
            }
        }

        public async Task<LocalOrder?> GetByIdAsync(int id)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                var order = await db.Table<LocalOrder>()
                    .Where(o => o.Id == id)
                    .FirstOrDefaultAsync();

                if (order != null)
                {
                    order.Items = await GetOrderItemsAsync(order.Id);
                }

                return order;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error getting order by id: {ex}");
                return null;
            }
        }

        public async Task<int> InsertAsync(LocalOrder entity)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                var result = await db.InsertAsync(entity);

                // Insert order items
                if (entity.Items != null && entity.Items.Count > 0)
                {
                    foreach (var item in entity.Items)
                    {
                        item.LocalOrderId = entity.Id;
                        await db.InsertAsync(item);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error inserting order: {ex}");
                return 0;
            }
        }

        public async Task<int> UpdateAsync(LocalOrder entity)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                return await db.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error updating order: {ex}");
                return 0;
            }
        }

        public async Task<int> DeleteAsync(LocalOrder entity)
        {
            try
            {
                var db = await _database.GetConnectionAsync();

                // Delete order items first
                await db.ExecuteAsync("DELETE FROM order_items WHERE local_order_id = ?", entity.Id);

                return await db.DeleteAsync(entity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error deleting order: {ex}");
                return 0;
            }
        }

        public async Task<int> DeleteAllAsync()
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                await db.DeleteAllAsync<LocalOrderItem>();
                return await db.DeleteAllAsync<LocalOrder>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error deleting all orders: {ex}");
                return 0;
            }
        }

        /// <summary>
        /// Gets all unsynced orders that need to be uploaded to the server
        /// </summary>
        public async Task<List<LocalOrder>> GetUnsyncedOrdersAsync()
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                var orders = await db.Table<LocalOrder>()
                    .Where(o => !o.Synced)
                    .OrderBy(o => o.CreatedAt)
                    .ToListAsync();

                // Load items for each order
                foreach (var order in orders)
                {
                    order.Items = await GetOrderItemsAsync(order.Id);
                }

                return orders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error getting unsynced orders: {ex}");
                return new List<LocalOrder>();
            }
        }

        /// <summary>
        /// Gets orders for a specific user
        /// </summary>
        public async Task<List<LocalOrder>> GetByUserIdAsync(string userId)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                var orders = await db.Table<LocalOrder>()
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                // Load items for each order
                foreach (var order in orders)
                {
                    order.Items = await GetOrderItemsAsync(order.Id);
                }

                return orders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error getting orders by user id: {ex}");
                return new List<LocalOrder>();
            }
        }

        /// <summary>
        /// Marks an order as synced
        /// </summary>
        public async Task<bool> MarkAsSyncedAsync(int localOrderId, string remoteOrderId)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                var order = await GetByIdAsync(localOrderId);

                if (order == null)
                    return false;

                order.Synced = true;
                order.SyncedAt = DateTime.UtcNow;
                order.RemoteId = remoteOrderId;

                await UpdateAsync(order);

                // Mark order items as synced
                await db.ExecuteAsync(
                    "UPDATE order_items SET synced = 1, remote_order_id = ? WHERE local_order_id = ?",
                    remoteOrderId, localOrderId);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error marking order as synced: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Gets order items for a specific order
        /// </summary>
        private async Task<List<LocalOrderItem>> GetOrderItemsAsync(int localOrderId)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                return await db.Table<LocalOrderItem>()
                    .Where(i => i.LocalOrderId == localOrderId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalOrderRepository] Error getting order items: {ex}");
                return new List<LocalOrderItem>();
            }
        }
    }
}
