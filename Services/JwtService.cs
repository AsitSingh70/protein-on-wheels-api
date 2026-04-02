using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProteinOnWheelsAPI.Models;

namespace ProteinOnWheelsAPI.Services;

public class JwtService
{
    private readonly string key = "thIS_iS_mY_SuPER_SeCRET_KEy_FOR_PrOTEIN_On_WHEELS_637105";

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.ASCII.GetBytes(key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("name", user.Name),
                new Claim("email", user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(120),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}