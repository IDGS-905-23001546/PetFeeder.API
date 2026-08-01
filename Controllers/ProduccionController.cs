using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProduccionController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public ProduccionController(DualWriteService dual) { _dual = dual; }

        // POST /api/Produccion/fabricar-dispensador/{productoId}?cantidadAFabricar=1
        [HttpPost("fabricar-dispensador/{productoId}")]
        public async Task<IActionResult> Fabricar(int productoId, int cantidadAFabricar = 1)
        {
            if (cantidadAFabricar <= 0)
                return BadRequest("La cantidad a fabricar debe ser mayor a 0.");

            var producto = await _db.InventarioProductos.FindAsync(productoId);
            if (producto == null)
                return NotFound("Producto no encontrado.");

            var receta = await _db.RecetasProducto
                .Where(r => r.ProductoId == productoId)
                .Include(r => r.Componente)
                .ToListAsync();

            if (receta.Count == 0)
                return BadRequest("El producto no tiene receta de produccion definida.");

            // 1. Verificar stock de componentes
            foreach (var item in receta)
            {
                int requeridos = item.CantidadRequerida * cantidadAFabricar;
                if (item.Componente == null || item.Componente.Stock < requeridos)
                {
                    var nombre = item.Componente?.Nombre ?? "?";
                    return BadRequest($"No hay suficiente stock del componente '{nombre}'.");
                }
            }

            try
            {
                using var tx = await _db.Database.BeginTransactionAsync();

                // 2. Descontar componentes
                foreach (var item in receta)
                {
                    if (item.Componente != null)
                        item.Componente.Stock -= item.CantidadRequerida * cantidadAFabricar;
                }

                // 3. Aumentar stock del producto
                producto.Stock += cantidadAFabricar;

                // 4. Crear una unidad de inventario por dispensador fabricado
                var ahora = DateTime.Now;
                for (int i = 0; i < cantidadAFabricar; i++)
                {
                    _db.DispensadoresInventario.Add(new DispensadorInventario
                    {
                        ProductoId = productoId,
                        CodigoUnico = $"PF-{producto.Id}-{ahora:yyyyMMddHHmmss}-{i + 1}",
                        Estado = "Pendiente",
                        CreadoEn = ahora
                    });
                }

                await _dual.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new
                {
                    mensaje = $"┬íEnsamblaje exitoso! Se produjeron {cantidadAFabricar} dispensador(es)."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR Produccion] {ex}");
                return StatusCode(500, "Error durante el ensamblaje. Intenta de nuevo.");
            }
        }
    }
}
