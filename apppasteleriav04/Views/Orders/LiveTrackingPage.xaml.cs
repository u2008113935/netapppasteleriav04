using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
 using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace apppasteleriav04.Views.Orders
{
    public partial class LiveTrackingPage : ContentPage
    {
        readonly SupabaseService _supabase = SupabaseService.Instance;
        readonly Guid _orderId;
        CancellationTokenSource? _cts;
        bool _running = false;
        readonly int _pollIntervalSeconds = 5;

        Pin? _courierPin;
        Microsoft.Maui.Controls.Maps.Polyline? _routeLine;  // Especificamos explícitamente

        public LiveTrackingPage(Guid orderId)
        {
            InitializeComponent();
            _orderId = orderId;

            // Configurar posición inicial del mapa (Lima, Perú)
            var initialPosition = new Location(-12.0464, -77.0428);
            MapControl.MoveToRegion(MapSpan.FromCenterAndRadius(initialPosition, Distance.FromKilometers(5)));

            _routeLine = new Microsoft.Maui.Controls.Maps.Polyline  // Especificamos explícitamente
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 4
            };
            MapControl.MapElements.Add(_routeLine);

            Debug.WriteLine($"[LiveTrackingPage] Inicializado para orden {_orderId}");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // No iniciar automát.  esperar a que el usuario presione Iniciar
            // Opcional: iniciar automáticamente: 
            // StartPolling();
        }

        protected override void OnDisappearing()
        {
            StopPolling();
            base.OnDisappearing();
        }

        private void BtnToggle_Clicked(object sender, EventArgs e)
        {
            if (_running)
                StopPolling();
            else
                StartPolling();
        }

        void StartPolling()
        {
            if (_running) return;
            _running = true;
            BtnToggle.Text = "Detener";
            _cts = new CancellationTokenSource();
            _ = PollLoopAsync(_cts.Token);
        }

        void StopPolling()
        {
            if (!_running) return;
            _running = false;
            BtnToggle.Text = "Iniciar";
            try
            {
                _cts?.Cancel();
            }
            catch { }
            _cts = null;
        }

        async Task PollLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await RefreshOnceAsync(token);
                    await Task.Delay(TimeSpan.FromSeconds(_pollIntervalSeconds), token);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"PollLoopAsync error: {ex}");
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LblStatus.Text = "Detenido";
                });
            }
        }

        // Llamada única para forzar actualización manual
        private async void OnManualRefreshClicked(object sender, EventArgs e)
        {
            await RefreshOnceAsync(CancellationToken.None);
        }

        // Realiza una única consulta y actualiza mapa/UI
        async Task RefreshOnceAsync(CancellationToken token)
        {
            try
            {
                Debug.WriteLine($"[LiveTrakingPage] Obteniendo la orden {_orderId}...");
                //var order = await _supabase.GetOrderAsync(_orderId, token);                
                var order = await _supabase.GetOrderAsync(_orderId);
                
                if (order == null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        LblStatus.Text = "Orden no encontrada";
                    });
                    return;
                }

                Debug.WriteLine($"[LiveTrackingPage] Orden encontrada: {order.Id}, Status: {order.Status}");
                // Actualizar texto de estado
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var repartidorInfo = order.RepartidorAsignado.HasValue
                        ? $"ID: {order.RepartidorAsignado.Value.ToString().Substring(0, 8)}..."
                        : "No asignado";

                    LblStatus.Text = $"Estado: {order.Status} | Repartidor: {(order.RepartidorAsignado?.ToString() ?? "-")}";
                    LblLastUpdate.Text = $"Última actualización: {DateTime.Now:HH:mm:ss}";
                });

                
                // Actualizar pin del repartidor si hay lat/lng
                if (order.RepartidorAsignado.HasValue &&
                    order.LatitudActual.HasValue && 
                    order.LongitudActual.HasValue)
                {
                    var lat = order.LatitudActual.Value;
                    var lng = order.LongitudActual.Value;

                    Debug.WriteLine($"[LiveTrackingPage] Repartidor en:  {lat}, {lng}");

                    
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        var loc = new Location(lat, lng);

                        if (_courierPin == null)
                        {
                            _courierPin = new Pin
                            {
                                Label = "Repartidor",
                                Location = loc,
                                Type = PinType.Place
                            };
                            
                            MapControl.Pins.Add(_courierPin);
                            MapControl.MoveToRegion(MapSpan.FromCenterAndRadius(loc, Distance.FromKilometers(1)));
                        }
                        else
                        {
                            _courierPin.Location = loc;
                        }

                        // Añadir punto a polyline (ruta)
                        if (_routeLine == null)
                        {
                            _routeLine = new Microsoft.Maui.Controls.Maps.Polyline { StrokeColor = Colors.Blue, StrokeWidth = 4 };
                            MapControl.MapElements.Add(_routeLine);
                        }
                        _routeLine.Geopath.Add(loc);
                    });
                }
                else 
                {
                    Debug.WriteLine("[LiveTrackingPage] No hay repartidor asignado o sin coordenadas");

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        LblStatus.Text = $"Estado: {order.Status} | Esperando asignación de repartidor";

                        // Si no hay repartidor, mostrar ubicación por defecto (Lima, Perú)
                        var defaultLocation = new Location(-12.0464, -77.0428);
                        MapControl.MoveToRegion(MapSpan.FromCenterAndRadius(defaultLocation, Distance.FromKilometers(5)));
                    });

                }

                // Cargar histórico reciente y dibujar ruta completa
                var locs = await _supabase.GetOrderLocationsAsync(_orderId, limit: 200);
                
                if (locs != null && locs.Count > 0)
                {
                    Debug.WriteLine($"[LiveTrackingPage] {locs.Count} ubicaciones históricas encontradas");

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        // reconstruir polyline
                        _routeLine ??= new Microsoft.Maui.Controls.Maps.Polyline 
                        { 
                            StrokeColor = Colors.Blue, 
                            StrokeWidth = 4 
                        };
                        _routeLine.Geopath.Clear();

                        // Los registros vienen ordenados por registrado_en desc; invertimos para dibujar en orden cronológico
                        foreach (var ll in locs.OrderBy(l => l.RegistradoEn))
                        {
                            _routeLine.Geopath.Add(new Location(ll.Latitud, ll.Longitud));
                        }

                        // mover cámara al último punto
                        var last = locs.OrderBy(l => l.RegistradoEn).Last();
                        var lastLoc = new Location(last.Latitud, last.Longitud);
                        MapControl.MoveToRegion(MapSpan.FromCenterAndRadius(lastLoc, Distance.FromKilometers(1)));

                        // actualizar pin también con el último
                        if (_courierPin == null)
                        {
                            _courierPin = new Pin 
                            { 
                                Label = "Repartidor", 
                                Location = lastLoc, 
                                Type = PinType.Place
                            };
                            MapControl.Pins.Add(_courierPin);
                        }
                        else
                        {
                            _courierPin.Location = lastLoc;
                        }
                    });
                }
                else {                     
                    Debug.WriteLine("[LiveTrackingPage] No hay ubicaciones históricas");                
                }
            }            
            catch (Exception ex)
            {
                Debug.WriteLine($"[LiveTrackingPage] RefreshOnceAsync error: {ex.Message}");
                Debug.WriteLine($"[LiveTrackingPage] StackTrace: {ex.StackTrace}");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LblStatus.Text = $"Error: {ex.Message}";
                });
            }
        }
    }
}