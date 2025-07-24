
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApi.Models
{
    public class Pedido
    {
    public int Id { get; set; }
    public string UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado { get; set; }
    public decimal Total { get; set; }
    public List<DetallePedido> Detalles { get; set; }
        

}
   
}