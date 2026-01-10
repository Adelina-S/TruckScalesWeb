using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TruckScalesWeb.ScaleConnection
{
    //[ApiController]
    //[Route("api/[controller]")]
    //[AllowAnonymous]
    //public class AuthController : ControllerBase
    //{
    //    private readonly IConfiguration _configuration;

    //    public AuthController(IConfiguration configuration)
    //    {
    //        _configuration = configuration;
    //    }

    //    [HttpPost("token")]
    //    public IActionResult GetToken([FromBody] LoginModel model)
    //    {
    //        // Простая проверка (замените на свою логику)
    //        if (model.Username == "client" && model.Password == "secret")
    //        {
    //            var token = GenerateJwtToken(model.Username);
    //            return Ok(new { Token = token });
    //        }

    //        return Unauthorized();
    //    }

    //    private string GenerateJwtToken(string username)
    //    {
    //        var key = new SymmetricSecurityKey(
    //            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    //        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //        var token = new JwtSecurityToken(
    //            issuer: _configuration["Jwt:Issuer"],
    //            audience: _configuration["Jwt:Audience"],
    //            claims: new[] { new Claim(ClaimTypes.Name, username) },
    //            expires: DateTime.UtcNow.AddHours(1),
    //            signingCredentials: credentials);

    //        return new JwtSecurityTokenHandler().WriteToken(token);
    //    }
    //}

    //public class LoginModel
    //{
    //    public string Username { get; set; }
    //    public string Password { get; set; }
    //}
}
