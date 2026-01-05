# Offline/Online Synchronization System

## Overview

This implementation provides a complete offline/online synchronization system for the pastelería application, allowing users to:

- View the product catalog without an internet connection (cached locally)
- Create orders offline (synced automatically when connection is restored)
- Automatic synchronization when connectivity is restored

## Architecture

### Components

#### 1. Local Models (`Models/Local/`)

- **LocalProduct.cs**: SQLite entity for caching products
  - Stores product information locally
  - Tracks last sync timestamp
  - Supports active/inactive flag

- **LocalOrder.cs**: SQLite entity for orders created offline
  - Stores order header with sync status
  - Tracks remote ID after successful sync
  - Contains navigation property for order items

- **LocalOrderItem.cs**: SQLite entity for order line items
  - Links to local order
  - Stores product details and quantities
  - Tracks sync status

- **SyncQueue.cs**: Queue for tracking pending sync operations
  - Entity type (Order, Transaction, Payment)
  - Operation type (INSERT, UPDATE, DELETE)
  - JSON payload for sync
  - Retry count and error tracking
  - Priority levels

#### 2. Database Infrastructure (`Data/Local/Database/`)

- **DatabaseConstants.cs**: Configuration constants
  - Database filename: `pasteleria.db3`
  - Connection flags
  - Database path (LocalApplicationData folder)

- **AppDatabase.cs**: SQLite database context
  - Singleton pattern
  - Automatic table creation
  - Connection management
  - Utility methods for clearing data

#### 3. Repositories (`Data/Local/Repositories/`)

- **ILocalRepository<T>**: Generic repository interface
  - Standard CRUD operations
  - Async/await pattern

- **LocalProductRepository**: Product cache management
  - Get all products
  - Get by category
  - Check cache staleness (24-hour default)
  - Insert or replace products

- **LocalOrderRepository**: Order management
  - Get all orders with items
  - Get unsynced orders
  - Mark orders as synced
  - Cascade operations for order items

#### 4. Connectivity Services (`Services/Connectivity/`)

- **IConnectivityService**: Interface for connectivity monitoring
  - IsConnected property
  - ConnectivityChanged event
  - Start/Stop monitoring

- **ConnectivityService**: Implementation using MAUI Essentials
  - Uses `Microsoft.Maui.Networking.Connectivity`
  - Fires events when connectivity changes
  - Thread-safe monitoring

#### 5. Sync Services (`Services/Sync/`)

- **ISyncService**: Interface for synchronization
  - SyncPendingAsync(): Process sync queue
  - SyncProductsAsync(): Download products from server
  - EnqueueOrderAsync(): Add order to sync queue
  - StartAutoSync()/StopAutoSync(): Auto-sync on connectivity restore
  - PendingSyncCount property
  - SyncStatusChanged event

- **SyncService**: Full implementation
  - Processes sync queue in priority order
  - Handles retry logic with error tracking
  - Automatic sync on connectivity restore
  - Converts local entities to remote format
  - Updates sync status

#### 6. SupabaseService Extensions

Added offline support methods:

- **GetProductsWithOfflineSupportAsync()**
  - Tries to fetch from server if online
  - Caches products locally
  - Falls back to local cache if offline or error

- **CreateOrderWithOfflineSupportAsync()**
  - Creates order on server if online
  - Saves order locally if offline
  - Automatically enqueues for sync

#### 7. BaseViewModel Updates

Enhanced with full INotifyPropertyChanged implementation:

- **SetProperty<T>()**: Helper for property changes
- **IsBusy**: Track operation state
- **Title**: Page/view title
- **ErrorMessage**: Display errors to user
- **RelayCommand**: Synchronous command implementation
- **AsyncRelayCommand**: Async command with busy state management

## Usage

### 1. Initialize Database

The database is automatically initialized on app startup in `MauiProgram.cs`:

```csharp
var database = app.Services.GetRequiredService<AppDatabase>();
await database.InitializeAsync();
```

### 2. Start Auto-Sync

Auto-sync is started automatically on app startup:

```csharp
var syncService = app.Services.GetRequiredService<ISyncService>();
syncService.StartAutoSync();
```

### 3. Get Products with Offline Support

```csharp
var products = await SupabaseService.Instance.GetProductsWithOfflineSupportAsync();
// Returns cached products if offline, fresh data if online
```

### 4. Create Order with Offline Support

```csharp
var order = await SupabaseService.Instance.CreateOrderWithOfflineSupportAsync(
    userId, 
    orderItems);
// Creates on server if online, queues for sync if offline
```

### 5. Manual Sync

