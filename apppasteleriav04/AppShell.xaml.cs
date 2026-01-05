using apppasteleriav04.Views.Auth;
using apppasteleriav04.Views.Cart;
using apppasteleriav04.Views.Profile.Admin;

namespace apppasteleriav04;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("checkout", typeof(CheckoutPage));
        
        // Admin routes
        Routing.RegisterRoute("admin/dashboard", typeof(AdminDashboardPage));
        Routing.RegisterRoute("admin/products", typeof(AdminProductsPage));
        Routing.RegisterRoute("admin/orders", typeof(AdminOrdersPage));
        Routing.RegisterRoute("admin/users", typeof(AdminUsersPage));
        Routing.RegisterRoute("admin/promotions", typeof(PromotionsPage));
        Routing.RegisterRoute("admin/analytics", typeof(AnalyticsPage));
    }
}