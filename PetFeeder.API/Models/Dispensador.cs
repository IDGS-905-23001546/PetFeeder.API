using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("dispensadores")]
    public class Dispensador
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("codigo_unico")]
        public string CodigoUnico { get; set; } = string.Empty;

        [Column("firmware_version")]
        public string FirmwareVersion { get; set; } = "v1.0.0";

        [Column("estado")]
        public string Estado { get; set; } = "offline";

        [Column("bateria_percent")]
        public byte BateriaPercent { get; set; }

        [Column("nivel_tolva_pct")]
        public byte NivelTolvaPct { get; set; }

        [Column("ssid_wifi")]
        public string? SsidWifi { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("last_ping_at")]
        public DateTime? LastPingAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}