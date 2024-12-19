namespace MauiApp1;
using System.Net.Http.Json;
using System.Data.SqlClient;

public partial class CambiarContraseñaPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    private const string connectionString = "Server=192.168.100.9,1433;Database=prueba3;User Id=inti;Password=1234;Encrypt=False;";

    public CambiarContraseñaPage()
	{
		InitializeComponent();
	}

    private async void OnCambiarContraseñaClicked(object sender, EventArgs e)
    {
        string nombreUsuario = UsernameEntry.Text;
        string contraseñaActual = ContraseñaActualEntry.Text;
        string nuevaContraseña = NuevaContraseñaEntry.Text;

        // Validación básica
        if (string.IsNullOrWhiteSpace(nombreUsuario) ||
            string.IsNullOrWhiteSpace(contraseñaActual) ||
            string.IsNullOrWhiteSpace(nuevaContraseña))
        {
            await DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
            return;
        }

        try
        {
            // Actualización en la base de datos
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                // Verificar si la contraseña actual es correcta
                string queryVerificar = "SELECT Contraseña FROM usuariosapp WHERE NombreUsuario = @NombreUsuario";
                SqlCommand cmdVerificar = new SqlCommand(queryVerificar, conn);
                cmdVerificar.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

                string contraseñaActualDB = (string)await cmdVerificar.ExecuteScalarAsync();

                if (contraseñaActualDB != contraseñaActual) // Comparar contraseñas (sin hash en este ejemplo)
                {
                    await DisplayAlert("Error", "La contraseña actual es incorrecta.", "OK");
                    return;
                }

                // Actualizar la contraseña (guardada directamente)
                string queryActualizar = "UPDATE usuariosapp SET Contraseña = @NuevaContraseña WHERE NombreUsuario = @NombreUsuario";
                SqlCommand cmdActualizar = new SqlCommand(queryActualizar, conn);
                cmdActualizar.Parameters.AddWithValue("@NuevaContraseña", nuevaContraseña); // Aquí debes hashear
                cmdActualizar.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

                int filasAfectadas = await cmdActualizar.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    await DisplayAlert("Éxito", "Contraseña actualizada correctamente.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo actualizar la contraseña.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }
}