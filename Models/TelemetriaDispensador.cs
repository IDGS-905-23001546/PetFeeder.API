using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("telemetria_dispensador")]
    public class TelemetriaDispensador
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }   // BIGINT -> long

        [Column("dispensador_id")]
        public int DispensadorId { get; set; }

        [Column("bateria_percent")]
        public byte BateriaPercent { get; set; }

        [Column("nivel_tolva_pct")]
        public byte NivelTolvaPct { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "activo";

        [Column("registrado_en")]
        public DateTime RegistradoEn { get; set; }
    }
}