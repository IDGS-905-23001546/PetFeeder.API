using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecetasController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public RecetasController(DualWriteService dual) { _dual = dual; }

        // GET /api/Recetas/productos-con-receta
        [HttpGet("productos-con-receta")]
        public async Task<IActionResult> ProductosConReceta()
        {
            var productos = await _db.InventarioProductos
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Stock,
                    p.Estado,
                    ComponentesCount = p.Recetas == null ? 0 : p.Recetas.Count
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();
            return Ok(productos);
        }

        // GET /api/Recetas/producto/{productoId}
        [HttpGet("producto/{productoId}")]
        public async Task<IActionResult> PorProducto(int productoId)
        {
            var items = await _db.RecetasProducto
                .Where(r => r.ProductoId == productoId)
                .Include(r => r.Componente)
                .Select(r => new
                {
                    r.Id,
                    r.ProductoId,
                    r.ComponenteId,
                    ComponenteNombre = r.Componente != null ? r.Componente.Nombre : "",
                    r.CantidadRequerida,
                    r.Dispensador
                })
                .ToListAsync();
            return Ok(items);
        }

        // POST /api/Recetas
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] RecetaRequest request)
        {
            if (request.CantidadRequerida <= 0)
                return BadRequest("La cantidad requerida debe ser mayor a 0.");

            bool existe = await _db.RecetasProducto
                .AnyAsync(r => r.ProductoId == request.ProductoId && r.ComponenteId == request.ComponenteId);
            if (existe)
                return BadRequest("Ese componente ya esta en la receta del producto.");

            var receta = new RecetaProducto
            {
                ProductoId = request.ProductoId,
                ComponenteId = request.ComponenteId,
                CantidadRequerida = request.CantidadRequerida,
                Dispensador = request.Dispensador
            };
            _db.RecetasProducto.Add(receta);
            await _dual.SaveChangesAsync();
            return Ok(receta);
        }

        // PUT /api/Recetas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] RecetaRequest request)
        {
            var r = await _db.RecetasProducto.FindAsync(id);
            if (r == null)
                return NotFound("Receta no encontrada.");
            if (request.CantidadRequerida <= 0)
                return BadRequest("La cantidad requerida debe ser mayor a 0.");

            r.CantidadRequerida = request.CantidadRequerida;
            r.Dispensador = request.Dispensador;
            await _dual.SaveChangesAsync();
            return Ok(r);
        }

        // DELETE /api/Recetas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var r = await _db.RecetasProducto.FindAsync(id);
            if (r == null)
                return NotFound("Receta no encontrada.");

            _db.RecetasProducto.Remove(r);
            await _dual.SaveChangesAsync();
            return NoContent();
        }
    }

    public class RecetaRequest
    {
        public int ProductoId { get; set; }
        public int ComponenteId { get; set; }
        public int CantidadRequerida { get; set; }
        public string? Dispensador { get; set; }
    }
}
