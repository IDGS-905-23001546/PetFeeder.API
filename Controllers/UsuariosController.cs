using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.DTOs;
using PetFeeder.API.Models;
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

            _db.Sesiones.Add(new Sesion
            {
                UsuarioId = usuario.Id,
                Token = Guid.NewGuid().ToString("N"),
                Dispositivo = "web",
                IpOrigen = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Activa = true,
                ExpiraEn = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });
            await _dual.SaveChangesAsync();

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

        // PUT /api/Usuarios/{id}  (actualizar perfil propio: nombre, telefono)
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPerfil(int id, [FromBody] ActualizarPerfilRequest request)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Usuario no encontrado." });

            if (!string.IsNullOrWhiteSpace(request.Nombre))
                usuario.Nombre = request.Nombre.Trim();

            usuario.Telefono = string.IsNullOrWhiteSpace(request.Telefono) ? null : request.Telefono.Trim();
            usuario.UpdatedAt = DateTime.UtcNow;
            await _dual.SaveChangesAsync();

            return Ok(new UsuarioRespuestaDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Verificado = usuario.Verificado,
                Rol = usuario.Rol
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

        // PUT /api/Usuarios/{id}/rol  (convertir cliente <-> admin desde la web)
        [HttpPut("{id}/rol")]
        public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolRequest request)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Usuario no encontrado." });

            var rol = request.Rol?.Trim().ToLower();
            if (rol != "admin" && rol != "cliente")
                return BadRequest(new RespuestaDto { Exito = false, Mensaje = "El rol debe ser 'admin' o 'cliente'." });

            usuario.Rol = rol;
            usuario.UpdatedAt = DateTime.UtcNow;
            await _dual.SaveChangesAsync();

            return Ok(new RespuestaDto
            {
                Exito = true,
                Mensaje = rol == "admin" ? "Usuario convertido a administrador." : "Usuario cambiado a cliente."
            });
        }

        // PUT /api/Usuarios/{id}/verificar  (verificar cuenta manualmente desde el panel admin)
        [HttpPut("{id}/verificar")]
        public async Task<IActionResult> Verificar(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Usuario no encontrado." });

            if (usuario.Verificado)
                return Ok(new RespuestaDto { Exito = true, Mensaje = "El usuario ya estaba verificado." });

            usuario.Verificado = true;
            usuario.UpdatedAt = DateTime.UtcNow;
            await _dual.SaveChangesAsync();

            return Ok(new RespuestaDto
            {
                Exito = true,
                Mensaje = "Cuenta verificada manualmente por el administrador."
            });
        }
    }

    public class CambiarRolRequest
    {
        public string? Rol { get; set; }
    }

    public class EstadoUsuarioRequest
    {
        public bool Activo { get; set; }
    }

    public class ActualizarPerfilRequest
    {
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
    }
}
