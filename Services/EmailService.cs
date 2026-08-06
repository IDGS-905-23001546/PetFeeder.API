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
            var asunto = "Cotizacion PetFeeder";
            var cuerpo = $@"
                    <div style='font-family:Arial,sans-serif;'>
                        <h2>Solicitud de cotizacion</h2>
                        <table style='border-collapse:collapse;'>
                            <tr><td style='padding:6px;border:1px solid #ccc;'>Contenedor</td><td style='padding:6px;border:1px solid #ccc;'>{contenedor}</td></tr>
                            <tr><td style='padding:6px;border:1px solid #ccc;'>Material</td><td style='padding:6px;border:1px solid #ccc;'>{material}</td></tr>
                            <tr><td style='padding:6px;border:1px solid #ccc;'>Cantidad</td><td style='padding:6px;border:1px solid #ccc;'>{cantidad}</td></tr>
                            <tr><td style='padding:6px;border:1px solid #ccc;'>Total</td><td style='padding:6px;border:1px solid #ccc;'>${total:N2} MXN</td></tr>
                        </table>
                        <p>Responder a: {correo}</p>
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
