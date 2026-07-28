using SendGrid;
using SendGrid.Helpers.Mail;

namespace PetFeeder.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarOtpAsync(string destinatario, string nombre, string codigo)
        {
            var apiKey = _config["SendGrid:ApiKey"]
                ?? Environment.GetEnvironmentVariable("SENDGRID_API_KEY");

            var fromEmail = _config["SendGrid:FromEmail"]
                ?? Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL")
                ?? "carlosriosrmz17@gmail.com";

            var fromName = _config["SendGrid:FromName"] ?? "PetFeeder";

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("SendGrid API key no configurada");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(destinatario, nombre);
            var subject = "Tu código de verificación PetFeeder";

            var htmlContent = $@"
                <div style='font-family:Arial,sans-serif; text-align:center;'>
                    <h2>Hola {nombre}</h2>
                    <p>Tu código de verificación de PetFeeder es:</p>
                    <h1 style='letter-spacing:8px; color:#2e7d32;'>{codigo}</h1>
                    <p>Este código expira en <b>10 minutos</b>.</p>
                    <p style='color:#888; font-size:12px;'>Si no fuiste tú, ignora este correo.</p>
                </div>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid error: {response.StatusCode} - {body}");
            }
        }
    }
}
