using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFeeder.API.Models
{
    [Table("inventario_componentes")]
    public class Componente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("stock")]
        public int Stock { get; set; }

        [Column("unidad_medida")]
        public string UnidadMedida { get; set; } = "pza";
    }
}
