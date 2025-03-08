using System.IdentityModel.Tokens.Jwt;
using System.Text;
using InventoryManagement.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Infrastructure.Middleware
{
      public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings)
        {
            _next = next;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                AttachUserToContext(context, token);

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "nameid").Value;

                // Attach user ID to context
                context.Items["UserId"] = userId;
                context.Items["Username"] = jwtToken.Claims.First(x => x.Type == "unique_name").Value;
                context.Items["UserRole"] = jwtToken.Claims.First(x => x.Type == "role").Value;
                context.Items["CurrentUser"] = new 
                {
                    Id = userId,
                    Username = jwtToken.Claims.First(x => x.Type == "unique_name").Value,
                    Email = jwtToken.Claims.First(x => x.Type == "email").Value,
                    Role = jwtToken.Claims.First(x => x.Type == "role").Value
                };
            }
            catch
            {
                // Do nothing if token validation fails
                // User won't be attached to context so request won't have access to secured endpoints
            }
        }
    }
}