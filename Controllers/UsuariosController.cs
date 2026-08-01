using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.DTOs;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        private readonly PasswordService _passwordService;

        public UsuariosController(DualWriteService dual, PasswordService passwordService)
        {
            _dual = dual;
            _passwordService = passwordService;
        }

        // POST /api/Usuarios/login  (lo usa la web)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null ||
                !_passwordService.Verificar(dto.Password, usuario.PasswordHash))
            {
                return Unauthorized(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Credenciales invalidas"
                });
            }

            if (!usuario.Verificado)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Tu cuenta aun no esta verificada."
                });
            }

            return Ok(new UsuarioRespuestaDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Verificado = usuario.Verificado,
                Rol = usuario.Rol
            });
        }

        // GET /api/Usuarios  (lista para el panel admin)
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.Usuarios
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Email,
                    u.Telefono,
                    u.Activo,
                    u.Rol,
                    u.Verificado
                })
                .ToListAsync();
            return Ok(lista);
        }

        // GET /api/Usuarios/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Usuario no encontrado." });

            return Ok(new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.Telefono,
                usuario.Activo,
                usuario.Rol,
                usuario.Verificado
            });
        }

        // PUT /api/Usuarios/{id}/estado  (suspender/activar desde la web)
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoUsuarioRequest request)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Usuario no encontrado." });

            usuario.Activo = request.Activo;
            usuario.UpdatedAt = DateTime.UtcNow;
            await _dual.SaveChangesAsync();

            return Ok(new RespuestaDto
            {
                Exito = true,
                Mensaje = request.Activo ? "Usuario activado." : "Usuario suspendido."
            });
        }
    }

    public class EstadoUsuarioRequest
    {
        public bool Activo { get; set; }
    }
}
