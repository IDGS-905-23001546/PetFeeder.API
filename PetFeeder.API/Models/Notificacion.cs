using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("notificaciones")]
    public class Notificacion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("dispensador_id")]
        public int? DispensadorId { get; set; }

        [Column("tipo")]
        public string Tipo { get; set; } = "otro";

        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Column("mensaje")]
        public string? Mensaje { get; set; }

        [Column("leida")]
        public bool Leida { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}