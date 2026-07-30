using System.Net;
using System.Net.Mail;

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
            var smtpHost = _config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
            var fromEmail = _config["EmailSettings:FromEmail"] ?? "";
            var fromName = _config["EmailSettings:FromName"] ?? "PetFeeder";
            var appPassword = _config["EmailSettings:AppPassword"] ?? "";

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(appPassword))
                throw new Exception("EmailSettings no configurado en appsettings.json");

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, appPassword)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = "Tu codigo de verificacion PetFeeder",
                IsBodyHtml = true,
                Body = $@"
                    <div style='font-family:Arial,sans-serif; text-align:center;'>
                        <h2>Hola {nombre}</h2>
                        <p>Tu codigo de verificacion de PetFeeder es:</p>
                        <h1 style='letter-spacing:8px; color:#2e7d32;'>{codigo}</h1>
                        <p>Este codigo expira en <b>10 minutos</b>.</p>
                        <p style='color:#888; font-size:12px;'>Si no fuiste tu, ignora este correo.</p>
                    </div>"
            };
            mail.To.Add(destinatario);

            await smtp.SendMailAsync(mail);
        }
    }
}
