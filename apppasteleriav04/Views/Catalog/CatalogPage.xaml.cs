using apppasteleriav04.ViewModels.Catalog;
using Microsoft.Maui.Controls;

namespace apppasteleriav04.Views.Catalog
{
    public partial class CatalogPage : ContentPage
    {
        private readonly CatalogViewModel _viewModel;

        public CatalogPage()
        {
            InitializeComponent();
            _viewModel = new CatalogViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (_viewModel.Products.Count == 0)
            {
                await _viewModel.LoadProductsAsync();
            }
        }
    }
}
