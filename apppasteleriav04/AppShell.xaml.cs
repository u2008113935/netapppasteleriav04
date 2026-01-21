using apppasteleriav04.Services.Core;
using apppasteleriav04.Views.Auth;
using apppasteleriav04.Views.Billing;
using apppasteleriav04.Views.Cart;
using apppasteleriav04.Views.Profile.Admin;
using apppasteleriav04.Views.Profile.Employee;
using System.Diagnostics;

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
        
        // Admin routes
        Routing.RegisterRoute("admin/dashboard", typeof(AdminDashboardPage));
        Routing.RegisterRoute("admin/products", typeof(AdminProductsPage));
        Routing.RegisterRoute("admin/orders", typeof(AdminOrdersPage));
        Routing.RegisterRoute("admin/users", typeof(AdminUsersPage));
        Routing.RegisterRoute("admin/promotions", typeof(PromotionsPage));
        Routing.RegisterRoute("admin/analytics", typeof(AnalyticsPage));
        Routing.RegisterRoute("payment", typeof(PaymentPage));
        Routing.RegisterRoute("payment-success", typeof(PaymentSuccessPage));
        Routing.RegisterRoute("payment-failed", typeof(PaymentFailedPage));
        
        // Billing/Invoice routes
        Routing.RegisterRoute("invoice", typeof(InvoicePage));
        Routing.RegisterRoute("invoices", typeof(InvoiceListPage));
        Routing.RegisterRoute("invoice-detail", typeof(InvoiceDetailPage));

        // RUTAS DE EMPLEADO (agregar)
        Routing.RegisterRoute("employee-dashboard", typeof(EmployeeDashboardPage));
        Routing.RegisterRoute("employee-kitchen", typeof(EmployeeKitchenPage));
        Routing.RegisterRoute("employee-delivery", typeof(EmployeeDeliveryPage));
        Routing.RegisterRoute("employee-backoffice", typeof(EmployeeBackOfficePage));

    }

    // NAVEGAR AUTOMÁTICAMENTE AL INICIAR SI HAY SESIÓN
    protected override async void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        try
        {
            // Cargar datos del storage
            await AuthService.Instance.LoadFromStorageAsync();

            Debug.WriteLine($"[AppShell] OnNavigated - IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
            Debug.WriteLine($"[AppShell] UserRole: {AuthService.Instance.UserRole}");

            if (AuthService.Instance.IsAuthenticated)
            {
                Debug.WriteLine($"[AppShell] Usuario autenticado:  {AuthService.Instance.UserEmail}");

                // Navegar según rol
                if (AuthService.Instance.IsEmployee())
                {
                    Debug.WriteLine($"[AppShell] Empleado detectado, navegando.. .");

                    if (AuthService.Instance.IsCocina())
                        await Shell.Current.GoToAsync("employee-kitchen");
                    else if (AuthService.Instance.IsReparto())
                        await Shell.Current.GoToAsync("employee-delivery");
                    else if (AuthService.Instance.IsBackoffice())
                        await Shell.Current.GoToAsync("employee-backoffice");
                    else
                        await Shell.Current.GoToAsync("employee-dashboard");
                }
            }
            else
            {
                Debug.WriteLine($"[AppShell] No hay sesión activa");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AppShell] Error en OnNavigated: {ex.Message}");
        }
    }

}