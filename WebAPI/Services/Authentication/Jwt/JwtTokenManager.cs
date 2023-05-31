using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebAPI.Services.Authentication.Jwt
{
    public class JwtTokenManager
    {
        private readonly JwtSettings _settings;
        private static readonly TimeSpan _lifeTime = TimeSpan.FromHours(1);

        public JwtTokenManager(JwtSettings settings)
        {
            _settings = settings;
        }
        public string CreateToken(UserClaims user)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Guid.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Login),
                new Claim(CustomClaimNames.IsAdmin, user.IsAdmin.ToString()),
            };
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_lifeTime),
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                    SecurityAlgorithms.HmacSha512Signature)
            };
            SecurityToken token = tokenHandler.CreateToken(descriptor);
            string jwtToken = tokenHandler.WriteToken(token);
            return jwtToken;
        }
    }
}
