using apppasteleriav04.Data.Local.Database;
using apppasteleriav04.Models.Local;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apppasteleriav04.Data.Local.Repositories
{
    public class LocalOrderRepository : ILocalRepository<LocalOrder>
    {
        private SQLiteAsyncConnection Database => AppDatabase.Instance.Database;

        public async Task<List<LocalOrder>> GetAllAsync()
        {
            return await Database.Table<LocalOrder>()
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

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
