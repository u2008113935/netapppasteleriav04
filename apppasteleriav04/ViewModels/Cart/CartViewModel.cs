using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Cart
{
    public class CartViewModel : BaseViewModel
    {
        private readonly CartService _cartService;

        public ObservableCollection<CartItem> Items => _cartService.Items;

        public decimal Total => _cartService.Total;

        public int Count => _cartService.Count;

        public bool IsEmpty => Count == 0;

        public ICommand IncreaseCommand { get; }
        public ICommand DecreaseCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand CheckoutCommand { get; }

        public event EventHandler? CheckoutRequested;

        public CartViewModel()
        {
            _cartService = CartService.Instance;
            Title = "Carrito de Compras";

            IncreaseCommand = new RelayCommand<CartItem>(IncreaseQuantity);
            DecreaseCommand = new RelayCommand<CartItem>(DecreaseQuantity);
            RemoveCommand = new RelayCommand<CartItem>(RemoveItem);
            ClearCommand = new RelayCommand(ClearCart);
            CheckoutCommand = new RelayCommand(OnCheckout, () => !IsEmpty);

            _cartService.CartChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(IsEmpty));
            };
        }

        private void IncreaseQuantity(CartItem? item)
        {
            if (item != null)
            {
                item.Quantity++;
            }
        }

        private void DecreaseQuantity(CartItem? item)
        {
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
            }
        }

        private void RemoveItem(CartItem? item)
        {
            if (item != null)
            {
                _cartService.Remove(item);
            }
        }

        private void ClearCart()
        {
            _cartService.Clear();
        }

        private void OnCheckout()
        {
            CheckoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
