using CustomBackend.Infra.Settings;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomBackend.Infra.Tokens
{
    public class JwtTokenManager
    {
        public string GenerateToken(
            Guid userId,
            string userName,
            string userEmail,
            string refreshToken,
            string tipoDeAcesso,
            AppSettings settings
        )
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(settings.JwtSetting.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Email, userEmail),
                    new Claim("RefreshToken", refreshToken),
                    new Claim("AccessType", tipoDeAcesso),
                }),
                Expires = DateTime.UtcNow.AddMinutes(settings.JwtSetting.TokenTimeoutInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
