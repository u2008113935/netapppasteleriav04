using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace apppasteleriav04.ViewModels.Profile.Admin
{
    /// <summary>
    /// ViewModel para gestión de usuarios del administrador
    /// </summary>
    public class AdminUsersViewModel : BaseViewModel
    {
        private readonly AdminService _adminService;

        public ObservableCollection<UserProfile> Users { get; set; } = new ObservableCollection<UserProfile>();

        private UserProfile? _selectedUser;
        public UserProfile? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        private string _filterRole = "Todos";
        public string FilterRole
        {
            get => _filterRole;
            set
            {
                _filterRole = value;
                OnPropertyChanged();
                _ = LoadUsersAsync();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand LoadUsersCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand ChangeRoleCommand { get; }
        public ICommand DeactivateUserCommand { get; }

        public AdminUsersViewModel()
        {
            _adminService = AdminService.Instance;

            LoadUsersCommand = new Command(async () => await LoadUsersAsync());
            EditUserCommand = new Command<UserProfile>(OnEditUser);
            ChangeRoleCommand = new Command<string>(async (role) => await UpdateUserRoleAsync(role));
            DeactivateUserCommand = new Command<UserProfile>(OnDeactivateUser);
        }

        public async Task LoadUsersAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                var role = FilterRole == "Todos" ? null : FilterRole.ToLower();
                var users = await _adminService.GetAllUsersAsync(role);

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminUsersViewModel] Error loading users: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnEditUser(UserProfile? user)
        {
            if (user != null)
            {
                SelectedUser = user;
                // Open edit dialog or navigate to edit page
            }
        }

        public async Task UpdateUserRoleAsync(string? newRole)
        {
            if (SelectedUser == null || string.IsNullOrEmpty(newRole)) return;

            IsLoading = true;
            try
            {
                var success = await _adminService.UpdateUserRoleAsync(SelectedUser.Id, newRole);
                if (success)
                {
                    SelectedUser.Role = newRole;
                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminUsersViewModel] Error updating user role: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnDeactivateUser(UserProfile? user)
        {
            if (user != null)
            {
                // Implement user deactivation logic
                Debug.WriteLine($"[AdminUsersViewModel] Deactivating user: {user.Email}");
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            // Implement INotifyPropertyChanged if BaseViewModel doesn't have it
        }
    }
}
