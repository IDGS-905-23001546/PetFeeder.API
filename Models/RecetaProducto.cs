using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("recetas_producto")]
    public class RecetaProducto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Column("componente_id")]
        public int ComponenteId { get; set; }

        [Column("cantidad_requerida")]
        public int CantidadRequerida { get; set; }

        [Column("dispensador")]
        public string? Dispensador { get; set; }

        [ForeignKey("ProductoId")]
        public ProductoTerminado? Producto { get; set; }

        [ForeignKey("ComponenteId")]
        public Componente? Componente { get; set; }
    }
}
