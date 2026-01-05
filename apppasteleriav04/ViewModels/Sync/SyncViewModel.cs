using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Sync
{
    public class SyncViewModel : BaseViewModel
    {
        private int _pendingCount;
        public int PendingCount
        {
            get => _pendingCount;
            set => SetProperty(ref _pendingCount, value);
        }

        private DateTime? _lastSyncTime;
        public DateTime? LastSyncTime
        {
            get => _lastSyncTime;
            set
            {
                SetProperty(ref _lastSyncTime, value);
                OnPropertyChanged(nameof(LastSyncTimeText));
            }
        }

        public string LastSyncTimeText
        {
            get
            {
                if (LastSyncTime == null)
                    return "Nunca sincronizado";

                var elapsed = DateTime.Now - LastSyncTime.Value;
                if (elapsed.TotalMinutes < 1)
                    return "Hace unos segundos";
                if (elapsed.TotalHours < 1)
                    return $"Hace {(int)elapsed.TotalMinutes} minutos";
                if (elapsed.TotalDays < 1)
                    return $"Hace {(int)elapsed.TotalHours} horas";
                return $"Hace {(int)elapsed.TotalDays} días";
            }
        }

        private bool _isSyncing;
        public bool IsSyncing
        {
            get => _isSyncing;
            set => SetProperty(ref _isSyncing, value);
        }

        private string _syncStatus = "Listo para sincronizar";
        public string SyncStatus
        {
            get => _syncStatus;
            set => SetProperty(ref _syncStatus, value);
        }

        public ICommand SyncNowCommand { get; }
        public ICommand ClearQueueCommand { get; }

        public SyncViewModel()
        {
            Title = "Sincronización";
            SyncNowCommand = new AsyncRelayCommand(SyncAsync, () => !IsSyncing);
            ClearQueueCommand = new RelayCommand(ClearQueue, () => PendingCount > 0);
        }

        public async Task SyncAsync()
        {
            ErrorMessage = string.Empty;
            IsSyncing = true;
            IsBusy = true;
            SyncStatus = "Sincronizando...";

            try
            {
                // Simulate sync operation
                await Task.Delay(2000);

                // In a real app, this would call the SyncService
                PendingCount = 0;
                LastSyncTime = DateTime.Now;
                SyncStatus = "Sincronización completada";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al sincronizar: {ex.Message}";
                SyncStatus = "Error en sincronización";
            }
            finally
            {
                IsSyncing = false;
                IsBusy = false;
            }
        }

        private void ClearQueue()
        {
            PendingCount = 0;
            SyncStatus = "Cola limpiada";
        }

        public void UpdatePendingCount(int count)
        {
            PendingCount = count;
        }
    }
}
