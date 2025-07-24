using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApi.Models
{

    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        // Relación: una categoría tiene muchos productos
        public List<Producto> Productos { get; set; }
    }
}