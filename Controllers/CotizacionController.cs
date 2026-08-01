using Microsoft.AspNetCore.Mvc;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionController : ControllerBase
    {
        private readonly EmailService _emailService;

        public CotizacionController(EmailService emailService)
        {
            _emailService = emailService;
        }

        // POST /api/Cotizacion/enviar
        [HttpPost("enviar")]
        public async Task<IActionResult> Enviar([FromBody] CotizacionRequest request)
        {
            try
            {
                await _emailService.EnviarCotizacionAsync(
                    request.Correo, request.Contenedor, request.Material,
                    request.Cantidad, request.Total);

                return Ok(new { mensaje = "Correo enviado correctamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Cotizacion no enviada: {ex.Message}");
                return BadRequest(new { mensaje = "No se pudo enviar el correo. Intenta de nuevo." });
            }
        }
    }

    public class CotizacionRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Contenedor { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}
