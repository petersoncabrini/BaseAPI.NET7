using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CustomBackend.Infra.Utils
{
    public static class HttpUtil
    {
        public const string UserAgentKey = "User-Agent";

        public static string GetUserAgent(this IHttpContextAccessor context)
        {
            var headers = context?.HttpContext?.Request?.Headers;

            if (headers == null || !headers.ContainsKey(UserAgentKey))
                return null;

            var result = headers[UserAgentKey];
            return result;
        }

        public static List<Claim> ParseJwtToken(string token)
        {
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwtSecurityToken?.Claims.ToList() ?? new List<Claim>();
        }



        public static Claim GetClaim(this IHttpContextAccessor context, string claimType) => context?.HttpContext?.User?.GetClaim(claimType);

        public static Claim GetClaim(this ClaimsPrincipal claims, string claimType) => claims?.Claims?.FirstOrDefault(x => x.Type == claimType);



        public static string? GetClaimValue(this IEnumerable<Claim>? claims, string claimType) => claims?.FirstOrDefault(x => x.Type == claimType)?.Value;

        public static string? GetClaimValue(this ClaimsPrincipal claims, string claimType) => claims?.Claims?.FirstOrDefault(x => x.Type == claimType)?.Value;

        public static T GetClaimValue<T>(this ClaimsPrincipal claims, string claimType)
        {
            var value = GetClaimValue(claims, claimType);

            if (value.IsValidString())
                return (T)Convert.ChangeType(value, typeof(T));

            return default;
        }

        public static string? GetClaimValue(this IHttpContextAccessor context, string claimType) => context?.HttpContext?.User?.GetClaimValue(claimType);
    }
}
