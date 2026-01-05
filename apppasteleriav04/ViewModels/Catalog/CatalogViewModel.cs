using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace apppasteleriav04.ViewModels.Catalog
{
    public class CatalogViewModel : BaseViewModel
    {
        private ObservableCollection<Product> _products = new();
        private ObservableCollection<Product> _filteredProducts = new();
        private string _searchText = string.Empty;

        /// <summary>
        /// Gets the collection of all products
        /// </summary>
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        /// <summary>
        /// Gets the filtered collection of products based on search
        /// </summary>
        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set => SetProperty(ref _filteredProducts, value);
        }

        /// <summary>
        /// Gets or sets the search text
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterProducts();
                }
            }
        }

        /// <summary>
        /// Command to load products
        /// </summary>
        public ICommand LoadProductsCommand { get; }

        /// <summary>
        /// Command to add product to cart
        /// </summary>
        public ICommand AddToCartCommand { get; }

        /// <summary>
        /// Command to refresh products
        /// </summary>
        public ICommand RefreshCommand { get; }

        public CatalogViewModel()
        {
            Title = "Catálogo";
            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            AddToCartCommand = new RelayCommand<Product>(AddToCart);
            RefreshCommand = new AsyncRelayCommand(LoadProductsAsync);
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
                
                foreach (var p in products)
                {
                    Debug.WriteLine($"Producto en ViewModel: {p.Nombre}, ImagenPath={p.ImagenPath}");
                    Products.Add(p);
                }

                FilterProducts();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CatalogViewModel] Error loading products: {ex}");
                ErrorMessage = "Error al cargar productos";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Filter products based on search text
        /// </summary>
        private void FilterProducts()
        {
            FilteredProducts.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var product in Products)
                {
                    FilteredProducts.Add(product);
                }
            }
            else
            {
                var filtered = Products.Where(p =>
                    (!string.IsNullOrEmpty(p.Nombre) && p.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Descripcion) && p.Descripcion.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );

                foreach (var product in filtered)
                {
                    FilteredProducts.Add(product);
                }
            }
        }

        /// <summary>
        /// Add a product to the cart
        /// </summary>
        private void AddToCart(Product? product)
        {
            if (product != null)
            {
                CartService.Instance.Add(product, 1);
                Debug.WriteLine($"[CatalogViewModel] Added {product.Nombre} to cart");
            }
        }
    }
}
