using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetUserId(this HttpContext context, string defaultUser = "test-user")
        {
            var claimUserId = context.User.FindFirst("id")?.Value
                           ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User.FindFirst(ClaimTypes.Name)?.Value
                           ?? context.User.Identity?.Name;

            if (!string.IsNullOrEmpty(claimUserId))
                return claimUserId;

            if (context.Request.Headers.TryGetValue("X-User-Id", out var headerUserId) && !string.IsNullOrEmpty(headerUserId))
                return headerUserId.ToString();

            return defaultUser;
        }
    }
}
