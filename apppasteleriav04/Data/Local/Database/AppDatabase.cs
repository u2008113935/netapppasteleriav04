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
                await _database.CreateTableAsync<SyncQueueItem>(); // Tabla de ítems en la cola de sincronización

                Debug.Write($"[AppDatabase] Base de datos inicializada correctamente.");
            }

            catch (Exception ex)
            {
                Debug.Write($"[AppDatabase] Error al inicializar la base de datos: {ex.Message}");
                throw; // Re-lanzar la excepción para manejo externo
            }

        }

        // Aquí puedes agregar más tablas si es necesario
        public SQLiteAsyncConnection? Database => _database ??
        throw new InvalidOperationException("Database not initialized");

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

    }
}



