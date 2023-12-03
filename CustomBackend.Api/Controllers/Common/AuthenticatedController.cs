using CustomBackend.Infra.Notifications;
using CustomBackend.Infra.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CustomBackend.Api.Controllers.Common
{
    [Authorize]
    public abstract class AuthenticatedController : RootController
    {
        protected AuthenticatedController(NotificationManager notificationManager) : base(notificationManager)
        {
        }

        public string UserEmail => User.GetClaimValue(ClaimTypes.Email);
    }
}
