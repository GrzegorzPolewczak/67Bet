using Microsoft.AspNetCore.Mvc;
using _67Bet.Identity.Application.Interfaces;
using _67Bet.Identity.Application.DTOs;
using _67Bet.Identity.Application.Mappings;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace _67Bet.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IConfiguration _configuration;

    public AuthController(IIdentityService identityService, IConfiguration configuration)
    {
        _identityService = identityService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request)
    {
        var user = await _identityService.RegisterAsync(request.Username, request.Email, request.Password);
        return Ok(user.ToDto());
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginRequest request)
    {
        var success = await _identityService.AuthenticateAsync(request.Email, request.Password);
        if (!success)
        {
            return Unauthorized("Nieprawidłowy e-mail lub hasło.");
        }

        var user = await _identityService.GetUserByEmailAsync(request.Email);
        var token = GenerateJwtToken(user!);
        
        return Ok(token);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);
        var user = await _identityService.GetUserByIdAsync(userId);
        
        if (user == null) return NotFound("Użytkownik nie znaleziony.");

        return Ok(user.ToDto());
    }

    private string GenerateJwtToken(_67Bet.Identity.Domain.Entities.User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.UserRole.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
