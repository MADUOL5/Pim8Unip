using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StreamingAPI.Models;

namespace StreamingAPI.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DateTime GetExpiration()
        {
            var minutos = _configuration.GetValue<int?>("Jwt:ExpiresMinutes") ?? 120;
            return DateTime.UtcNow.AddMinutes(minutos);
        }

        public string GenerateToken(Usuario usuario, DateTime expiraEm)
        {
            var credentials = new SigningCredentials(GetSigningKey(), SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var token = new JwtSecurityToken(
                issuer: GetIssuer(),
                audience: GetAudience(),
                claims: claims,
                expires: expiraEm,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetSigningKey(),
                ValidateIssuer = true,
                ValidIssuer = GetIssuer(),
                ValidateAudience = true,
                ValidAudience = GetAudience(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        }

        private SymmetricSecurityKey GetSigningKey()
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Configure Jwt:Key no appsettings.json.");

            if (Encoding.UTF8.GetByteCount(key) < 32)
            {
                throw new InvalidOperationException("Jwt:Key precisa ter pelo menos 32 caracteres.");
            }

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }

        private string GetIssuer()
        {
            return _configuration["Jwt:Issuer"] ?? "StreamingAPI";
        }

        private string GetAudience()
        {
            return _configuration["Jwt:Audience"] ?? "StreamingAPI.Client";
        }
    }
}
