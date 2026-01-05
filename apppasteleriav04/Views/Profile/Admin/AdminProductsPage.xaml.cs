using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Profile.Admin;

namespace apppasteleriav04.Views.Profile.Admin
{
    public partial class AdminProductsPage : ContentPage
    {
        private readonly AdminProductsViewModel _viewModel;

        public AdminProductsPage()
        {
            InitializeComponent();
            _viewModel = new AdminProductsViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadProducts();
        }

        private async Task LoadProducts()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            await _viewModel.LoadProductsAsync();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.SearchText = e.NewTextValue;
        }

        private void OnAddProductClicked(object sender, EventArgs e)
        {
            _viewModel.AddProductCommand.Execute(null);
            ShowEditModal();
        }

        private void OnEditProductClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                _viewModel.EditProductCommand.Execute(product);
                ShowEditModal();
                FillEditForm(product);
            }
        }

        private async void OnDeleteProductClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                bool confirm = await DisplayAlert("Confirmar", 
                    $"¿Está seguro de eliminar el producto {product.DisplayName}?", 
                    "Sí", "No");
                
                if (confirm)
                {
                    await _viewModel.DeleteProductAsync(product);
                }
            }
        }

        private async void OnSaveProductClicked(object sender, EventArgs e)
        {
            if (_viewModel.SelectedProduct == null) return;

            // Update product from form
            _viewModel.SelectedProduct.Nombre = NameEntry.Text;
            _viewModel.SelectedProduct.Descripcion = DescriptionEntry.Text;
            _viewModel.SelectedProduct.Categoria = CategoryEntry.Text;
            
            if (decimal.TryParse(PriceEntry.Text, out decimal price))
            {
                _viewModel.SelectedProduct.Precio = price;
            }
            
            _viewModel.SelectedProduct.ImagenPath = ImageUrlEntry.Text;

            await _viewModel.SaveProductAsync();
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
            ClearEditForm();
        }

        private void FillEditForm(Product product)
        {
            NameEntry.Text = product.Nombre ?? string.Empty;
            DescriptionEntry.Text = product.Descripcion ?? string.Empty;
            CategoryEntry.Text = product.Categoria ?? string.Empty;
            PriceEntry.Text = product.Precio?.ToString() ?? "0";
            ImageUrlEntry.Text = product.ImagenPath ?? string.Empty;
        }

        private void ClearEditForm()
        {
            NameEntry.Text = string.Empty;
            DescriptionEntry.Text = string.Empty;
            CategoryEntry.Text = string.Empty;
            PriceEntry.Text = string.Empty;
            ImageUrlEntry.Text = string.Empty;
        }
    }
}
