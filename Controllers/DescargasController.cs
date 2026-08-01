using Microsoft.AspNetCore.Mvc;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    public class DescargasController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public DescargasController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // GET /DownloadApp  (fuera de /api, igual que PawFeeder)
        [HttpGet("/DownloadApp")]
        public IActionResult DescargarApk()
        {
            var ruta = Path.Combine(_env.WebRootPath ?? "wwwroot", "downloads", "pawfeeder.apk");
            if (!System.IO.File.Exists(ruta))
                return NotFound("El archivo de la app no esta disponible.");

            var bytes = System.IO.File.ReadAllBytes(ruta);
            return File(bytes, "application/vnd.android.package-archive", "PawFeeder.apk");
        }
    }
}
