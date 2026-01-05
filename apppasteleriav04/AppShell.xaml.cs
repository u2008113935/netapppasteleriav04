using apppasteleriav04.Views.Auth;
using apppasteleriav04.Views.Cart;
using apppasteleriav04.Views.Billing;

namespace apppasteleriav04;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        // Auth routes
        Routing.RegisterRoute("login", typeof(LoginPage));
        
        // Cart and Payment routes
        Routing.RegisterRoute("checkout", typeof(CheckoutPage));
        Routing.RegisterRoute("payment", typeof(PaymentPage));
        Routing.RegisterRoute("payment-success", typeof(PaymentSuccessPage));
        Routing.RegisterRoute("payment-failed", typeof(PaymentFailedPage));
        
        // Billing/Invoice routes
        Routing.RegisterRoute("invoice", typeof(InvoicePage));
        Routing.RegisterRoute("invoices", typeof(InvoiceListPage));
        Routing.RegisterRoute("invoice-detail", typeof(InvoiceDetailPage));
    }
}