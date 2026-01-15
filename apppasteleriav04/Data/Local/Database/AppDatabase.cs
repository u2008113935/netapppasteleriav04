using System;
using SQLite;
using apppasteleriav04.Models.Local;
using System.Diagnostics;
using System.Threading.Tasks;


namespace apppasteleriav04.Data.Local.Database
{

    // Clase para manejar la base de datos local
    public class AppDatabase
    {
        private static AppDatabase? _instance;
        private SQLiteAsyncConnection? _database;

        // Constructor privado para implementar el patrón singleton
        public static AppDatabase Instance => _instance ??= new AppDatabase();

        //
        private AppDatabase() { }

        // Inicializa la base de datos local
        public async Task InitializeAsync()
        {
            if (_database != null) // Ya inicializado
            {
                Debug.Write("[AppDatabase] La base de datos ya está inicializada.");
                return; // Evitar re-inicialización
            }
            try
            {
                Debug.Write($"[AppDatabase] Inicializando ... Ruta: {DatabaseConstants.DatabasePath}");

                // Crear la conexión a la base de datos SQLite
                _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);

                // Crear las tablas necesarias
                await _database.CreateTableAsync<LocalProduct>(); // Tabla de productos locales
                await _database.CreateTableAsync<LocalOrder>(); // Tabla de órdenes locales
                await _database.CreateTableAsync<LocalOrderItem>(); // Tabla de ítems de órdenes locales
                await _database.CreateTableAsync<LocalCartItem>(); // Tabla de ítems del carrito local                
                await _database.CreateTableAsync<SyncQueueItem>(); // Tabla de ítems en la cola de sincronización

                Debug.Write($"[AppDatabase] Base de datos inicializada correctamente.");

                // Log de tablas creadas
                var tables = await _database.QueryAsync<sqlite_master>(
                    "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name"
                );

                Debug.WriteLine($"[AppDatabase] Tablas existentes: {string.Join(", ", tables.Select(t => t.name))}");

            }

            catch (Exception ex)
            {
                Debug.WriteLine($"[AppDatabase] Error al inicializar la base de datos: {ex.Message}");
                Debug.WriteLine($"[AppDatabase] StackTrace: {ex.StackTrace}");
                throw;
            }

        }

        // Aquí puedes agregar más tablas si es necesario
        public SQLiteAsyncConnection? Database => _database ??
               throw new InvalidOperationException("Database not initialized. Call InitializeAsync() first.");

        // Cierra la conexión a la base de datos
        public async Task CloseAsync()
        {
            if (_database != null)
            {
                await _database.CloseAsync();
                _database = null;
                Debug.Write("[AppDatabase] Conexion cerrada");
            }
        }

        /// Resetea la base de datos (útil para testing)
        
        public async Task ResetDatabaseAsync()
        {
            if (_database == null) return;

            try
            {
                Debug.WriteLine("[AppDatabase] Reseteando base de datos...");

                await _database.DropTableAsync<LocalProduct>();
                await _database.DropTableAsync<LocalOrder>();
                await _database.DropTableAsync<LocalOrderItem>();
                await _database.DropTableAsync<LocalCartItem>();
                await _database.DropTableAsync<SyncQueueItem>();

                await InitializeAsync();

                Debug.WriteLine("[AppDatabase] Base de datos reseteada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppDatabase] Error reseteando BD: {ex.Message}");
                throw;
            }

        }
    }

    // Clase helper para query de metadatos de SQLite
    public class sqlite_master
    {
        public string? name { get; set; }
    }

}




