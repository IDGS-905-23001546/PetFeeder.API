using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("dispensadores_inventario")]
    public class DispensadorInventario
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Column("codigo_unico")]
        public string CodigoUnico { get; set; } = string.Empty;

        [Column("estado")]
        public string Estado { get; set; } = "Pendiente";

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        [ForeignKey("ProductoId")]
        public ProductoTerminado? Producto { get; set; }
    }
}
