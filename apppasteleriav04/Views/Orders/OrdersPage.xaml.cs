using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Orders;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Linq;

namespace apppasteleriav04.Views.Orders
{
    public partial class OrdersPage : ContentPage
    {
        private readonly OrdersViewModel _viewModel;

        public OrdersPage()
        {
            InitializeComponent();
            _viewModel = new OrdersViewModel();
            BindingContext = _viewModel;
            
            // Optional: fill Picker with status options
            StatusPicker.ItemsSource = new System.Collections.Generic.List<string> 
            { 
                "Todos", "pendiente", "en_preparacion", "en_reparto", "entregado", "cancelado" 
            };
            StatusPicker.SelectedIndex = 0;
        }

        
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            System.Diagnostics.Debug.WriteLine("[OrdersPage] OnAppearing - Cargando órdenes...");

            await _viewModel.LoadOrdersAsync(); //Cargar órdenes al aparecer la página
            
            System.Diagnostics.Debug.WriteLine("[OrdersPage] Pedidos cagados: { _viewModel.Orders.Count}");
        }

        //Cancelar operaciones en curso al desaparecer la página
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.CancelOperations();
        }

        // Seleccion de orden en la colección
        private async void OrdersCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel = e.CurrentSelection;
            if (sel == null || sel.Count == 0) return;

            if (sel[0] is Order order)
            {
                // Deselect for UX
                ((CollectionView)sender).SelectedItem = null;

                // Navigate to details or tracking page
                try
                {
                    try
                    {
                        await Navigation.PushAsync(new LiveTrackingPage(order.Id));
                    }
                    catch (Exception navEx)
                    {
                        Debug.WriteLine($"Navigation to LiveTrackingPage failed: {navEx}");
                        // Fallback: show basic info
                        await DisplayAlert("Pedido", $"Id: {order.Id}\nTotal: S/ {order.Total:N2}\nEstado: {order.Status}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"OrdersCollection_SelectionChanged error: {ex}");
                }
            }
        }

        // View/Track button handler
        private async void OnViewOrTrackClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Guid orderId)
            {
                try
                {
                    // Find order in the list
                    var order = _viewModel.Orders?.FirstOrDefault(o => o.Id == orderId);
                    
                    if (order == null)
                    {
                        await DisplayAlert("Error", "Orden no encontrada.", "OK");
                        return;
                    }

                    // Navigate to LiveTrackingPage
                    try
                    {
                        await Navigation.PushAsync(new LiveTrackingPage(order.Id));
                    }
                    catch
                    {
                        // If LiveTrackingPage doesn't exist, show minimal details
                        await DisplayAlert("Pedido", $"Id: {order.Id}\nTotal: S/ {order.Total:N2}\nEstado: {order.Status}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"OnViewOrTrackClicked error: {ex}");
                    await DisplayAlert("Error", "No se pudo abrir la orden.", "OK");
                }
            }
        }
    }
}