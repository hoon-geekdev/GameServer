using Microsoft.AspNetCore.Authorization;
using Constants;
using Services.Redis;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;

namespace Utils
{
    public class AuthGuardMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RedisCacheService _redis;
        private readonly JwtSetting _jwtSettings;

        public AuthGuardMiddleware(RequestDelegate next, RedisCacheService redis, IOptions<JwtSetting> jwtSettings)
        {
            _next = next;
            _redis = redis;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            bool isAuthorized = context.GetEndpoint()?.Metadata.GetMetadata<AuthorizeAttribute>() != null;

            if (!isAuthorized)
            {
                // 인증이 필요하지 않은 요청이므로 다음 미들웨어로 넘어가기
                await _next(context);
                return;
            }

            string? token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                // 토큰이 없는 경우 401 Unauthorized 반환
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Token is required.");
                return;
            }

            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            try
            {
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };

                ClaimsPrincipal principal = jwtHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                string? accountId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string? tokenJti = principal.FindFirst("jti")?.Value;

                if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(tokenJti))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Invalid token claims.");
                    return;
                }

                // Redis의 jti 검증
                UserCache userCache = new UserCache(accountId, _redis);
                string? storedJti = await userCache.GetToken();

                if (storedJti != tokenJti)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Invalid session.");
                    return;
                }

                context.Items["accountId"] = accountId;

                // 다음 미들웨어로 넘어가기
                await _next(context);
            }
            catch (SecurityTokenExpiredException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Token expired.");
            }
            catch (SecurityTokenException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid token.");
            }
        }
    }
}
