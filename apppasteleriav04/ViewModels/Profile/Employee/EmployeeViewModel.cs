using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.ViewModels.Profile.Employee
{
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        private Models.Domain.Employee? _employee;
        private bool _isLoading;

        public Models.Domain.Employee? Employee
        {
            get => _employee;
            set
            {
                if (_employee == value) return;
                _employee = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadProfileCommand { get; }

        public EmployeeViewModel()
        {
            LoadProfileCommand = new Command(async () => await LoadProfileAsync());
        }

        private async Task LoadProfileAsync()
        {
            IsLoading = true;
            try
            {
                // TODO: Implement loading employee profile from service
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EmployeeViewModel] Error loading profile: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
