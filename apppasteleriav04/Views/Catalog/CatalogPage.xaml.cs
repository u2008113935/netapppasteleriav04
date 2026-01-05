using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using apppasteleriav04.ViewModels.Catalog;

namespace apppasteleriav04.Views.Catalog
{
    public partial class CatalogPage : ContentPage
    {
        CatalogViewModel vm;

        public CatalogPage()
        {
            InitializeComponent();
            vm = new CatalogViewModel();
            this.BindingContext = vm;
            _ = vm.LoadProductsAsync();
        }

        void OnAddToCartClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                CartService.Instance.Add(product, 1);
            }
        }

        void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = e.NewTextValue?.Trim();

            if (string.IsNullOrEmpty(text))
            {
                ProductsCollection.ItemsSource = vm.Products;
                return;
            }

            // Puedes agregar un filtro dinámico aquí sobre vm.Products...
            var filtered = vm.Products
                .Where(p =>
                    (!string.IsNullOrEmpty(p.Nombre) && p.Nombre.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Descripcion) && p.Descripcion.Contains(text, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            ProductsCollection.ItemsSource = filtered; // permite filtrado en tiempo real
        }
    }
}
