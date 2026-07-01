using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("dispensaciones_agua")]
    public class DispensacionAgua
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

        [Column("horario_agua_id")]
        public int? HorarioAguaId { get; set; }

        [Column("tipo")]
        public string Tipo { get; set; } = "manual";

        [Column("nombre")]
        public string Nombre { get; set; } = "Manual";

        [Column("cantidad_ml", TypeName = "decimal(7,1)")]
        public decimal CantidadMl { get; set; }

        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "ejecutada";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
