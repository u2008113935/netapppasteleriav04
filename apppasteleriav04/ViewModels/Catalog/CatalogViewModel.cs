using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace apppasteleriav04.ViewModels.Catalog
{
    public class CatalogViewModel : BaseViewModel
    {
        public ObservableCollection<Product> Products { get; set; } = new();
        public bool IsBusy { get; set; }

        public CatalogViewModel() { }

        public async Task LoadProductsAsync()
        {
            IsBusy = true;
            var products = await SupabaseService.Instance.GetProductsAsync();
            Products.Clear();
            foreach (var p in products)
            {
                Debug.WriteLine($"Producto en ViewModel: {p.Nombre}, ImagenPath={p.ImagenPath}");
                Products.Add(p);
            }
            IsBusy = false;
        }
    }
}
