using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MauiApp1;
public partial class LoginPage : ContentPage
{
    private const string ConnectionString = "Server=192.168.100.9,1433;Database=prueba3;User Id=inti;Password=1234;Encrypt=False;";

    //private const string ConnectionString = "Server=192.168.100.9,1433;Database=DBDAP;User Id=inti;Password=1234;Encrypt=False;";
    //aspnet-SysDAP-ecd8419e-c4da-4598-8a5f-94264ca48313
    public LoginPage()
	{
		InitializeComponent();
	}
    public static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();

            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }

    public bool VerifySHA256Password(string enteredPassword, string storedHash)
    {
        using (var sha256 = MD5.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
            var hashedPassword = Convert.ToBase64String(hashedBytes);
            return hashedPassword == storedHash;
        }
    }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorLabel.Text = "Por favor, completa todos los campos.";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                // Consulta para obtener el PasswordHash
                string query = "SELECT ContraseñaHash, EmpleadoID FROM UsuariosApp WHERE NombreUsuario = @UserName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserName", username);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string storedHash = reader["ContraseñaHash"].ToString(); // Obtener ContraseñaHash
                            int empleadoId = Convert.ToInt32(reader["EmpleadoID"]); // Obtener EmpleadoID

                            // Verificar la contraseña usando el PasswordHasher
                            var passwordHasher = new PasswordHasher<object>();
                            var verificationResult = passwordHasher.VerifyHashedPassword(null, storedHash, password);

                            if (verificationResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
                            {
                                await DisplayAlert("Inicio de Sesión", "¡Inicio de sesión exitoso!", "OK");

                                // Redirigir a otra página y enviar el EmpleadoID
                                await Navigation.PushAsync(new MainPage(empleadoId));
                            }
                            else
                            {
                                ErrorLabel.Text = "Contraseña incorrecta.";
                                ErrorLabel.IsVisible = true;
                            }
                        }
                        else
                        {
                            ErrorLabel.Text = "Usuario no encontrado.";
                            ErrorLabel.IsVisible = true;
                        }
                    }

                    
                    //var result = await command.ExecuteScalarAsync();

                    //if (result != null)
                    //{
                    //    string storedHash = result.ToString();


                    //    var passwordHasher = new PasswordHasher<object>();
                    //    var verificationResult = passwordHasher.VerifyHashedPassword(null, storedHash, password);

                    //    if (verificationResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
                    //    {
                    //        await DisplayAlert("Inicio de Sesión", "¡Inicio de sesión exitoso!", "OK");

                    //        // Redirigir a otra página
                    //        await Navigation.PushAsync(new MainPage("3921902"));
                    //    }
                    //    else
                    //    {
                    //        ErrorLabel.Text = "Contraseña incorrecta.";
                    //        ErrorLabel.IsVisible = true;
                    //    }
                    //}
                    //else
                    //{
                    //    ErrorLabel.Text = "Usuario no encontrado.";
                    //    ErrorLabel.IsVisible = true;
                    //}
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            ErrorLabel.Text = "Error al conectar con la base de datos.";
            ErrorLabel.IsVisible = true;
        }
    }
}