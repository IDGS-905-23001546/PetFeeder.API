using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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

            var asunto = "Cotización PawFeeder";
            var cuerpo = $@"
                <div style='font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px'>
                  <h2 style='color:#2C5F8A;margin-bottom:8px'>Cotización PawFeeder</h2>
                  <p>Gracias por solicitar una cotización.</p>
                  <table style='border-collapse:collapse;width:100%'>
                    <tr><td style='padding:8px 0;color:#5F6E7D'><b>Modelo</b></td><td style='padding:8px 0'>PawFeeder grande · acero inoxidable</td></tr>
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
            var apiKey = _config["Resend:ApiKey"] ?? "";
            var fromEmail = _config["Resend:FromEmail"] ?? "";
            var fromName = _config["Resend:FromName"] ?? "PetFeeder";

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
                throw new Exception("Resend no configurado (Resend:ApiKey o Resend:FromEmail faltan).");

            using var http = _httpClientFactory.CreateClient("resend");
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from = $"{fromName} <{fromEmail}>",
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
                throw new Exception($"Resend respondio {resp.StatusCode}: {body}");
        }
    }
}
