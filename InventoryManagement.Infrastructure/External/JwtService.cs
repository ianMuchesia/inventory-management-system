using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagement.Application.Persistence.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Settings;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Infrastructure.External
{
    public class JwtService : IJwtService
    {

        private readonly JwtSettings _jwtSettings;

        public JwtService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }


        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("nameid", user.Id.ToString()),
                    new Claim("unique_name", user.Username),
                    new Claim("role", user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Audience = _jwtSettings.Audience,
                Issuer = _jwtSettings.Issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var result = tokenHandler.WriteToken(token);
            return result;
        }
    }
}