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
    /// ViewModel para gestión de productos del administrador
    /// </summary>
    public class AdminProductsViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabaseService;
        private readonly AdminService _adminService;

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        
        private ObservableCollection<Product> _allProducts = new ObservableCollection<Product>();

        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
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

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        private string _filterCategory = "Todas";
        public string FilterCategory
        {
            get => _filterCategory;
            set
            {
                _filterCategory = value;
                OnPropertyChanged();
                FilterProducts();
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
        public ICommand LoadProductsCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand SaveProductCommand { get; }

        public AdminProductsViewModel()
        {
            _supabaseService = SupabaseService.Instance;
            _adminService = AdminService.Instance;

            LoadProductsCommand = new Command(async () => await LoadProductsAsync());
            AddProductCommand = new Command(OnAddProduct);
            EditProductCommand = new Command<Product>(OnEditProduct);
            DeleteProductCommand = new Command<Product>(async (p) => await DeleteProductAsync(p));
            SaveProductCommand = new Command(async () => await SaveProductAsync());
        }

        public async Task LoadProductsAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                var products = await _supabaseService.GetProductsAsync();
                
                _allProducts.Clear();
                Products.Clear();
                
                foreach (var product in products)
                {
                    _allProducts.Add(product);
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminProductsViewModel] Error loading products: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnAddProduct()
        {
            SelectedProduct = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = string.Empty,
                Descripcion = string.Empty,
                Categoria = string.Empty,
                Precio = 0
            };
            IsEditing = true;
        }

        private void OnEditProduct(Product? product)
        {
            if (product != null)
            {
                SelectedProduct = product;
                IsEditing = true;
            }
        }

        public async Task SaveProductAsync()
        {
            if (SelectedProduct == null) return;

            IsLoading = true;
            try
            {
                bool success;
                var existingProduct = _allProducts.FirstOrDefault(p => p.Id == SelectedProduct.Id);

                if (existingProduct != null)
                {
                    // Update existing product
                    success = await _adminService.UpdateProductAsync(SelectedProduct);
                }
                else
                {
                    // Create new product
                    var created = await _adminService.CreateProductAsync(SelectedProduct);
                    success = created != null;
                }

                if (success)
                {
                    await LoadProductsAsync();
                    IsEditing = false;
                    SelectedProduct = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminProductsViewModel] Error saving product: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DeleteProductAsync(Product? product)
        {
            if (product == null) return;

            IsLoading = true;
            try
            {
                var success = await _adminService.DeleteProductAsync(product.Id);
                if (success)
                {
                    await LoadProductsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminProductsViewModel] Error deleting product: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterProducts()
        {
            Products.Clear();
            
            var filtered = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(p => 
                    (p.Nombre?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Descripcion?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (FilterCategory != "Todas" && !string.IsNullOrWhiteSpace(FilterCategory))
            {
                filtered = filtered.Where(p => p.Categoria == FilterCategory);
            }

            foreach (var product in filtered)
            {
                Products.Add(product);
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            // Implement INotifyPropertyChanged if BaseViewModel doesn't have it
        }
    }
}
