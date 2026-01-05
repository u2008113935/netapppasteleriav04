using SQLite;
using apppasteleriav04.Models.Local;

namespace apppasteleriav04.Data.Local.Database
{
    public class AppDatabase
    {
        private static AppDatabase? _instance;
        private SQLiteAsyncConnection? _database;
        
        public static AppDatabase Instance => _instance ??= new AppDatabase();
        
        private AppDatabase() { }
        
        public async Task InitializeAsync()
        {
            if (_database != null) return;
            
            _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
            await _database.CreateTableAsync<LocalProduct>();
            await _database.CreateTableAsync<LocalOrder>();
            await _database.CreateTableAsync<LocalOrderItem>();
            await _database.CreateTableAsync<SyncQueueItem>();
        }
        
        public SQLiteAsyncConnection Database => _database 
            ?? throw new InvalidOperationException("Database not initialized");
    }
}
