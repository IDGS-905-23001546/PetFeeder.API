namespace PetFeeder.API.DTOs
{

    // POST /api/auth/registro
    public class RegistroDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    // POST /api/auth/login
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // POST /api/auth/verificar
    public class VerificarOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }

    // POST /api/auth/reenviar
    public class ReenviarOtpDto
    {
        public string Email { get; set; } = string.Empty;
    }

    // SALIDAS (lo que la API RESPONDE) 

    // Respuesta genérica con mensaje (registro, verificación, errores)
    public class RespuestaDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    // Respuesta del login exitoso (datos del usuario, SIN password)
    public class UsuarioRespuestaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Verificado { get; set; }
    }
}