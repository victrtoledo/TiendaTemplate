using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TiendaApi.Data;
using TiendaApi.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Se requiere autenticación para todas las acciones
public class PedidosController : ControllerBase
{
    private readonly TiendaDbContext _context;

    public PedidosController(TiendaDbContext context)
    {
        _context = context;
    }

    // GET: api/pedidos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // El ID del usuario del token
        var userRole = User.FindFirstValue(ClaimTypes.Role); // El rol del usuario

        if (userRole == "Admin")
        {
            // Admin puede ver todos los pedidos
            var pedidos = await _context.Pedidos
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();
            return Ok(pedidos);
        }
        else
        {
            // Usuario normal solo ve sus propios pedidos
            var pedidosUsuario = await _context.Pedidos
                .Where(p => p.UsuarioId == userId)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();
            return Ok(pedidosUsuario);
        }
    }

    // GET: api/pedidos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var pedido = await _context.Pedidos
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return NotFound();

        if (userRole != "Admin" && pedido.UsuarioId != userId)
            return Forbid(); // No puede acceder a pedidos de otros usuarios

        return Ok(pedido);
    }

    // POST: api/pedidos
    [HttpPost]
    public async Task<ActionResult<Pedido>> CrearPedido(Pedido pedido)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        pedido.UsuarioId = userId; // Asignar el pedido al usuario autenticado

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

    // PUT y DELETE pueden implementarse solo para admins o según lo que decidas
}
