using System;
using System.Security.Claims;

namespace MSAVA_BLL.Utils
{
    public static class AuthUtils
    {
        public static Guid GetUserId(ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst("id")?.Value;
            if (idClaim == null)
                throw new InvalidOperationException("Missing 'id' claim in user principal.");
            if (!Guid.TryParse(idClaim, out var userId))
                throw new InvalidOperationException($"Claim 'id' is not a valid Guid: '{idClaim}'.");
            return userId;
        }

        public static string GetUsername(ClaimsPrincipal user)
        {
            var username = user.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Missing or empty username in user identity.");
            return username;
        }

        public static bool GetIsAdmin(ClaimsPrincipal user)
        {
            var isAdminClaim = user.FindFirst("isAdmin")?.Value;
            if (isAdminClaim == null)
                throw new InvalidOperationException("Missing 'isAdmin' claim in user principal.");
            if (isAdminClaim == "true")
                return true;
            if (isAdminClaim == "false")
                return false;
            throw new InvalidOperationException($"Claim 'isAdmin' must be 'true' or 'false', but was '{isAdminClaim}'.");
        }

        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }

        public static bool IsBanned(ClaimsPrincipal user)
        {
            return user.IsInRole("Banned");
        }
    }
}
