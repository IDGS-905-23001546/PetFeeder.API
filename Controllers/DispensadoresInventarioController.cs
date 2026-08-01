using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DispensadoresInventarioController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public DispensadoresInventarioController(DualWriteService dual) { _dual = dual; }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.DispensadoresInventario
                .Include(d => d.Producto)
                .OrderByDescending(d => d.CreadoEn)
                .Select(d => new
                {
                    d.Id,
                    d.ProductoId,
                    ProductoNombre = d.Producto != null ? d.Producto.Nombre : "",
                    d.CodigoUnico,
                    d.Estado,
                    d.CreadoEn
                })
                .ToListAsync();
            return Ok(lista);
        }

        [HttpGet("conteo-terminados")]
        public async Task<IActionResult> ConteoTerminados()
        {
            int total = await _db.DispensadoresInventario
                .CountAsync(d => d.Estado == "Terminado");
            return Ok(new { total });
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoProductoRequest request)
        {
            var d = await _db.DispensadoresInventario.FindAsync(id);
            if (d == null)
                return NotFound("Dispensador no encontrado.");

            d.Estado = request.Estado;
            await _dual.SaveChangesAsync();
            return Ok(new { mensaje = $"Estado actualizado a '{request.Estado}'." });
        }
    }
}
