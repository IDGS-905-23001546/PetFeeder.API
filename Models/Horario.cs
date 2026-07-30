
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("horarios")]
    public class Horario
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("mascota_id")]
        public int? MascotaId { get; set; }

        [Column("dispensador_id")]
        public int? DispensadorId { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("icono")]
        public string Icono { get; set; } = "sun";

        [Column("hora")]
        public string Hora { get; set; } = string.Empty;

        [Column("lunes")]
        public bool Lunes { get; set; }

        [Column("martes")]
        public bool Martes { get; set; }

        [Column("miercoles")]
        public bool Miercoles { get; set; }

        [Column("jueves")]
        public bool Jueves { get; set; }

        [Column("viernes")]
        public bool Viernes { get; set; }

        [Column("sabado")]
        public bool Sabado { get; set; }

        [Column("domingo")]
        public bool Domingo { get; set; }

        [Column("porcion_gramos", TypeName = "decimal(6,1)")]
        public decimal PorcionGramos { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}