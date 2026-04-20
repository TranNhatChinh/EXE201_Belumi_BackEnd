using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Entities;

namespace YourApp.Infrastructure.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;
        private readonly SymmetricSecurityKey _key;

        public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
        {
            if (jwtSettings?.Value == null)
                throw new ArgumentNullException(nameof(jwtSettings), "JwtSettings is missing in configuration.");

            _jwtSettings = jwtSettings.Value;
            
            // Cache the key for performance
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        }

        public string GenerateToken(User user)
        {
            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Standard for ASP.NET Auth
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        }
    }
}