```csharp
var syncService = app.Services.GetRequiredService<ISyncService>();
var result = await syncService.SyncPendingAsync();

Console.WriteLine($"Synced: {result.ItemsSynced}, Failed: {result.ItemsFailed}");
```

### 6. Monitor Sync Status

```csharp
syncService.SyncStatusChanged += (sender, e) =>
{
    Console.WriteLine($"Pending: {e.PendingCount}, Syncing: {e.IsSyncing}");
    if (e.Message != null)
        Console.WriteLine($"Message: {e.Message}");
};
```

## Synchronization Flow

### Online Order Creation

1. User creates order
2. `CreateOrderWithOfflineSupportAsync()` detects online status
3. Order created directly on Supabase
4. Returns Order object immediately

### Offline Order Creation

1. User creates order
2. `CreateOrderWithOfflineSupportAsync()` detects offline status
3. Order saved to `LocalOrder` table
4. SyncQueue item created with priority
5. Returns simulated Order object
6. When connectivity restored:
   - ConnectivityService fires ConnectivityChanged event
   - SyncService processes queue
   - Order uploaded to Supabase
   - Local order marked as synced with remote ID

### Product Caching

1. First time online:
   - Products fetched from Supabase
   - Cached in `LocalProduct` table
   - Displayed to user

2. Subsequent loads:
   - Check if cache is stale (>24 hours)
   - If fresh and offline: use cache
   - If online: fetch fresh data and update cache
   - If offline and no cache: show empty state

## Database Schema

### products
```sql
CREATE TABLE products (
    id TEXT PRIMARY KEY,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    categoria TEXT,
    imagen_url TEXT,
    precio REAL NOT NULL,
    last_synced DATETIME NOT NULL,
    is_active INTEGER NOT NULL DEFAULT 1
);
```

### orders
```sql
CREATE TABLE orders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    remote_id TEXT,
    user_id TEXT NOT NULL,
    total REAL NOT NULL,
    status TEXT NOT NULL DEFAULT 'pendiente',
    created_at DATETIME NOT NULL,
    synced INTEGER NOT NULL DEFAULT 0,
    synced_at DATETIME,
    repartidor_asignado TEXT,
    latitud_actual REAL,
    longitud_actual REAL,
    hora_est_llegada DATETIME,
    entregado_en DATETIME
);
```

### order_items
```sql
CREATE TABLE order_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    local_order_id INTEGER NOT NULL,
    remote_order_id TEXT,
    product_id TEXT NOT NULL,
    product_name TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    price REAL NOT NULL,
    synced INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY(local_order_id) REFERENCES orders(id)
);
CREATE INDEX idx_order_items_local_order_id ON order_items(local_order_id);
```

### sync_queue
```sql
CREATE TABLE sync_queue (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    entity_type TEXT NOT NULL,
    local_entity_id INTEGER NOT NULL,
    operation TEXT NOT NULL DEFAULT 'INSERT',
    json_data TEXT,
    is_synced INTEGER NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL,
    synced_at DATETIME,
    retry_count INTEGER NOT NULL DEFAULT 0,
    error_message TEXT,
    priority INTEGER NOT NULL DEFAULT 0
);
```

## Error Handling

The system includes comprehensive error handling:

- **Network errors**: Automatically fall back to offline mode
- **Sync failures**: Tracked with retry count and error messages
- **Database errors**: Logged with Debug.WriteLine
- **Validation errors**: Products validated before caching

## Performance Considerations

- **Singleton pattern**: Database and services use singleton for efficiency
- **Lazy loading**: Order items loaded on demand
- **Batch operations**: Sync queue processes items in batch
- **Connection pooling**: SQLite uses shared cache mode
- **Async/await**: All I/O operations are asynchronous

## Testing Offline Mode

To test offline functionality:

1. Enable airplane mode on device
2. Open app - products should load from cache
3. Create an order - should save locally
4. Check pending sync count
5. Disable airplane mode
6. Watch auto-sync process the order

## Future Enhancements

Potential improvements:

- [ ] Conflict resolution for concurrent edits
- [ ] Incremental sync for large datasets
- [ ] Background sync using background tasks
- [ ] Compression for JSON payloads
- [ ] Sync statistics and reporting
- [ ] User notification on sync completion
- [ ] Sync settings (auto-sync on/off, sync frequency)

## Dependencies

- **sqlite-net-pcl** (1.9.172): SQLite ORM
- **SQLitePCLRaw.bundle_green** (2.1.10): SQLite native binaries
- **Microsoft.Maui.Essentials**: Connectivity monitoring

## Notes

- Database stored in: `Environment.SpecialFolder.LocalApplicationData`
- Cache validity: 24 hours (configurable)
- Sync queue priority: 0=normal, 1=high, 2=critical
- All timestamps stored in UTC
