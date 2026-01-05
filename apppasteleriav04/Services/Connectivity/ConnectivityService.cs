using System;
using System.Diagnostics;

namespace apppasteleriav04.Services.Connectivity
{
    public class ConnectivityService : IConnectivityService
    {
        public event EventHandler<bool>? ConnectivityChanged;

        public bool IsConnected => Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess == Microsoft.Maui.Networking.NetworkAccess.Internet;

        public ConnectivityService()
        {
            Microsoft.Maui.Networking.Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
            Debug.WriteLine($"[ConnectivityService] Initialized. Current status: {IsConnected}");
        }

        private void OnConnectivityChanged(object? sender, Microsoft.Maui.Networking.ConnectivityChangedEventArgs e)
        {
            var isNowConnected = e.NetworkAccess == Microsoft.Maui.Networking.NetworkAccess.Internet;
            Debug.WriteLine($"[ConnectivityService] Connectivity changed: {isNowConnected}");
            ConnectivityChanged?.Invoke(this, isNowConnected);
        }
    }
}
