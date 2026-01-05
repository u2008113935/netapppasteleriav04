using apppasteleriav04.Views.Auth;
using apppasteleriav04.Views.Cart;
using apppasteleriav04.Views.Profile.Employee;

namespace apppasteleriav04;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("checkout", typeof(CheckoutPage));
        
        // Employee routes
        Routing.RegisterRoute("employee/dashboard", typeof(EmployeeDashboardPage));
        Routing.RegisterRoute("employee/orders", typeof(EmployeeOrdersPage));
        Routing.RegisterRoute("employee/kitchen", typeof(KitchenPage));
        Routing.RegisterRoute("employee/orderdetails", typeof(OrderDetailsPage));
    }
}