using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Profile.Admin;

namespace apppasteleriav04.Views.Profile.Admin
{
    public partial class PromotionsPage : ContentPage
    {
        private readonly PromotionsViewModel _viewModel;

        public PromotionsPage()
        {
            InitializeComponent();
            _viewModel = new PromotionsViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPromotions();
        }

        private async Task LoadPromotions()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            await _viewModel.LoadPromotionsAsync();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private void OnAddPromotionClicked(object sender, EventArgs e)
        {
            _viewModel.AddPromotionCommand.Execute(null);
            ShowEditModal();
            ClearEditForm();
        }

        private void OnEditPromotionClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Promotion promotion)
            {
                _viewModel.EditPromotionCommand.Execute(promotion);
                ShowEditModal();
                FillEditForm(promotion);
            }
        }

        private async void OnDeletePromotionClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Promotion promotion)
            {
                bool confirm = await DisplayAlert("Confirmar", 
                    $"¿Está seguro de eliminar la promoción {promotion.DisplayName}?", 
                    "Sí", "No");
                
                if (confirm)
                {
                    _viewModel.DeletePromotionCommand.Execute(promotion);
                }
            }
        }

        private async void OnSavePromotionClicked(object sender, EventArgs e)
        {
            if (_viewModel.SelectedPromotion == null) return;

            // Update promotion from form
            _viewModel.SelectedPromotion.Name = NameEntry.Text;
            _viewModel.SelectedPromotion.Description = DescriptionEntry.Text;
            _viewModel.SelectedPromotion.Code = CodeEntry.Text;
            
            if (DiscountTypePicker.SelectedItem is string discountType)
            {
                _viewModel.SelectedPromotion.DiscountType = discountType;
            }

            if (decimal.TryParse(DiscountValueEntry.Text, out decimal discountValue))
            {
                _viewModel.SelectedPromotion.DiscountValue = discountValue;
            }
            
            _viewModel.SelectedPromotion.StartDate = StartDatePicker.Date;
            _viewModel.SelectedPromotion.EndDate = EndDatePicker.Date;

            await _viewModel.SavePromotionAsync();
            HideEditModal();
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            HideEditModal();
        }

        private void ShowEditModal()
        {
            EditModal.IsVisible = true;
        }

        private void HideEditModal()
        {
            EditModal.IsVisible = false;
        }

        private void FillEditForm(Promotion promotion)
        {
            NameEntry.Text = promotion.Name ?? string.Empty;
            DescriptionEntry.Text = promotion.Description ?? string.Empty;
            CodeEntry.Text = promotion.Code ?? string.Empty;
            DiscountTypePicker.SelectedItem = promotion.DiscountType ?? "percentage";
            DiscountValueEntry.Text = promotion.DiscountValue.ToString();
            StartDatePicker.Date = promotion.StartDate;
            EndDatePicker.Date = promotion.EndDate;
        }

        private void ClearEditForm()
        {
            NameEntry.Text = string.Empty;
            DescriptionEntry.Text = string.Empty;
            CodeEntry.Text = string.Empty;
            DiscountTypePicker.SelectedIndex = 0;
            DiscountValueEntry.Text = string.Empty;
            StartDatePicker.Date = DateTime.Today;
            EndDatePicker.Date = DateTime.Today.AddDays(30);
        }
    }
}
