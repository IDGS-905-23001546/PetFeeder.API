using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("opiniones")]
    public class Opinion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("nombre_usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Column("detalles_mascota")]
        public string DetallesMascota { get; set; } = string.Empty;

        [Column("calificacion")]
        public int Calificacion { get; set; }

        [Column("comentario")]
        public string Comentario { get; set; } = string.Empty;

        [Column("fecha")]
        public string Fecha { get; set; } = string.Empty;

        [Column("estado")]
        public string Estado { get; set; } = "Nuevo";

        [Column("respuesta_admin")]
        public string? RespuestaAdmin { get; set; }

        [Column("fecha_respuesta")]
        public string? FechaRespuesta { get; set; }
    }
}
