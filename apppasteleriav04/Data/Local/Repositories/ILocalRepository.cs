using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apppasteleriav04.Data.Local.Repositories
{
    /// <summary>
    /// Base interface for local repositories
    /// </summary>
    public interface ILocalRepository<T> where T : class, new()
    {
        /// <summary>
        /// Gets all entities from the local database
        /// </summary>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Inserts a new entity into the local database
        /// </summary>
        Task<int> InsertAsync(T entity);

        /// <summary>
        /// Updates an existing entity in the local database
        /// </summary>
        Task<int> UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity from the local database
        /// </summary>
        Task<int> DeleteAsync(T entity);

        /// <summary>
        /// Deletes all entities from the local database
        /// </summary>
        Task<int> DeleteAllAsync();
    }
}
