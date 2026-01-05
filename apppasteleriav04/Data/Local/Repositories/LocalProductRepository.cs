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
    /// Repository for managing local product cache
    /// </summary>
    public class LocalProductRepository : ILocalRepository<LocalProduct>
    {
        private readonly AppDatabase _database;

        public LocalProductRepository()
        {
            _database = AppDatabase.Instance;
        }

        public async Task<List<LocalProduct>> GetAllAsync()
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                return await db.Table<LocalProduct>()
                    .Where(p => p.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error getting all products: {ex}");
                return new List<LocalProduct>();
            }
        }

        public async Task<LocalProduct?> GetByIdAsync(int id)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                return await db.Table<LocalProduct>()
                    .Where(p => p.Id == id.ToString())
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error getting product by id: {ex}");
                return null;
            }
        }

        public async Task<LocalProduct?> GetByRemoteIdAsync(string remoteId)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                return await db.Table<LocalProduct>()
                    .Where(p => p.Id == remoteId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error getting product by remote id: {ex}");
                return null;
            }
        }

        public async Task<int> InsertAsync(LocalProduct entity)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                entity.LastSynced = DateTime.UtcNow;
                return await db.InsertAsync(entity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error inserting product: {ex}");
                return 0;
            }
        }

        public async Task<int> InsertOrReplaceAsync(LocalProduct entity)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                entity.LastSynced = DateTime.UtcNow;
                return await db.InsertOrReplaceAsync(entity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error inserting/replacing product: {ex}");
                return 0;
            }
        }

        public async Task<int> UpdateAsync(LocalProduct entity)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                entity.LastSynced = DateTime.UtcNow;
                return await db.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error updating product: {ex}");
                return 0;
            }
        }

        public async Task<int> DeleteAsync(LocalProduct entity)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                return await db.DeleteAsync(entity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error deleting product: {ex}");
                return 0;
            }
        }

        public async Task<int> DeleteAllAsync()
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                return await db.DeleteAllAsync<LocalProduct>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error deleting all products: {ex}");
                return 0;
            }
        }

        /// <summary>
        /// Gets products by category
        /// </summary>
        public async Task<List<LocalProduct>> GetByCategoryAsync(string category)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                return await db.Table<LocalProduct>()
                    .Where(p => p.Categoria == category && p.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error getting products by category: {ex}");
                return new List<LocalProduct>();
            }
        }

        /// <summary>
        /// Checks if products cache is stale (older than specified hours)
        /// </summary>
        public async Task<bool> IsCacheStaleAsync(int maxAgeHours = 24)
        {
            try
            {
                var db = await _database.GetConnectionAsync();
                var latestProduct = await db.Table<LocalProduct>()
                    .OrderByDescending(p => p.LastSynced)
                    .FirstOrDefaultAsync();

                if (latestProduct == null)
                    return true;

                var age = DateTime.UtcNow - latestProduct.LastSynced;
                return age.TotalHours > maxAgeHours;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalProductRepository] Error checking cache staleness: {ex}");
                return true;
            }
        }
    }
}
