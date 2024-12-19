using System.Text.RegularExpressions;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using ZXing.QrCode.Internal;
using Microsoft.Data.SqlClient;

namespace MauiApp1;

public partial class EntradaPage : ContentPage
{
    int EmpleadoId = 0;
	public EntradaPage(int idEmpleado)
	{
        EmpleadoId = idEmpleado;
		InitializeComponent();
	}

    private async void EntradaCameraBarcodeReaderView_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        //Detener el escaneo mientras procesamos
        EntradaCameraBarcodeReaderView.IsDetecting = false;
        string url = "";
        try
        {
            // Procesar QR
            MainThread.BeginInvokeOnMainThread(async () =>//await MainThread.InvokeOnMainThreadAsync(() =>
            {
                string qrContent = e.Results[0].Value;
                //EntradaResultLabel.Text = $"QR escaneado: {qrContent}";
                try
                {
                    ////Validar si es una URL válida
                    //    if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
                    //        (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                    //{
                    //    // Redirigir al navegador
                    //    await Launcher.OpenAsync(uriResult);
                    //}
                    //else
                    //{
                    //    // Mostrar mensaje si el QR no es una URL
                    //    await DisplayAlert("Código QR Detectado", $"Contenido: {url}", "OK");
                    //}

                    url = qrContent;
                    if (!string.IsNullOrEmpty(url))
                    {
                        bool isValid = await ValidateQRCodeLocationAsync(url);
                        if (isValid)
                        {
                            //Aquí deberías obtener el código del empleado(puedes pedirlo previamente)                            
                            int employeeId = EmpleadoId; // Simulación

                            // Crear el registro
                            var record = new Marcacion
                            {
                                idEmpleado = employeeId,
                                fecha = DateTime.Now.Date,
                                entrada = DateTime.Now.Date
                                //qrCode = locationFromQr
                            };

                            //Guardar el registro  
                            await SaveToSqlServer(record);
                            //StatusLabel.Text = "Marcación registrada exitosamente.";

                            DisplayAlert("Mensaje", "Marcación registrada exitosamente.", "OK");
                        }
                        else
                        {
                            DisplayAlert("Error", "La ubicación del QR no coincide con la ubicación actual.", "Reintentar");
                        }
                    }

                    //var (latitude, longitude) = ExtractCoordinatesFromGoogleMapsUrl(qrContent);

                    //DisplayAlert("QR Escaneado", $"Latitud: {latitude}, Longitud: {longitude}", "OK");
                }
                catch (Exception)
                {
                    DisplayAlert("Error", "No se pudieron extraer coordenadas del QR.", "OK");
                }
            });
        }
        finally
        {
            await Task.Delay(5000); // Evitar escaneos inmediatos
            EntradaCameraBarcodeReaderView.IsDetecting = true;
        }
        
    }

    public async Task<bool> ValidateQRCodeLocationAsync(string url)
    {
        try
        {
            // Escanear el QR
            string qrUrl = url;//await ScanQRCodeAsync();

            // Extraer las coordenadas del QR
            var (qrLat, qrLon) = ExtractCoordinatesFromGoogleMapsUrl(qrUrl);

            // Obtener la ubicación actual
            var currentLocation = await GetCurrentLocationAsync();
            if (currentLocation != null)
            {
                // Comparar ubicaciones
                return AreLocationsClose(currentLocation.Latitude, currentLocation.Longitude, qrLat, qrLon);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validando ubicación: {ex.Message}");
        }
        return false;
    }
    public (double Latitude, double Longitude) ExtractCoordinatesFromGoogleMapsUrl(string url)
    {
        var regex = new Regex(@"q=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)");
        var match = regex.Match(url);
        if (match.Success)
        {
            double latitude = double.Parse(match.Groups[1].Value);
            double longitude = double.Parse(match.Groups[2].Value);
            return (latitude, longitude);
        }
        throw new FormatException("La URL del QR no contiene coordenadas válidas.");
    }

    public async Task<Location> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);
            return location;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo ubicación: {ex.Message}");
            return null;
        }
    }

    public bool AreLocationsClose(double currentLat, double currentLon, double qrLat, double qrLon, double thresholdMeters = 50)
    {
        const double EarthRadius = 6371000; // Radio de la Tierra en metros
        double dLat = (qrLat - currentLat) * Math.PI / 180;
        double dLon = (qrLon - currentLon) * Math.PI / 180;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(currentLat * Math.PI / 180) * Math.Cos(qrLat * Math.PI / 180) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = EarthRadius * c; // Distancia en metros

        return distance <= thresholdMeters;
    }

    private const string ConnectionString = "Server=192.168.100.9,1433;Database=prueba3;User Id=inti;Password=1234;Encrypt=False;";

    private async Task SaveToSqlServer(Marcacion record)
    {
        using var connection = new SqlConnection(ConnectionString);

        try
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                @"INSERT INTO marcaciones (EmpleadoId, Fecha, HoraEntrada)
          VALUES (@idEmpleado, GETDATE(), GETDATE())";
            command.Parameters.AddWithValue("@idEmpleado", record.idEmpleado);
            //command.Parameters.AddWithValue("@fecha", record.fecha);
            //command.Parameters.AddWithValue("@entrada", record.entrada);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {

            throw ex;
        }

    }
}