using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("mascotas")]
    public class Mascota
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("raza")]
        public string Raza { get; set; } = string.Empty;

        [Column("edad_anos")]
        public byte EdadAnos { get; set; }

        [Column("peso_kg", TypeName = "decimal(5,2)")]
        public decimal PesoKg { get; set; }

        [Column("tamano")]
        public string Tamano { get; set; } = "mediano";

        [Column("activa")]
        public bool Activa { get; set; }

        [Column("foto_uri")]
        public string? FotoUri { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}