namespace apppasteleriav04.Services.Connectivity
{
    public interface IConnectivityService
    {
        bool IsConnected { get; }
        event EventHandler<bool> ConnectivityChanged;
    }
}
