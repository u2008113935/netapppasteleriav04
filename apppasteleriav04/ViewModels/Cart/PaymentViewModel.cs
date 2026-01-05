using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Cart
{
    public class PaymentViewModel : BaseViewModel
    {
        private Order? _order;
        public Order? Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _selectedPaymentMethod = "efectivo";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                SetProperty(ref _selectedPaymentMethod, value);
                OnPropertyChanged(nameof(ShowCardFields));
            }
        }

        public bool ShowCardFields => SelectedPaymentMethod == "tarjeta";

        private string _cardNumber = string.Empty;
        public string CardNumber
        {
            get => _cardNumber;
            set => SetProperty(ref _cardNumber, value);
        }

        private string _cardExpiry = string.Empty;
        public string CardExpiry
        {
            get => _cardExpiry;
            set => SetProperty(ref _cardExpiry, value);
        }

        private string _cardCvv = string.Empty;
        public string CardCvv
        {
            get => _cardCvv;
            set => SetProperty(ref _cardCvv, value);
        }

        private string _cardHolder = string.Empty;
        public string CardHolder
        {
            get => _cardHolder;
            set => SetProperty(ref _cardHolder, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        private string _paymentStatus = string.Empty;
        public string PaymentStatus
        {
            get => _paymentStatus;
            set => SetProperty(ref _paymentStatus, value);
        }

        public ICommand ProcessPaymentCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RetryCommand { get; }

        public event EventHandler<Payment>? PaymentCompleted;
        public event EventHandler<string>? PaymentFailed;
        public event EventHandler? PaymentCancelled;

        public PaymentViewModel()
        {
            Title = "Procesar Pago";
            ProcessPaymentCommand = new AsyncRelayCommand(ProcessPaymentAsync, () => !IsProcessing);
            CancelCommand = new RelayCommand(() => PaymentCancelled?.Invoke(this, EventArgs.Empty));
            RetryCommand = new AsyncRelayCommand(ProcessPaymentAsync, () => !IsProcessing);
        }

        public void Initialize(Order order, decimal amount)
        {
            Order = order;
            Amount = amount;
        }

        private async Task ProcessPaymentAsync()
        {
            ErrorMessage = string.Empty;
            PaymentStatus = string.Empty;

            if (Order == null)
            {
                ErrorMessage = "Orden no válida";
                return;
            }

            if (SelectedPaymentMethod == "tarjeta")
            {
                if (!ValidateCardData())
                {
                    return;
                }
            }

            IsProcessing = true;
            IsBusy = true;
            PaymentStatus = "Procesando pago...";

            try
            {
                await Task.Delay(2000); // Simulate payment processing

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = Order.Id,
                    Amount = Amount,
                    PaymentMethod = SelectedPaymentMethod,
                    Status = "completado",
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                if (SelectedPaymentMethod == "tarjeta")
                {
                    payment.LastFourDigits = CardNumber.Length >= 4 ? CardNumber.Substring(CardNumber.Length - 4) : "****";
                    payment.Gateway = "culqi";
                }

                PaymentStatus = "Pago completado exitosamente";
                PaymentCompleted?.Invoke(this, payment);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al procesar el pago: {ex.Message}";
                PaymentStatus = "Pago fallido";
                PaymentFailed?.Invoke(this, ex.Message);
            }
            finally
            {
                IsProcessing = false;
                IsBusy = false;
            }
        }

        private bool ValidateCardData()
        {
            if (string.IsNullOrWhiteSpace(CardNumber) || CardNumber.Length < 13)
            {
                ErrorMessage = "Número de tarjeta inválido";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CardExpiry))
            {
                ErrorMessage = "Fecha de vencimiento requerida";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CardCvv) || CardCvv.Length < 3)
            {
                ErrorMessage = "CVV inválido";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CardHolder))
            {
                ErrorMessage = "Nombre del titular requerido";
                return false;
            }

            return true;
        }
    }
}
