using System;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.Mocks
{
    public class MockConnectivityService
    {
        public bool IsConnected { get; set; } = true;
        public event EventHandler<bool>? ConnectivityChanged;

        public bool IsInternetAvailable()
        {
            return IsConnected;
        }

        public Task<bool> CheckConnectivityAsync()
        {
            return Task.FromResult(IsConnected);
        }

        public void SimulateConnectivityChange(bool isConnected)
        {
            IsConnected = isConnected;
            ConnectivityChanged?.Invoke(this, isConnected);
        }

        public void SimulateOffline()
        {
            SimulateConnectivityChange(false);
        }

        public void SimulateOnline()
        {
            SimulateConnectivityChange(true);
        }

        public void Reset()
        {
            IsConnected = true;
        }
    }
}
