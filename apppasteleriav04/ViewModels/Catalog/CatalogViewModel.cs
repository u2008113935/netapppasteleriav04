using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Input;

namespace apppasteleriav04.ViewModels.Catalog
{
    public class CatalogViewModel : BaseViewModel
    {
        private ObservableCollection<Product> _products = new();
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private ObservableCollection<Product> _filteredProducts = new();
        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set => SetProperty(ref _filteredProducts, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterProducts();
            }
        }

        private string? _selectedCategory;
        public string? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                FilterProducts();
            }
        }

        public ICommand LoadProductsCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }

        public event EventHandler<Product>? ProductAddedToCart;

        public CatalogViewModel()
        {
            Title = "Catálogo";
            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            AddToCartCommand = new RelayCommand<Product>(AddToCart);
            RefreshCommand = new AsyncRelayCommand(LoadProductsAsync);
            SearchCommand = new RelayCommand(FilterProducts);
        }

        /// <summary>
        /// Load products from the service
        /// </summary>
        public async Task LoadProductsAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var products = await SupabaseService.Instance.GetProductsAsync();
                
                Products.Clear();
                FilteredProducts.Clear();
                
                foreach (var p in products)
                {
                    Debug.WriteLine($"Producto en ViewModel: {p.Nombre}, ImagenPath={p.ImagenPath}");
                    Products.Add(p);
                    FilteredProducts.Add(p);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar productos: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterProducts()
        {
            var filtered = Products.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(p => 
                    p.Nombre?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    p.Descripcion?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrWhiteSpace(SelectedCategory))
            {
                filtered = filtered.Where(p => p.Categoria == SelectedCategory);
            }

            FilteredProducts.Clear();
            foreach (var product in filtered)
            {
                FilteredProducts.Add(product);
            }
        }

        private void AddToCart(Product? product)
        {
            if (product != null)
            {
                CartService.Instance.Add(product, 1);
                ProductAddedToCart?.Invoke(this, product);
            }
        }
    }
}
