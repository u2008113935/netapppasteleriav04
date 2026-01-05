using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Profile.Admin;

namespace apppasteleriav04.Views.Profile.Admin
{
    public partial class AdminUsersPage : ContentPage
    {
        private readonly AdminUsersViewModel _viewModel;

        public AdminUsersPage()
        {
            InitializeComponent();
            _viewModel = new AdminUsersViewModel();
            BindingContext = _viewModel;
            
            RolePicker.SelectedIndex = 0; // "Todos"
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            await _viewModel.LoadUsersAsync();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private async void OnRoleFilterChanged(object sender, EventArgs e)
        {
            if (RolePicker.SelectedItem is string role)
            {
                _viewModel.FilterRole = role;
                await LoadUsers();
            }
        }

        private async void OnChangeRoleClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is UserProfile user)
            {
                _viewModel.SelectedUser = user;
                
                string newRole = await DisplayActionSheet(
                    "Seleccionar nuevo rol", 
                    "Cancelar", 
                    null, 
                    "cliente", "vendedor", "empleado", "gerente", "operaciones", "ti");

                if (!string.IsNullOrEmpty(newRole) && newRole != "Cancelar")
                {
                    await _viewModel.UpdateUserRoleAsync(newRole);
                }
            }
        }

        private void OnViewHistoryClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is UserProfile user)
            {
                DisplayAlert("Historial", $"Ver historial de pedidos de {user.Email}", "OK");
                // Navigate to user history page
            }
        }
    }
}
