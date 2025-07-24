using Microsoft.EntityFrameworkCore;
using TiendaApi.Models;

namespace TiendaApi.Data
{
    public class ProductoDbContext : DbContext
    {
        public ProductoDbContext(DbContextOptions<ProductoDbContext> options)
            : base(options)
        {

        }
        public DbSet<Producto> Productos { get; set; }
     

    }
}