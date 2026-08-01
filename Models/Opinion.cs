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
    }
}
