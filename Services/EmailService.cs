using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PetFeeder.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task EnviarOtpAsync(string destinatario, string nombre, string codigo)
        {
            var asunto = "Tu codigo de verificacion PetFeeder";
            var cuerpo = $@"
                <div style='font-family:Arial,sans-serif; text-align:center;'>
                    <h2>Hola {nombre}</h2>
                    <p>Tu codigo de verificacion de PetFeeder es:</p>
                    <h1 style='letter-spacing:8px; color:#2e7d32;'>{codigo}</h1>
                    <p>Este codigo expira en <b>10 minutos</b>.</p>
                    <p style='color:#888; font-size:12px;'>Si no fuiste tu, ignora este correo.</p>
                </div>";
            await EnviarAsync(destinatario, asunto, cuerpo);
        }

        public async Task EnviarCotizacionAsync(
            string correo, string contenedor, string material, int cantidad, decimal total)
        {
            static string E(string? v) => System.Net.WebUtility.HtmlEncode(v ?? string.Empty);

            var asunto = "Cotizacion PawFeeder";
            var cuerpo = $@"
                <div style='font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px'>
                  <h2 style='color:#2C5F8A;margin-bottom:8px'>Cotizacion PawFeeder</h2>
                  <p>Gracias por solicitar una cotizacion.</p>
                  <table style='border-collapse:collapse;width:100%'>
                    <tr><td style='padding:8px 0;color:#5F6E7D'><b>Modelo</b></td><td style='padding:8px 0'>PawFeeder grande - acero inoxidable</td></tr>
                    <tr><td style='padding:8px 0;color:#5F6E7D'><b>Contenedor</b></td><td style='padding:8px 0'>{E(contenedor)} (3.0 kg)</td></tr>
                    <tr><td style='padding:8px 0;color:#5F6E7D'><b>Material</b></td><td style='padding:8px 0'>{E(material)}</td></tr>
                    <tr><td style='padding:8px 0;color:#5F6E7D'><b>Cantidad</b></td><td style='padding:8px 0'>{cantidad}</td></tr>
                    <tr><td style='padding:12px 0;color:#1E3A5C;font-size:18px'><b>Total estimado</b></td><td style='padding:12px 0;font-size:18px;color:#2C5F8A'><b>$ {total:N2} MXN</b></td></tr>
                  </table>
                  <p style='color:#5F6E7D;font-size:14px;margin-top:20px'>Correo de contacto: {E(correo)}</p>
                  <p style='color:#5F6E7D;font-size:14px'>Gracias por confiar en <b>PawFeeder</b>.</p>
                </div>";
            await EnviarAsync(correo, asunto, cuerpo);
        }

        private async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            var resendApiKey = _config["Resend:ApiKey"];
            var brevoApiKey = _config["Brevo:ApiKey"];
            var smtpHost = _config["EmailSettings:SmtpHost"];

            var resendConfigurado = !string.IsNullOrWhiteSpace(resendApiKey);
            var brevoConfigurado = !string.IsNullOrWhiteSpace(brevoApiKey);
            var smtpConfigurado = !string.IsNullOrWhiteSpace(smtpHost);

            Exception? ultimoError = null;

            if (resendConfigurado)
            {
                try
                {
                    await EnviarConResendAsync(destinatario, asunto, cuerpoHtml, resendApiKey!);
                    return;
                }
                catch (Exception ex) when (brevoConfigurado || smtpConfigurado)
                {
                    Console.WriteLine($"[WARNING] Resend fallo, probando otro proveedor: {ex.Message}");
                    ultimoError = ex;
                }
            }

            if (brevoConfigurado)
            {
                try
                {
                    await EnviarConBrevoAsync(destinatario, asunto, cuerpoHtml, brevoApiKey!);
                    return;
                }
                catch (Exception ex) when (smtpConfigurado)
                {
                    Console.WriteLine($"[WARNING] Brevo fallo, probando SMTP: {ex.Message}");
                    ultimoError = ex;
                }
            }

            if (smtpConfigurado)
            {
                await EnviarConSmtpAsync(destinatario, asunto, cuerpoHtml);
                return;
            }

            if (ultimoError != null)
                throw ultimoError;

            if (resendConfigurado)
            {
                await EnviarConResendAsync(destinatario, asunto, cuerpoHtml, resendApiKey!);
                return;
            }

            if (brevoConfigurado)
            {
                await EnviarConBrevoAsync(destinatario, asunto, cuerpoHtml, brevoApiKey!);
                return;
            }

            throw new InvalidOperationException(
                "No hay servicio de correo configurado. Configura Brevo (Brevo__ApiKey), Resend (Resend__ApiKey) o SMTP (EmailSettings__SmtpHost).");
        }

        private async Task EnviarConResendAsync(string destinatario, string asunto, string cuerpoHtml, string apiKey)
        {
            var fromEmail = _config["Resend:FromEmail"] ?? "";
            var fromName = _config["Resend:FromName"] ?? "PetFeeder";

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new InvalidOperationException("Resend no configurado: falta Resend:FromEmail.");

            using var http = _httpClientFactory.CreateClient("resend");
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());

            var payload = new
            {
                from = $"{fromName} <{fromEmail.Trim()}>",
                to = new[] { destinatario },
                subject = asunto,
                html = cuerpoHtml
            };

            var contenido = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var resp = await http.PostAsync("https://api.resend.com/emails", contenido);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Resend respondio {(int)resp.StatusCode}: {body}");
        }

        private async Task EnviarConBrevoAsync(string destinatario, string asunto, string cuerpoHtml, string apiKey)
        {
            var fromEmail = (_config["Brevo:FromEmail"] ?? _config["EmailSettings:FromEmail"] ?? _config["Resend:FromEmail"] ?? "").Trim();
            var fromName = (_config["Brevo:FromName"] ?? _config["EmailSettings:FromName"] ?? _config["Resend:FromName"] ?? "PetFeeder").Trim();

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new InvalidOperationException("Brevo no configurado: falta Brevo:FromEmail.");

            using var http = _httpClientFactory.CreateClient("brevo");
            http.DefaultRequestHeaders.Remove("api-key");
            http.DefaultRequestHeaders.Add("api-key", apiKey.Trim());
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                sender = new { name = fromName, email = fromEmail },
                to = new[] { new { email = destinatario } },
                subject = asunto,
                htmlContent = cuerpoHtml
            };

            var contenido = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var resp = await http.PostAsync("https://api.brevo.com/v3/smtp/email", contenido);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Brevo respondio {(int)resp.StatusCode}: {body}");
        }

        private async Task EnviarConSmtpAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            var host = _config["EmailSettings:SmtpHost"]?.Trim() ?? "";
            var portStr = _config["EmailSettings:SmtpPort"];
            var fromEmail = (_config["EmailSettings:FromEmail"] ?? _config["EmailSettings:From"] ?? "").Trim();
            var fromName = (_config["EmailSettings:FromName"] ?? "PetFeeder").Trim();
            var username = (_config["EmailSettings:Username"] ?? fromEmail).Trim();
            var password = (_config["EmailSettings:AppPassword"] ?? _config["EmailSettings:Password"] ?? "")
                .Replace(" ", string.Empty);

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("SMTP no configurado: faltan EmailSettings:SmtpHost, FromEmail o AppPassword.");

            var port = int.TryParse(portStr, out var p) ? p : 587;

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(fromName, fromEmail));
            email.To.Add(MailboxAddress.Parse(destinatario));
            email.Subject = asunto;
            email.Body = new TextPart("html") { Text = cuerpoHtml };

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            var secure = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
            await smtp.ConnectAsync(host, port, secure);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
