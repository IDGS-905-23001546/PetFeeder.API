using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("inventario_productos")]
    public class ProductoTerminado
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("stock")]
        public int Stock { get; set; }

        [Column("estado")]
        public string? Estado { get; set; }

        public ICollection<RecetaProducto>? Recetas { get; set; }
    }
}
