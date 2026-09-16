using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HomelabBlog.Api.Controllers;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var validUsername = _config["AdminUser:Username"];
        var storedHash = _config["AdminUser:PasswordHash"];

        if (request.Username != validUsername ||
            storedHash is null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
        {
            return Unauthorized();
        }

        var signingKey = _config["Jwt:SigningKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: new[] { new Claim(ClaimTypes.Name, request.Username) },
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: creds
        );

        return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token)));
    }
}