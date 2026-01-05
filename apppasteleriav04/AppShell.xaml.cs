using apppasteleriav04.Views.Auth;
using apppasteleriav04.Views.Cart;

namespace apppasteleriav04;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("checkout", typeof(CheckoutPage));
    }
}