using M_SAVA_Shared.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace M_SAVA_API.Middleware
{
    public class RequestContextMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var headersDto = new HeadersDTO
            {
                Authorization = context.Request.Headers["Authorization"],
                ContentType = context.Request.ContentType,
                Accept = context.Request.Headers["Accept"],
                UserAgent = context.Request.Headers["User-Agent"],
                Host = context.Request.Headers["Host"],
                Referer = context.Request.Headers["Referer"],
                Origin = context.Request.Headers["Origin"],
                AcceptEncoding = context.Request.Headers["Accept-Encoding"],
                AcceptLanguage = context.Request.Headers["Accept-Language"],
                CacheControl = context.Request.Headers["Cache-Control"],
                Pragma = context.Request.Headers["Pragma"],
                Cookie = context.Request.Headers["Cookie"],
                XRequestedWith = context.Request.Headers["X-Requested-With"],
                XForwardedFor = context.Request.Headers["X-Forwarded-For"],
                Connection = context.Request.Headers["Connection"],
                CustomHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            var sessionDto = new SessionDTO();
            ClaimsPrincipal user = context.User;
            sessionDto.LoggedIn = user?.Identity?.IsAuthenticated ?? false;
            if (sessionDto.LoggedIn && user != null)
            {
                sessionDto.UserId = Guid.TryParse(
                    user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub),
                    out var uid) ? uid : Guid.Empty;
                sessionDto.Username = user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ?? string.Empty;
                sessionDto.IsAdmin = user.IsInRole("Admin");
                sessionDto.IsBanned = user.IsInRole("Banned");
                sessionDto.IsWhitelisted = user.IsInRole("Whitelisted");
                sessionDto.Roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                sessionDto.Claims = user.Claims
                                    .GroupBy(c => c.Type)
                                    .ToDictionary(g => g.Key, g => g.Select(c => c.Value)
                                    .ToList());
                sessionDto.IssuedAt = DateTime.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Iat), out var iat) ? iat : DateTime.MinValue;
                sessionDto.ExpiresAt = DateTime.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Exp), out var exp) ? exp : DateTime.MinValue;
                var accessGroupsClaim = user.FindFirst("accessGroups")?.Value;
                sessionDto.AccessGroups = string.IsNullOrEmpty(accessGroupsClaim) ? new() : accessGroupsClaim.Split(',').Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty).ToList();
            }
            else
            {
                sessionDto.UserId = Guid.Empty;
                sessionDto.Username = string.Empty;
                sessionDto.Roles = new();
                sessionDto.Claims = new();
                sessionDto.AccessGroups = new();
                sessionDto.IssuedAt = DateTime.MinValue;
                sessionDto.ExpiresAt = DateTime.MinValue;
            }

            context.Items[nameof(HeadersDTO)] = headersDto;
            context.Items[nameof(SessionDTO)] = sessionDto;

            await _next(context);
        }
    }
}
