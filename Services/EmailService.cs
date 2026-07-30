using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PetFeeder.API.Services
{
    public class EmailService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public EmailService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task EnviarOtpAsync(string destinatario, string nombre, string codigo)
        {
            var apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY")
                ?? _config["Resend:ApiKey"]
                ?? "";

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("RESEND_API_KEY no configurada");

            var from = _config["Resend:FromEmail"] ?? "onboarding@resend.dev";
            var fromName = _config["Resend:FromName"] ?? "PetFeeder";

            var body = new
            {
                from = $"{fromName} <{from}>",
                to = new[] { destinatario },
                subject = "Tu codigo de verificacion PetFeeder",
                html = $@"
                    <div style='font-family:Arial,sans-serif; text-align:center;'>
                        <h2>Hola {nombre}</h2>
                        <p>Tu codigo de verificacion de PetFeeder es:</p>
                        <h1 style='letter-spacing:8px; color:#2e7d32;'>{codigo}</h1>
                        <p>Este codigo expira en <b>10 minutos</b>.</p>
                        <p style='color:#888; font-size:12px;'>Si no fuiste tu, ignora este correo.</p>
                    </div>"
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = content;

            var response = await _http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Resend error: {response.StatusCode} - {responseBody}");
        }
    }
}
