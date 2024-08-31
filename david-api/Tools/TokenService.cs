using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace webapp.DAL.Tools
{
    public class TokenService
    {
        public static string BuildToken(string key, string issuer, string audience, string username, string role)
        {
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer,
              audience,
              claims,
              expires: DateTime.Now.AddDays(365),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static ClaimsPrincipal ValidateToken(string token, string key, string issuer, string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                // Handle token validation failure (e.g., log the error, rethrow, etc.)
                throw new SecurityTokenValidationException("Token validation failed", ex);
            }
        }
    }

}
