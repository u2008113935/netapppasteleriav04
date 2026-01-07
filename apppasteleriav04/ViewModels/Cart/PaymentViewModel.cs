using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Helpers;
using RelayCommand = apppasteleriav04.ViewModels.Base.RelayCommand;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Models.DTOs.Payment;
using apppasteleriav04.Services.Payment;
using apppasteleriav04.ViewModels.Base;
//using RelayCommand = apppasteleriav04.ViewModels.Base.RelayCommand;

namespace apppasteleriav04.ViewModels.Cart
{
    public class PaymentViewModel : BaseViewModel
    {
        private readonly IPaymentService _paymentService;

        private Order? _order;
        private decimal _amount;
        private string _selectedPaymentMethod = "efectivo";
        private string _cardNumber = string.Empty;
        private string _cardExpiry = string.Empty;
        private string _cardCvv = string.Empty;
        private string _cardHolder = string.Empty;
        private bool _isProcessing;
        private string _paymentStatus = string.Empty;
        private string _errorMessage = string.Empty;

        public PaymentViewModel()
        {
            _paymentService = new PaymentService();
            
            AvailablePaymentMethods = new ObservableCollection<PaymentMethodOption>
            {
                new PaymentMethodOption { Id = "efectivo", Name = "Efectivo", Icon = "💵" },
                new PaymentMethodOption { Id = "tarjeta", Name = "Tarjeta de Crédito/Débito", Icon = "💳" },
                new PaymentMethodOption { Id = "yape", Name = "Yape", Icon = "📱" },
                new PaymentMethodOption { Id = "plin", Name = "Plin", Icon = "📲" }
            };

            ProcessPaymentCommand = new AsyncRelayCommand(ProcessPaymentAsync, CanProcessPayment);
            CancelCommand = new RelayCommand(Cancel);
            RetryCommand = new AsyncRelayCommand(ProcessPaymentAsync, CanRetry);
        }

        public Order? Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                SetProperty(ref _selectedPaymentMethod, value);
                ((AsyncRelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public string CardNumber
        {
            get => _cardNumber;
            set
            {
                var formatted = CardValidator.FormatCardNumber(value);
                SetProperty(ref _cardNumber, formatted);
            }
        }

        public string CardExpiry
        {
            get => _cardExpiry;
            set => SetProperty(ref _cardExpiry, value);
        }

        public string CardCvv
        {
            get => _cardCvv;
            set => SetProperty(ref _cardCvv, value);
        }

        public string CardHolder
        {
            get => _cardHolder;
            set => SetProperty(ref _cardHolder, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                SetProperty(ref _isProcessing, value);
                ((AsyncRelayCommand)ProcessPaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public string PaymentStatus
        {
            get => _paymentStatus;
            set => SetProperty(ref _paymentStatus, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ObservableCollection<PaymentMethodOption> AvailablePaymentMethods { get; }

        public ICommand ProcessPaymentCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RetryCommand { get; }

        public event EventHandler? PaymentCompleted;
        public event EventHandler? PaymentFailed;

        public async Task ProcessPaymentAsync()
        {
            try
            {
                IsProcessing = true;
                ErrorMessage = string.Empty;
                PaymentStatus = "Procesando pago...";

                // Validate card if payment method is card
                if (SelectedPaymentMethod == "tarjeta")
                {
                    if (!ValidateCard())
                    {
                        ErrorMessage = "Por favor, verifica los datos de tu tarjeta";
                        PaymentStatus = "Error de validación";
                        PaymentFailed?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }

                // Create payment request
                var request = new PaymentRequestDto
                {
                    OrderId = Order?.Id ?? Guid.NewGuid(),
                    Amount = Amount,
                    Currency = "PEN",
                    PaymentMethod = SelectedPaymentMethod,
                    CustomerName = CardHolder,
                    Description = $"Pedido #{Order?.Id.ToString().Substring(0, 8)}"
                };

                // Process payment
                var result = await _paymentService.ProcessPaymentAsync(request);

                if (result.Success)
                {
                    PaymentStatus = "¡Pago completado exitosamente!";
                    await Task.Delay(1000);
                    PaymentCompleted?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = result.Message;
                    PaymentStatus = "Pago fallido";
                    PaymentFailed?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al procesar el pago: {ex.Message}";
                PaymentStatus = "Error";
                PaymentFailed?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        public bool ValidateCard()
        {
            if (SelectedPaymentMethod != "tarjeta")
                return true;

            // Validate card number
            if (!CardValidator.ValidateLuhn(CardNumber))
            {
                ErrorMessage = "Número de tarjeta inválido";
                return false;
            }

            // Validate expiry
            if (!CardValidator.ValidateExpiry(CardExpiry))
            {
                ErrorMessage = "Fecha de expiración inválida o vencida";
                return false;
            }

            // Validate CVV
            var cardBrand = CardValidator.GetCardBrand(CardNumber);
            if (!CardValidator.ValidateCvv(CardCvv, cardBrand))
            {
                ErrorMessage = "CVV inválido";
                return false;
            }

            // Validate card holder
            if (string.IsNullOrWhiteSpace(CardHolder))
            {
                ErrorMessage = "Ingrese el nombre del titular";
                return false;
            }

            return true;
        }

        private bool CanProcessPayment()
        {
            return !IsProcessing && Amount > 0 && !string.IsNullOrEmpty(SelectedPaymentMethod);
        }

        private bool CanRetry()
        {
            return !IsProcessing && !string.IsNullOrEmpty(ErrorMessage);
        }

        private void Cancel()
        {
            // Navigate back or close
        }

        public void Initialize(Order order, decimal amount)
        {
            Order = order;
            Amount = amount;
            ErrorMessage = string.Empty;
            PaymentStatus = string.Empty;
        }
    }

    public class PaymentMethodOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}
