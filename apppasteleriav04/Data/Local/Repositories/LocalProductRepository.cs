using apppasteleriav04.Data.Local.Database;
using apppasteleriav04.Models.Local;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apppasteleriav04.Data.Local.Repositories
{
    public class LocalProductRepository : ILocalRepository<LocalProduct>
    {
        private SQLiteAsyncConnection Database => AppDatabase.Instance.Database;

        public async Task<List<LocalProduct>> GetAllAsync()
        {
            return await Database.Table<LocalProduct>().ToListAsync();
        }

        public async Task<LocalProduct?> GetByIdAsync(Guid id)
        {
            return await Database.Table<LocalProduct>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveAsync(LocalProduct item)
        {
            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();

            var existing = await GetByIdAsync(item.Id);
            if (existing != null)
                return await Database.UpdateAsync(item);
            else
                return await Database.InsertAsync(item);
        }

        public async Task<int> DeleteAsync(LocalProduct item)
        {
            return await Database.DeleteAsync(item);
        }

        public async Task<int> DeleteAllAsync()
        {
            return await Database.DeleteAllAsync<LocalProduct>();
        }

        public async Task<List<LocalProduct>> GetUnsyncedAsync()
        {
            return await Database.Table<LocalProduct>()
                .Where(p => !p.IsSynced)
                .ToListAsync();
        }

        public async Task<int> MarkAsSyncedAsync(Guid id)
        {
            var product = await GetByIdAsync(id);
            if (product == null) return 0;

            product.IsSynced = true;
            product.LastSyncedAt = DateTime.UtcNow;
            return await Database.UpdateAsync(product);
        }

        public async Task<bool> IsCacheStaleAsync(TimeSpan maxAge)
        {
            var products = await GetAllAsync();
            if (products.Count == 0) return true;

            var oldestSync = products.Min(p => p.LastSyncedAt);
            return (DateTime.UtcNow - oldestSync) > maxAge;
        }
    }
}
