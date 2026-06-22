using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PetFeeder.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        // Se inyecta la configuración para leer la sección "EmailSettings"
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // Envía el correo con el código OTP al usuario
        public async Task EnviarOtpAsync(string destinatario, string nombre, string codigo)
        {
            // 1. Leer los datos de "EmailSettings" del appsettings.json
            var settings = _config.GetSection("EmailSettings");
            var host = settings["SmtpHost"];
            var port = int.Parse(settings["SmtpPort"]!);
            var fromEmail = settings["FromEmail"];
            var fromName = settings["FromName"];
            var appPassword = settings["AppPassword"];

            // 2. Construir el mensaje
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(fromName, fromEmail));
            mensaje.To.Add(new MailboxAddress(nombre, destinatario));
            mensaje.Subject = "Tu código de verificación PetFeeder";

            mensaje.Body = new TextPart("html")
            {
                Text = $@"
                      <div style='font-family:Arial,sans-serif; text-align:center;'>
                          <h2>Hola {nombre} 👋</h2>
                          <p>Tu código de verificación de PetFeeder es:</p>
                          <h1 style='letter-spacing:8px; color:#2e7d32;'>{codigo}</h1>
                          <p>Este código expira en <b>5 minutos</b>.</p>
                          <p style='color:#888; font-size:12px;'>Si no fuiste tú, ignora este correo.</p>
                      </div>"
            };

            // 3. Conectar a Gmail, autenticar y enviar
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(fromEmail, appPassword);
            await client.SendAsync(mensaje);
            await client.DisconnectAsync(true);
        }
    }
}