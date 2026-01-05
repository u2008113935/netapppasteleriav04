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
    /// ViewModel para gestión de promociones
    /// </summary>
    public class PromotionsViewModel : BaseViewModel
    {
        private readonly AdminService _adminService;

        public ObservableCollection<Promotion> Promotions { get; set; } = new ObservableCollection<Promotion>();

        private Promotion? _selectedPromotion;
        public Promotion? SelectedPromotion
        {
            get => _selectedPromotion;
            set
            {
                _selectedPromotion = value;
                OnPropertyChanged();
            }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
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
        public ICommand LoadPromotionsCommand { get; }
        public ICommand AddPromotionCommand { get; }
        public ICommand EditPromotionCommand { get; }
        public ICommand DeletePromotionCommand { get; }
        public ICommand SavePromotionCommand { get; }

        public PromotionsViewModel()
        {
            _adminService = AdminService.Instance;

            LoadPromotionsCommand = new Command(async () => await LoadPromotionsAsync());
            AddPromotionCommand = new Command(OnAddPromotion);
            EditPromotionCommand = new Command<Promotion>(OnEditPromotion);
            DeletePromotionCommand = new Command<Promotion>(OnDeletePromotion);
            SavePromotionCommand = new Command(async () => await SavePromotionAsync());
        }

        public async Task LoadPromotionsAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                var promotions = await _adminService.GetPromotionsAsync();

                Promotions.Clear();
                foreach (var promotion in promotions)
                {
                    Promotions.Add(promotion);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PromotionsViewModel] Error loading promotions: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnAddPromotion()
        {
            SelectedPromotion = new Promotion
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Description = string.Empty,
                DiscountType = "percentage",
                DiscountValue = 0,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                IsActive = true
            };
            IsEditing = true;
        }

        private void OnEditPromotion(Promotion? promotion)
        {
            if (promotion != null)
            {
                SelectedPromotion = promotion;
                IsEditing = true;
            }
        }

        private void OnDeletePromotion(Promotion? promotion)
        {
            if (promotion != null)
            {
                // Implement delete logic
                Debug.WriteLine($"[PromotionsViewModel] Deleting promotion: {promotion.Name}");
            }
        }

        public async Task SavePromotionAsync()
        {
            if (SelectedPromotion == null) return;

            IsLoading = true;
            try
            {
                bool success;
                var existingPromotion = Promotions.FirstOrDefault(p => p.Id == SelectedPromotion.Id);

                if (existingPromotion != null)
                {
                    // Update existing promotion
                    success = await _adminService.UpdatePromotionAsync(SelectedPromotion);
                }
                else
                {
                    // Create new promotion
                    var created = await _adminService.CreatePromotionAsync(SelectedPromotion);
                    success = created != null;
                }

                if (success)
                {
                    await LoadPromotionsAsync();
                    IsEditing = false;
                    SelectedPromotion = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PromotionsViewModel] Error saving promotion: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            // Implement INotifyPropertyChanged if BaseViewModel doesn't have it
        }
    }
}
