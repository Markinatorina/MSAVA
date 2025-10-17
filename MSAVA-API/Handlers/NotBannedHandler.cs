using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using MSAVA_BLL.Utils;

namespace MSAVA_API.Handlers
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
