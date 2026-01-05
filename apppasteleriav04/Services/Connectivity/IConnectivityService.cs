using System;

namespace apppasteleriav04.Services.Connectivity
{
    /// <summary>
    /// Interface for monitoring network connectivity changes
    /// </summary>
    public interface IConnectivityService
    {
        /// <summary>
        /// Event fired when connectivity status changes
        /// </summary>
        event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;

        /// <summary>
        /// Gets the current network connectivity status
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Starts monitoring connectivity changes
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// Stops monitoring connectivity changes
        /// </summary>
        void StopMonitoring();
    }

    /// <summary>
    /// Event args for connectivity change events
    /// </summary>
    public class ConnectivityChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }

        public ConnectivityChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
