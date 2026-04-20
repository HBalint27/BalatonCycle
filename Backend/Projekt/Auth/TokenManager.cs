using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Projekt.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Projekt.Auth
{
    public class TokenManager
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly Dictionary<string, List<string>> _rolesPermissions = new();

        public List<string> Permissions
        {
            get
            {
                var permissions = new List<string>();
                foreach (var values in _rolesPermissions.Values)
                {
                    permissions.AddRange(values);
                }
                return permissions;
            }
        }

        public TokenManager(IConfiguration configuration)
        {
            _secretKey = configuration["Auth:JWT:Key"] ?? throw new ApplicationException("JWT Key hiányzik");
            _issuer = configuration["Auth:JWT:Issuer"] ?? throw new ApplicationException("JWT Issuer hiányzik");
            _audience = configuration["Auth:JWT:Audience"] ?? throw new ApplicationException("JWT Audience hiányzik");

            foreach (var role in configuration.GetSection("Auth:Roles").GetChildren())
            {
                var permissions = role.GetChildren()
                    .Select(p => p.Value)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();

                _rolesPermissions.Add(role.Key, permissions);
            }
        }

        public string GenerateToken(Felhasznalo user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("Fid", user.Fid.ToString()) // <-- ADD THIS LINE!
            };

            if (user.Statusz != null && _rolesPermissions.ContainsKey(user.Statusz))
            {
                foreach (var permission in _rolesPermissions[user.Statusz])
                {
                    claims.Add(new Claim("permission", permission));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}