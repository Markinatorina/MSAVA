using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using M_SAVA_BLL.Utils;

namespace M_SAVA_API.Handlers
{
    public class NotBannedRequirement : IAuthorizationRequirement { }

    public class NotBannedHandler : AuthorizationHandler<NotBannedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NotBannedRequirement requirement)
        {
            if (!AuthUtils.IsBanned(context.User))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
