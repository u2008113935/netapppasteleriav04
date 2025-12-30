namespace apppasteleriav04.Views.Catalog
{
    public partial class CatalogPage : ContentPage
    {
        public CatalogPage()
        {
            InitializeComponent();
        }

        private void OnSearchClicked(object sender, EventArgs e)
        {
            // Alterna visibilidad del buscador
            ProductSearchBar.IsVisible = !ProductSearchBar.IsVisible;
            if (ProductSearchBar.IsVisible)
            {
                ProductSearchBar.Focus();
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            // Filtrar productos en base al texto
            // Ejemplo:
            // Products = FullProducts.Where(p => p.Name.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase)).ToList();
            // ProductsCollectionView.ItemsSource = Products;
        }

        private void OnFilterClicked(object sender, EventArgs e)
        {
            // Abre tu panel de filtros (puedes usar popup/modal)
            // DisplayActionSheet("Filtros", "Cancelar", null, "Tortas", "Cupcakes", "Galletas", "Ofertas", "Favoritos");
        }
    }
}