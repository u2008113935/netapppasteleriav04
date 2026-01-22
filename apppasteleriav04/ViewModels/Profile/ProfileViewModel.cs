using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Profile
{
    public class ProfileViewModel : BaseViewModel
    {
        // =================================
        //CAMPOS PRIVADOS Y PROPIEDADES (Data Binding)
        // =================================

        //Campo privado almacena perfil del usuario
        private UserProfile? _userProfile;

        //Notifica los cambios en el perfil del usuario
        public UserProfile? UserProfile
        {
            get => _userProfile;
            set => SetProperty(ref _userProfile, value);
        }

        //Email del usuario logueado
        private string _email = string.Empty;


        //Notifica los cambios en el email del usuario
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }


        //Nombre completo del usuario editable
        private string _fullName = string.Empty;

        //Notifica los cambios en el nombre completo del usuario
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }


        //URL del avatar del usuario
        private string _avatarUrl = string.Empty;

        //Notifica los cambios en la URL del avatar del usuario
        public string AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

        //Indica si el perfil está en modo edición o solo lectura 
        private bool _isEditing = false;

        //Notifica los cambios en el estado de edición del perfil
        public bool IsEditing
        {   
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }


        //Mensaje de error relacionado con el perfil
        private string _errorMessage = string.Empty;

        //Notifica los cambios en el mensaje de error
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }


        //Indica si hay un error presente
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        //Indica si el usuario está autenticado
        public bool IsAuthenticated => AuthService.Instance.IsAuthenticated;

        // =================================
        // COMANDOS (Binding a botones XAML)
        // =================================

        //Comando para cerrar sesión
        public ICommand LogoutCommand { get; }
        
        //Comando para editar perfil
        public ICommand EditProfileCommand { get; }

        //Comando para guardar perfil
        public ICommand SaveProfileCommand { get; }

        //Comando para cancelar edición de perfil
        public ICommand CancelEditCommand { get; }

        //Comando para cargar perfil
        public ICommand LoadProfileCommand { get; }

        //Evento disparado cuando usuario NO está autenticado
        public event EventHandler? AuthenticationRequired;

        //Evento disparado cuando cierre de sesión se completa
        public event EventHandler? LogoutCompleted;

        //Evento disparado cuando se solicita editar el perfil
        public event EventHandler? EditProfileRequested;


        // =================================
        // CONSTRUCTOR
        // =================================

        // Se ejecuta UNA VEZ cuando se crea la instancia del ViewModel
        // Aqui se inicializan los comandos 
        public ProfileViewModel()
        {
            Title = "Perfil";
            LogoutCommand = new RelayCommand(Logout);
            EditProfileCommand = new RelayCommand(EnableEditing);
            SaveProfileCommand = new RelayCommand(SaveProfile);
            CancelEditCommand = new RelayCommand(CancelEdit);
            LoadProfileCommand = new AsyncRelayCommand(LoadProfileAsync);
        }



        // =================================
        // MÉTODOS PÚBLICOS
        // =================================
        
        
        //Verificar la sesion activa
        public bool CheckAuthentication()
        {
            //Si no está autenticado, dispara el evento AuthenticationRequired
            if (!AuthService.Instance.IsAuthenticated)
            {
                AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                return false;
            }
            return true;
        }

        // Cargar datos del perfil desde Supabase de forma asincrona
        public async Task LoadProfileAsync()
        {
            //Si no está autenticado, muestra mensaje de error y sale
            if (!IsAuthenticated)
            {
                ErrorMessage = "No ha iniciado sesión";
                return;
            }

            IsBusy = true;

            // Manejo de errores, limpiar mensaje previos
            ErrorMessage = string.Empty;

            try
            {
                // Obtener email del usuario autenticado
                Email = AuthService.Instance.UserEmail ?? string.Empty;

                // Obtener ID del usuario desde Supabase
                var userId = AuthService.Instance.UserId;

                // Cargar perfil del usuario desde Supabase
                if (!string.IsNullOrEmpty(userId))
                {
                    var profile = await SupabaseService.Instance.GetProfileAsync(Guid.Parse(userId));


                    if (profile != null)
                    {
                        // Asignar perfil cargado a la propiedad UserProfile
                        UserProfile = profile;

                        FullName = profile.FullName ?? string.Empty;
                        AvatarUrl = profile.AvatarPublicUrl ?? string.Empty;
                    }                    
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar perfil: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] Error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =================================
        // MÉTODOS PRIVADOS
        // =================================

        // Activar modo edición
        private void EnableEditing()
        {
            IsEditing = true;
            EditProfileRequested?.Invoke(this, EventArgs.Empty);
        }

        // Cancela edicion y descata cambios
        private void CancelEdit()
        {
            // Desactivar modo edición
            IsEditing = false;

            // Recargar valores originales del perfil
            if (UserProfile != null)
            {
                FullName = UserProfile.FullName ?? string.Empty;
                AvatarUrl = UserProfile.AvatarPublicUrl ?? string.Empty;
            }

        }



        private void SaveProfile()
        {
            // Sincronizar valores editados al objeto UserProfile
            if (UserProfile != null)
            {
                UserProfile.FullName = FullName;

                UserProfile.AvatarUrl = AvatarUrl;
            }

            // DEsactiva el modo edidicon
            IsEditing = false;
            ErrorMessage = "Perfil actualizado correctamente";
        }

        // Ejecuta logout del usuario
        private void Logout()
        {
            // Elimina sesion del servicio
            AuthService.Instance.Logout();

            // Limpiar datos del perfil
            UserProfile = null;
            Email = string.Empty;
            FullName = string.Empty;
            AvatarUrl = string.Empty;

            // Notificar cambios en la autenticación a binding
            OnPropertyChanged(nameof(IsAuthenticated));

            // Disparar evento de cierre de sesión completado
            LogoutCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
