using apppasteleriav04.ViewModels.Sync;

namespace apppasteleriav04.Views.Sync;

public partial class SyncStatusPage : ContentPage
{
    private readonly SyncViewModel _viewModel;

    public SyncStatusPage()
    {
        InitializeComponent();
        _viewModel = new SyncViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateStatusUI();
    }

    private void UpdateStatusUI()
    {
        if (FindByName("PendingCountLabel") is Label pendingLabel)
            pendingLabel.Text = _viewModel.PendingCount.ToString();
        
        if (FindByName("LastSyncLabel") is Label lastSyncLabel)
            lastSyncLabel.Text = _viewModel.LastSyncTimeText;
        
        if (FindByName("StatusLabel") is Label statusLabel)
            statusLabel.Text = _viewModel.SyncStatus;
    }

    private async void OnSyncNowClicked(object sender, EventArgs e)
    {
        await _viewModel.SyncAsync();
        UpdateStatusUI();
        await DisplayAlert("Sincronización", _viewModel.SyncStatus, "OK");
    }

    private void OnClearQueueClicked(object sender, EventArgs e)
    {
        _viewModel.ClearQueueCommand.Execute(null);
        UpdateStatusUI();
    }
}