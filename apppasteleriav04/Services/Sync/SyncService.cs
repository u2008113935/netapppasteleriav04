using apppasteleriav04.Data.Local.Database;
using apppasteleriav04.Data.Local.Repositories;
using apppasteleriav04.Models.Local;
using apppasteleriav04.Services.Connectivity;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Models.Domain;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace apppasteleriav04.Services.Sync
{

    //Servicio de sincronización offline-to-online

    //Evento para progreso de sincronización
    public class SyncService : ISyncService
    {
        //Campos privados, son los datos internos del servicio

        // Servicio de conectividad para detectar cambios en la conexión
        private readonly IConnectivityService _connectivityService;

        // Acceso a la base de datos local Sqlite
        private SQLiteAsyncConnection Database => AppDatabase.Instance.Database;

        // Estado de sincronización
        private bool _isSyncing = false;

        // Conteo de pedidos pendientes de sincronización
        private int _pendingSyncCount = 0;

        //Evento para notificar progreso de sincronización
        public event EventHandler<SyncProgressEventArgs>? SyncProgress;

        //Propiedades públicas
        public int PendingSyncCount => _pendingSyncCount;
        public bool IsSyncing => _isSyncing;

        //==============================================================
        //CONSTRUCTOR
        //==============================================================

        public SyncService(IConnectivityService connectivityService)
        {
            // Suscripción a eventos de conectividad

            //Guardar referencia al servicio de conectividad
            _connectivityService = connectivityService;

            // Suscripción al evento de cambio de conectividad
            //Cada vez que cambie la conectividad, se llamará a OnConnectivityChanged
            _connectivityService.ConnectivityChanged += OnConnectivityChanged;

            // Actualizar conteo inicial de pendientes
            Task.Run(async () => await UpdatePendingCountAsync());
        }

        //=============================================================================
        //Manejo de eventos de estos metodos privados
        //=============================================================================
        private async void OnConnectivityChanged(object? sender, bool isConnected)
        {
            Debug.WriteLine($"[SyncService] Connectividad cambio: {isConnected}");
            if (isConnected)
            {
                Debug.WriteLine("[SyncService] Iniciando auto-sync...");
                await SyncPendingAsync();
            }
        }

        //Actualiza el conteo de items pendientes de sincronización
        private async Task UpdatePendingCountAsync()
        {
            //Contar items en la tabla SyncQueueItem
            try
            {
                //Actualizar el conteo de pendientes
                _pendingSyncCount = await Database.Table<SyncQueueItem>().CountAsync();
                Debug.WriteLine($"[SyncService] Items pendientes: {_pendingSyncCount}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error actualizando contador: {ex.Message}");
                _pendingSyncCount = 0;
            }
        }


        //=============================================================================
        // Metodos publicos consumo de API y sincronización a Supabase
        //=============================================================================

        //Encola un item para sincronización
        public async Task EnqueueAsync(string entityType, Guid entityId, string action, string payloadJson)
        {
            //Crear nuevo item de sincronización
            var item = new SyncQueueItem
            {
                EntityType = entityType,        // "order", "product", etc.
                EntityId = entityId,            // ID del item a sincronizar
                Action = action,                 // "create", "update", "delete"
                PayloadJson = payloadJson,      // Datos serializados en JSON
                CreatedAt = DateTime.UtcNow,     // Fecha de creación
                Priority = 0,
                RetryCount = 0
            };

            //Insertar en la base de datos local, es decir guardar el SQLite
            await Database.InsertAsync(item);

            //Actualizar el conteo de pendientes
            await UpdatePendingCountAsync();

            Debug.WriteLine($"[SyncService] Encolado {action} {entityType} {entityId}");
        }

        //Sincroniza los items pendientes
        public async Task SyncPendingAsync()
        {

            //Paso 1: Validaciones iniciales

            //Evitar sincronizaciones concurrentes
            if (_isSyncing)
            {
                Debug.WriteLine("[SyncService] Ya sincronizando, omitiendo...");
                return;
            }

            // Verificar conectividad
            if (!_connectivityService.IsConnected)
            {
                Debug.WriteLine("[SyncService] Sin conexion, omitiendo sync...");
                return;
            }

            _isSyncing = true;

            try
            {
                //Paso 2: Obtener items pendientes de sincronización

                // Obtener todos los items encolados ordenados por prioridad y fecha
                var pendingItems = await Database.Table<SyncQueueItem>()
                    .OrderBy(i => i.Priority)
                    .ThenBy(i => i.CreatedAt)
                    .ToListAsync();

                Debug.WriteLine($"[SyncService] {pendingItems.Count} items pendientes");

                //Si no hay nada que sincronizar, salir
                if (pendingItems.Count == 0)
                {
                    return;
                }

                //Contadores para progreso
                var totalItems = pendingItems.Count;
                var processedItems = 0;
                var failedItems = 0;


                // Paso 3: Procesar cada item pendiente

                //Iterar sobre cada item pendiente
                foreach (var item in pendingItems)
                {
                    try
                    {
                        //Notificar progreso antes de procesar el item
                        RaiseSyncProgress(totalItems, processedItems, failedItems, item.EntityType, false, null);

                        //Intentar sincornizar el item
                        var success = await ProcessSyncItemAsync(item);

                        if (success)
                        {
                            await Database.DeleteAsync(item);
                            processedItems++;
                            Debug.WriteLine($"[SyncService] Sincronizado {item.EntityType} {item.EntityId}");
                        }
                        else
                        {
                            // Fallo: Incrementar contador de reintentos                            
                            item.RetryCount++;
                            item.LastAttemptAt = DateTime.UtcNow;

                            // Maximo 5 reintentos
                            if (item.RetryCount >= 5)
                            {
                                item.LastError = "Max retries exceeded";
                                await Database.DeleteAsync(item);
                                failedItems++;
                                Debug.WriteLine($"[SyncService] Max reintentos {item.EntityType}");
                            }
                            else
                            {
                                // Guardar el item actualizado para reintento futuro
                                await Database.UpdateAsync(item);
                                failedItems++;
                                Debug.WriteLine($"[SyncService] Reintento {item.RetryCount}/5");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SyncService] Error procesando items: {ex.Message}");
                        item.RetryCount++;
                        item.LastAttemptAt = DateTime.UtcNow;
                        item.LastError = ex.Message;
                        await Database.UpdateAsync(item);
                        failedItems++;
                    }
                }

                //Paso 4: Notificar finalización de sincronización

                RaiseSyncProgress(totalItems, processedItems, failedItems, null, true, null);
                Debug.WriteLine($"[SyncService] Procesados: {processedItems}, Fallos: {failedItems}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error crítico: {ex}");
                RaiseSyncProgress(0, 0, 0, null, true, ex.Message);
            }
            finally
            {
                _isSyncing = false;
                await UpdatePendingCountAsync();
            }
        }

        //Procesa un item individual de sincronización
        private async Task<bool> ProcessSyncItemAsync(SyncQueueItem item)
        {
            Debug.WriteLine($"[SyncService] Procesando {item.EntityType} - {item.Action}");

            try
            {
                // Determinar tipo de entidad
                switch (item.EntityType.ToLower())
                {
                    case "order":
                        return await SyncOrderAsync(item);

                    case "product":
                        return await SyncProductAsync(item);

                    default:
                        Debug.WriteLine($"[SyncService] EntityType desconocido: {item.EntityType}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error en ProcessSyncItemAsync: {ex.Message}");
                return false;
            }
        }

        /// Sincroniza una orden con el backend
        /// FLUJO:  Deserializar → Enviar a Supabase → Actualizar LocalOrder        
        private async Task<bool> SyncOrderAsync(SyncQueueItem item)
        {
            try
            {
                Debug.WriteLine($"[SyncService] Sincronizando orden {item.EntityId}");

                // ═════════════════════════════════════════
                // PASO 1: Deserializar payload correctamente
                // ═════════════════════════════════════════
                // El payload tiene estructura: { "order": {... }, "items": [...] }
                var payloadDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(item.PayloadJson);

                if (payloadDict == null)
                {
                    Debug.WriteLine("[SyncService] Payload null");
                    return false;
                }

                // Extraer "order" del JSON
                var localOrder = JsonSerializer.Deserialize<LocalOrder>(
                    payloadDict["order"].GetRawText()
                );

                // Extraer "items" del JSON
                var localItems = JsonSerializer.Deserialize<List<LocalOrderItem>>(
                    payloadDict["items"].GetRawText()
                );

                if (localOrder == null || localItems == null)
                {
                    Debug.WriteLine("[SyncService] Error deserializando");
                    return false;
                }

                Debug.WriteLine($"[SyncService] Orden:  {localOrder.Id}, Total: {localOrder.Total:C}");
                Debug.WriteLine($"[SyncService] Items: {localItems.Count}");

                // ═════════════════════════════════════════
                // PASO 2: Convertir a formato Supabase
                // ═════════════════════════════════════════
                var orderItems = localItems.Select(li => new OrderItem
                {
                    ProductId = li.ProductId,
                    Quantity = li.Quantity,
                    Price = li.UnitPrice
                }).ToList();

                // ═════════════════════════════════════════
                // PASO 3: Enviar a Supabase
                // ═════════════════════════════════════════
                var createdOrder = await SupabaseService.Instance.CreateOrderAsync(
                    localOrder.UserId,
                    orderItems
                );

                if (createdOrder != null)
                {
                    Debug.WriteLine($"[SyncService] Orden creada en Supabase:  {createdOrder.Id}");

                    // ═════════════════════════════════════════
                    // PASO 4: Actualizar LocalOrder
                    // ═════════════════════════════════════════
                    var orderRepo = new LocalOrderRepository();
                    localOrder.IsSynced = true;
                    localOrder.SyncedAt = DateTime.UtcNow;
                    await orderRepo.SaveAsync(localOrder);

                    Debug.WriteLine("[SyncService] LocalOrder actualizada");
                    return true;
                }
                else
                {
                    Debug.WriteLine("[SyncService] CreateOrderAsync retornó null");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] Error en SyncOrderAsync: {ex.Message}");
                Debug.WriteLine($"[SyncService] StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// Sincroniza un producto (placeholder para futuras features)        
        private async Task<bool> SyncProductAsync(SyncQueueItem item)
        {
            await Task.CompletedTask;
            Debug.WriteLine("[SyncService] Sincronización de productos no implementada");
            return true;
        }

        //Dispara el evento de progreso de sincronización
        private void RaiseSyncProgress(int total, int processed, int failed, string? currentEntity, bool isComplete, string? errorMessage)
        {
            SyncProgress?.Invoke(this, new SyncProgressEventArgs
            {
                TotalItems = total,
                ProcessedItems = processed,
                FailedItems = failed,
                CurrentEntity = currentEntity,
                IsComplete = isComplete,
                ErrorMessage = errorMessage
            });
        }
    }

}
