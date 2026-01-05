using apppasteleriav04.ViewModels.Cart;

namespace apppasteleriav04.Views.Cart;

public partial class PaymentPage : ContentPage
{
    private PaymentViewModel _viewModel;

    public PaymentPage()
    {
        InitializeComponent();
        _viewModel = new PaymentViewModel();
        BindingContext = _viewModel;

        SetupEventHandlers();
    }

    public PaymentPage(PaymentViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private void SetupEventHandlers()
    {
        // Handle property changes to show/hide card details
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PaymentViewModel.SelectedPaymentMethod))
            {
                CardDetailsSection.IsVisible = _viewModel.SelectedPaymentMethod == "tarjeta";
            }
            else if (e.PropertyName == nameof(PaymentViewModel.PaymentStatus))
            {
                StatusLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.PaymentStatus);
            }
            else if (e.PropertyName == nameof(PaymentViewModel.ErrorMessage))
            {
                ErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.ErrorMessage);
            }
        };

        // Handle payment events
        _viewModel.PaymentCompleted += async (s, e) =>
        {
            await Shell.Current.GoToAsync("//cart/payment-success");
        };

        _viewModel.PaymentFailed += async (s, e) =>
        {
            await Shell.Current.GoToAsync("//cart/payment-failed");
        };
    }

    private void OnCashSelected(object sender, EventArgs e)
    {
        _viewModel.SelectedPaymentMethod = "efectivo";
    }

    private void OnCardSelected(object sender, EventArgs e)
    {
        _viewModel.SelectedPaymentMethod = "tarjeta";
    }

    private void OnYapeSelected(object sender, EventArgs e)
    {
        _viewModel.SelectedPaymentMethod = "yape";
    }

    private void OnPlinSelected(object sender, EventArgs e)
    {
        _viewModel.SelectedPaymentMethod = "plin";
    }
}