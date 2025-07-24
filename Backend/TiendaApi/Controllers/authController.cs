using TiendaApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TiendaApi.Models;
using Microsoft.EntityFrameworkCore;
using TiendaApi.Dtos;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TiendaDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(TiendaDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult SoloAdmin()
    {
        return Ok("Este es un endpoint solo para admins.");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == request.NombreUsuario))
            return BadRequest("El usuario ya existe.");

        var usuario = new Usuario
        {
            NombreUsuario = request.NombreUsuario,
            Correo = request.Correo,
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(request.Contrasena),
            Rol = request.Rol.ToLower()
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok("Usuario registrado correctamente.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.ContrasenaHash))
            return Unauthorized("Credenciales inválidas.");

        var token = CrearToken(usuario);

        return Ok(new
        {
            token,
            rol = usuario.Rol,
            id = usuario.Id
        });
    }

    private string CrearToken(Usuario usuario)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), // ID de usuario
        new Claim(ClaimTypes.Name, usuario.NombreUsuario),
        new Claim(ClaimTypes.Role, usuario.Rol.ToLower()) // rol en minúsculas para evitar problemas
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(3),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
}
