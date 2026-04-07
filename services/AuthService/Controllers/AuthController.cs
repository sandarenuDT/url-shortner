using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email already in use." });

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email
        });
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email
        });
    }
}