using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("otp_verificacion")]
    public class OtpVerificacion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("intentos")]
        public byte Intentos { get; set; }

        [Column("max_intentos")]
        public byte MaxIntentos { get; set; }

        [Column("expira_en")]
        public DateTime ExpiraEn { get; set; }

        [Column("usado")]
        public bool Usado { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}