using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosProveedorController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public PedidosProveedorController(DualWriteService dual) { _dual = dual; }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.PedidosProveedor
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] PedidoProveedor dto)
        {
            if (dto.ProveedorId <= 0) return BadRequest("Selecciona un proveedor válido.");
            if (dto.ComponenteId <= 0) return BadRequest("Selecciona un componente válido.");
            if (dto.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a 0.");

            var proveedor = await _db.Proveedores.FindAsync(dto.ProveedorId);
            if (proveedor == null) return BadRequest("El proveedor no existe.");

            var componente = await _db.Componentes.FindAsync(dto.ComponenteId);
            if (componente == null) return BadRequest("El componente no existe.");

            dto.Id = 0;
            var ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            dto.Folio = string.IsNullOrWhiteSpace(dto.Folio)
                ? $"PF-{ms[^Math.Min(8, ms.Length)..]}"
                : dto.Folio.Trim();
            dto.ProveedorNombre = proveedor.Nombre;
            dto.ComponenteNombre = componente.Nombre;
            dto.UnidadMedida = string.IsNullOrWhiteSpace(componente.UnidadMedida) ? "pza" : componente.UnidadMedida;
            dto.Fecha = DateTime.Now;
            dto.Estado = "pendiente";
            dto.FechaRecibido = null;

            _db.PedidosProveedor.Add(dto);
            await _dual.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpPut("{id}/recibir")]
        public async Task<IActionResult> MarcarRecibido(int id)
        {
            var pedido = await _db.PedidosProveedor.FindAsync(id);
            if (pedido == null) return NotFound("El pedido no existe.");
            if (pedido.Estado == "recibido") return BadRequest("El pedido ya fue marcado como recibido.");

            var componente = await _db.Componentes.FindAsync(pedido.ComponenteId);
            if (componente != null)
            {
                componente.Stock += pedido.Cantidad;
            }

            pedido.Estado = "recibido";
            pedido.FechaRecibido = DateTime.Now;
            await _dual.SaveChangesAsync();
            return Ok(pedido);
        }
    }
}
