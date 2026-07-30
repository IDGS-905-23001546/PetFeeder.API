using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("sesiones")]
    public class Sesion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("token")]
        public string Token { get; set; } = string.Empty;

        [Column("dispositivo")]
        public string? Dispositivo { get; set; }

        [Column("ip_origen")]
        public string? IpOrigen { get; set; }

        [Column("activa")]
        public bool Activa { get; set; } = true;

        [Column("expira_en")]
        public DateTime? ExpiraEn { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}