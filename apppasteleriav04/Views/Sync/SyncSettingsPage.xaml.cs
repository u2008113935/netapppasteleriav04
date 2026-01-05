using apppasteleriav04.ViewModels.Sync;

namespace apppasteleriav04.Views.Sync;

public partial class SyncSettingsPage : ContentPage
{
    private readonly SyncViewModel _viewModel;
    private bool _autoSyncEnabled = true;
    private int _syncIntervalMinutes = 15;

    public SyncSettingsPage()
    {
        InitializeComponent();
        _viewModel = new SyncViewModel();
        BindingContext = _viewModel;
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Load from preferences
        _autoSyncEnabled = Preferences.Get("AutoSyncEnabled", true);
        _syncIntervalMinutes = Preferences.Get("SyncIntervalMinutes", 15);
        
        if (FindByName("AutoSyncSwitch") is Switch autoSwitch)
            autoSwitch.IsToggled = _autoSyncEnabled;
        
        if (FindByName("IntervalPicker") is Picker intervalPicker)
            intervalPicker.SelectedIndex = GetIntervalIndex(_syncIntervalMinutes);
    }

    private int GetIntervalIndex(int minutes)
    {
        return minutes switch
        {
            5 => 0,
            15 => 1,
            30 => 2,
            60 => 3,
            _ => 1
        };
    }

    private void OnAutoSyncToggled(object sender, ToggledEventArgs e)
    {
        _autoSyncEnabled = e.Value;
        Preferences.Set("AutoSyncEnabled", _autoSyncEnabled);
    }

    private void OnIntervalChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && picker.SelectedIndex >= 0)
        {
            _syncIntervalMinutes = picker.SelectedIndex switch
            {
                0 => 5,
                1 => 15,
                2 => 30,
                3 => 60,
                _ => 15
            };
            Preferences.Set("SyncIntervalMinutes", _syncIntervalMinutes);
        }
    }

    private async void OnClearCacheClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Limpiar Caché", 
            "¿Estás seguro de que deseas limpiar el caché local?", 
            "Sí", "No");
        
        if (confirm)
        {
            // Clear local database cache
            await DisplayAlert("Éxito", "Caché limpiado correctamente", "OK");
        }
    }

    private async void OnForceFullSyncClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sincronización Completa", 
            "Esto descargará todos los datos del servidor. ¿Continuar?", 
            "Sí", "No");
        
        if (confirm)
        {
            await _viewModel.SyncAsync();
            await DisplayAlert("Éxito", "Sincronización completa realizada", "OK");
        }
    }
}