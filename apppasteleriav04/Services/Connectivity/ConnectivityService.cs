using System;
using System.Diagnostics;

namespace apppasteleriav04.Services.Connectivity
{
    /// <summary>
    /// Service for monitoring network connectivity using MAUI Essentials
    /// </summary>
    public class ConnectivityService : IConnectivityService
    {
        private bool _isMonitoring;

        public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

        public bool IsConnected => Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess == 
            Microsoft.Maui.Networking.NetworkAccess.Internet;

        public ConnectivityService()
        {
            Debug.WriteLine("[ConnectivityService] Initialized");
        }

        public void StartMonitoring()
        {
            if (_isMonitoring)
                return;

            Debug.WriteLine("[ConnectivityService] Starting connectivity monitoring");

            Microsoft.Maui.Networking.Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
            _isMonitoring = true;
        }

        public void StopMonitoring()
        {
            if (!_isMonitoring)
                return;

            Debug.WriteLine("[ConnectivityService] Stopping connectivity monitoring");

            Microsoft.Maui.Networking.Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
            _isMonitoring = false;
        }

        private void OnConnectivityChanged(object? sender, Microsoft.Maui.Networking.ConnectivityChangedEventArgs e)
        {
            var isConnected = e.NetworkAccess == Microsoft.Maui.Networking.NetworkAccess.Internet;

            Debug.WriteLine($"[ConnectivityService] Connectivity changed: {(isConnected ? "Online" : "Offline")}");

            ConnectivityChanged?.Invoke(this, new ConnectivityChangedEventArgs(isConnected));
        }
    }
}
