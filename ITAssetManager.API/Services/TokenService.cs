using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ITAssetManager.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ITAssetManager.API.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"] ?? "ITAssetManager_DefaultSecret_Key_2026!@#$%"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("EmployeeNo", user.EmployeeNo),
            new Claim("DepartmentId", user.DepartmentId ?? ""),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "ITAssetManager",
            audience: _config["Jwt:Audience"] ?? "ITAssetManager",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
