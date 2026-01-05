namespace apppasteleriav04.Data.Local.Repositories
{
    public interface ILocalRepository<T> where T : class, new()
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task<int> SaveAsync(T item);
        Task<int> DeleteAsync(T item);
        Task<int> DeleteAllAsync();
    }
}
