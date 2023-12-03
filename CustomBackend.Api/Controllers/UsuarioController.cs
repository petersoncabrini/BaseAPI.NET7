using CustomBackend.Api.Controllers.Common;
using CustomBackend.Domain.Usuarios.Dtos.Request;
using CustomBackend.Domain.Usuarios.Interfaces;
using CustomBackend.Infra.Dtos.Request;
using CustomBackend.Infra.Dtos.Result;
using CustomBackend.Infra.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomBackend.Api.Controllers
{
    [ApiVersion("1")]
    public class UsuarioController : RootController
    {
        private IUsuarioService usuarioService;

        public UsuarioController(
            NotificationManager notificationManager,
            IUsuarioService usuarioService
        ) : base(notificationManager)
        {
            this.usuarioService = usuarioService;
        }

        [Authorize]
        [HttpGet("ping")]
        public IActionResult Ping()
            => Ok();

        [AllowAnonymous]
        [HttpPost()]
        public Task<IActionResult> CreateAsync(NewUserRequest request)
            => Response(usuarioService.CreateAsync(request).ConfigureAwait(false));

        [HttpPost("auth")]
        public Task<ActionResult<TokenAuthResult>> AutenticarPorEmailAdminAsync(AuthByMailRequest request)
            => Response(usuarioService.AuthenticateAsync(request));

        [HttpPost("auth/refresh")]
        public Task<ActionResult<TokenAuthResult>> AutenticarPorRefreshTokenAdminAsync(AuthByRefreshTokenRequest request)
            => Response(usuarioService.AuthenticateAsync(request));
    }
}