using apppasteleriav04.Services.Core;
using apppasteleriav04.Views.Auth;
using apppasteleriav04.Views.Billing;
using apppasteleriav04.Views.Cart;
using apppasteleriav04.Views.Profile.Admin;
using apppasteleriav04.Views.Profile.Employee;
using apppasteleriav04.Views.Profile.Manager;
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
        Routing.RegisterRoute("employee", typeof(EmployeePage));

        // RUTAS DE MANAGER (GERENTE)
        Routing.RegisterRoute("manager", typeof(ManagerPage));
        

    }

    // NAVEGAR AUTOMÁTICAMENTE AL INICIAR SI HAY SESIÓN
    protected override async void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        try
        {
            // Paso 1: Cargar datos del storage
            await AuthService.Instance.LoadFromStorageAsync();

            Debug.WriteLine($"[AppShell] OnNavigated - IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
            Debug.WriteLine($"[AppShell] UserRole: {AuthService.Instance.UserRole}");

            if (AuthService.Instance.IsAuthenticated)
            {
                Debug.WriteLine($"[AppShell] Usuario autenticado:  {AuthService.Instance.UserEmail}");

                // Paso 2: Obtener el rol

                var userRole = AuthService.Instance.UserRole?.ToLower();
                Debug.WriteLine($"[AppShell] Rol detectado: {userRole}");

                // Paso 3: Navegar según rol

                if (userRole == "gerente" )
                {
                    Debug.WriteLine($"[AppShell] Admin detectado navegando a manager...");
                    await Shell.Current.GoToAsync("manager");
                }
                else if (AuthService.Instance.IsEmployee())
                {
                    Debug.WriteLine($"[AppShell] Empleado detectado, navegando.. .");

                    if (AuthService.Instance.IsCocina())
                    {
                        Debug.WriteLine($"[AppShell] Cocina detectada");
                        await Shell.Current.GoToAsync("employee-kitchen");
                    }
                    else if (AuthService.Instance.IsReparto())
                    {
                        Debug.WriteLine($"[AppShell] Delivery detectada");
                        await Shell.Current.GoToAsync("employee-delivery");
                    }
                    else if (AuthService.Instance.IsBackoffice())
                    {
                        Debug.WriteLine($"[AppShell] Backoffice detectada");
                        await Shell.Current.GoToAsync("employee-backoffice");
                    }
                    else
                    {
                        Debug.WriteLine($"[AppShell] Empleado generico detectada, mostrando el dashboard");
                        await Shell.Current.GoToAsync("employee-dashboard");
                    }
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
            Debug.WriteLine($"[AppShell] Stack trace: {ex.StackTrace}");
        }
    }

}