using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using apppasteleriav04.Models.Local;
using SQLite;

namespace apppasteleriav04.Data.Local.Database
{
    /// <summary>
    /// SQLite database context for local offline storage
    /// </summary>
    public class AppDatabase
    {
        private static AppDatabase? _instance;
        private static readonly object _lock = new object();

        private SQLiteAsyncConnection? _database;

        /// <summary>
        /// Singleton instance of the database
        /// </summary>
        public static AppDatabase Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AppDatabase();
                    }
                    return _instance;
                }
            }
        }

        private AppDatabase()
        {
            // Private constructor for singleton
        }

        /// <summary>
        /// Initializes the database connection and creates tables
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                if (_database != null)
                    return;

                Debug.WriteLine($"[AppDatabase] Initializing database at: {DatabaseConstants.DatabasePath}");

                _database = new SQLiteAsyncConnection(
                    DatabaseConstants.DatabasePath,
                    DatabaseConstants.Flags);

                // Create tables
                await _database.CreateTableAsync<LocalProduct>();
                await _database.CreateTableAsync<LocalOrder>();
                await _database.CreateTableAsync<LocalOrderItem>();
                await _database.CreateTableAsync<SyncQueue>();
                await _database.CreateTableAsync<LocalPayment>();
                await _database.CreateTableAsync<LocalTransaction>();

                Debug.WriteLine("[AppDatabase] Database initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppDatabase] Error initializing database: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Gets the database connection, initializing it if necessary
        /// </summary>
        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_database == null)
            {
                await InitializeAsync();
            }

            return _database!;
        }

        /// <summary>
        /// Closes the database connection
        /// </summary>
        public async Task CloseAsync()
        {
            if (_database != null)
            {
                await _database.CloseAsync();
                _database = null;
                Debug.WriteLine("[AppDatabase] Database connection closed");
            }
        }

        /// <summary>
        /// Clears all data from the database (for testing or logout)
        /// </summary>
        public async Task ClearAllDataAsync()
        {
            try
            {
                var db = await GetConnectionAsync();

                await db.DeleteAllAsync<LocalProduct>();
                await db.DeleteAllAsync<LocalOrder>();
                await db.DeleteAllAsync<LocalOrderItem>();
                await db.DeleteAllAsync<SyncQueue>();
                await db.DeleteAllAsync<LocalPayment>();
                await db.DeleteAllAsync<LocalTransaction>();

                Debug.WriteLine("[AppDatabase] All data cleared from database");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppDatabase] Error clearing data: {ex}");
                throw;
            }
        }
    }
}
