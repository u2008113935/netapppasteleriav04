// REEMPLAZAR COMPLETO
namespace apppasteleriav04
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas adicionales (navegación modal)
            Routing.RegisterRoute("productdetail", typeof(Views.Catalog.ProductDetailPage));
            Routing.RegisterRoute("checkout", typeof(Views.Cart.CheckoutPage));
            Routing.RegisterRoute("payment", typeof(Views.Cart.PaymentPage));
            Routing.RegisterRoute("tracking", typeof(Views.Orders.LiveTrackingPage));
            Routing.RegisterRoute("login", typeof(Views.Auth.LoginPage));
            Routing.RegisterRoute("register", typeof(Views.Auth.RegisterPage));
        }
    }
}