using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("dispensaciones")]
    public class Dispensacion
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

        [Column("horario_id")]
        public int? HorarioId { get; set; }

        [Column("tipo")]
        public string Tipo { get; set; } = "manual";

        [Column("nombre")]
        public string Nombre { get; set; } = "Manual";

        [Column("porcion_gramos", TypeName = "decimal(6,1)")]
        public decimal PorcionGramos { get; set; }

        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "ejecutada";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}