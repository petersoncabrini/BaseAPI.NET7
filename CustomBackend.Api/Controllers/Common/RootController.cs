using CustomBackend.Infra.Notifications;
using CustomBackend.Infra.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace CustomBackend.Api.Controllers.Common
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class RootController : ControllerBase
    {
        protected readonly NotificationManager notificationManager;

        public RootController(NotificationManager notificationManager) => this.notificationManager = notificationManager;

        protected ActionResult<T> Response<T>(T result)
        {
            var notificationResult = CheckNotifications();

            if (notificationResult != null)
                return notificationResult;

            return Ok(result);
        }

        protected async Task<ActionResult<T>> Response<T>(Task<T> task)
        {
            try
            {
                var result = await task;

                var notificationResult = CheckNotifications();

                if (notificationResult != null)
                    return notificationResult;

                return Ok(result);
            }
            catch (Exception ex)
            {
                notificationManager.Add(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, notificationManager.List());
            }
        }

        protected async Task<IActionResult> Response(ConfiguredTaskAwaitable task)
        {
            try
            {
                await task;

                var notificationResult = CheckNotifications();

                if (notificationResult != null)
                    return notificationResult;

                return Ok();
            }
            catch (Exception ex)
            {
                notificationManager.Add(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, notificationManager.List());
            }
        }

        protected async Task<IActionResult> Response<T>(ConfiguredTaskAwaitable<T> task)
        {
            try
            {
                await task;

                var notificationResult = CheckNotifications();

                if (notificationResult != null)
                    return notificationResult;

                return Ok();
            }
            catch (Exception ex)
            {
                notificationManager.Add(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, notificationManager.List());
            }
        }

        protected string UserAgent { get => Request.Headers[HttpUtil.UserAgentKey]; }
        protected string RequestIp { get => Request.HttpContext.Connection.RemoteIpAddress.ToString(); }

        protected string GetUserAgent(string userAgent) => userAgent.IsValidString() ? userAgent : UserAgent;
        protected string GetUserRequestIp(string requestIp) => requestIp.IsValidString() ? requestIp : RequestIp;

        private ObjectResult CheckNotifications()
        {
            if (notificationManager.Any(NotificationType.Error, NotificationType.Validation))
                return StatusCode(StatusCodes.Status500InternalServerError, notificationManager.List());
            else if (notificationManager.Any(NotificationType.Authentication))
                return StatusCode(StatusCodes.Status401Unauthorized, notificationManager.List());
            else if (notificationManager.Any(NotificationType.Authorization))
                return StatusCode(StatusCodes.Status403Forbidden, notificationManager.List());
            else if (notificationManager.Any(NotificationType.NotFound))
                return StatusCode(StatusCodes.Status404NotFound, notificationManager.List());
            else if (notificationManager.Any(NotificationType.Info))
                return StatusCode(StatusCodes.Status200OK, notificationManager.List());

            return null;
        }
    }
}
