namespace PetFeeder.API.Services
{
    public class PasswordService
    {
        // Convierte la contraseña en texto plano a un hash seguro (para guardar en BD)
        public string Encriptar(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Compara la contraseña que escribió el usuario contra el hash guardado
        public bool Verificar(string password, string hashGuardado)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashGuardado);
        }
    }
}