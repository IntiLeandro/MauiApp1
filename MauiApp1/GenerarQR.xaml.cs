using SkiaSharp;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace MauiApp1;
public partial class GenerarQR : ContentPage
{
    public GenerarQR()
    {
        InitializeComponent();
    }

    private void GenerateQRCode(object sender, EventArgs e)
    {
        string url = "https://www.google.com/maps?q=40.748817,-73.985428"; // URL de Google Maps con coordenadas
        // Tamaño del código QR
        int qrCodeSize = 300;

        // Generar el código QR
        var writer = new ZXing.QrCode.QRCodeWriter();
        var bitMatrix = writer.encode(url, ZXing.BarcodeFormat.QR_CODE, qrCodeSize, qrCodeSize);

        // Crear una imagen SKBitmap con SkiaSharp
        using var bitmap = new SKBitmap(qrCodeSize, qrCodeSize);
        using var canvas = new SKCanvas(bitmap);

        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        // Dibujar los píxeles del QR
        for (int x = 0; x < qrCodeSize; x++)
        {
            for (int y = 0; y < qrCodeSize; y++)
            {
                if (bitMatrix[x, y])
                {
                    canvas.DrawPoint(x, y, paint.Color);
                }
            }
        }

        // Convertir el SKBitmap a un ImageSource para MAUI
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        //QRCodeImage.Source = ImageSource.FromStream(() => data.AsStream());
    }
}
