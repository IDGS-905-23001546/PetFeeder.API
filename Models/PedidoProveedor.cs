using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("pedidos_proveedor")]
    public class PedidoProveedor
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("folio")]
        public string Folio { get; set; } = string.Empty;

        [Column("proveedor_id")]
        public int ProveedorId { get; set; }

        [Column("proveedor_nombre")]
        public string ProveedorNombre { get; set; } = string.Empty;

        [Column("componente_id")]
        public int ComponenteId { get; set; }

        [Column("componente_nombre")]
        public string ComponenteNombre { get; set; } = string.Empty;

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("unidad_medida")]
        public string UnidadMedida { get; set; } = "pza";

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Column("estado")]
        public string Estado { get; set; } = "pendiente";

        [Column("fecha_recibido")]
        public DateTime? FechaRecibido { get; set; }
    }
}
