using CustomBackend.Domain.Common.Services;
using CustomBackend.Domain.Usuarios.Dtos.Request;
using CustomBackend.Domain.Usuarios.Interfaces;
using CustomBackend.Domain.Usuarios.Models;
using CustomBackend.Infra.Dtos.Request;
using CustomBackend.Infra.Dtos.Result;
using CustomBackend.Infra.Notifications;
using CustomBackend.Infra.Settings;
using CustomBackend.Infra.Tokens;
using CustomBackend.Infra.Utils;

namespace CustomBackend.Domain.Usuarios.Services
{
    public class UsuarioService : ServiceBase, IUsuarioService
    {
        const string defaultAuthErrorMessage = "Usuário ou senha inválidos";

        private readonly IUsuarioDbRepository usuarioDbRepository;
        private readonly JwtTokenManager jwtTokenManager;
        private readonly AppSettings settings;

        public UsuarioService(
            NotificationManager notificationManager,
            IUsuarioDbRepository usuarioDbRepository,
            JwtTokenManager jwtTokenManager,
            AppSettings settings
        ) : base(notificationManager)
        {
            this.usuarioDbRepository = usuarioDbRepository;
            this.jwtTokenManager = jwtTokenManager;
            this.settings = settings;
        }

        #region CRUD

        public async Task CreateAsync(NewUserRequest request)
        {

            if (request.Senha != request.ConfirmacaoDeSenha)
            {
                notificationManager.Add("A senha não corresponde a confirmação da senha.");
                return;
            }

            var entity = new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                Senha = request.Senha.ToMd5(),
                TipoDeAcesso = request.TipoDeAcesso
            };

            await ValidateAndExecAsync(async () => await usuarioDbRepository.SaveAndCommitAsync(entity));
        }

        public Task<Usuario> GetAsync(string email, bool active = true) => usuarioDbRepository.FirstOrDefaultAsync(u =>
            u.Email == email
            && u.Active == active
        );

        #endregion

        #region Auth

        public async Task<TokenAuthResult> AuthenticateAsync(AuthByMailRequest request)
        {
            ValidateRequest(request);

            if (notificationManager.Any())
                return null;

            var user = await GetAsync(request.Email);

            if (user == null)
            {
                notificationManager.Add(defaultAuthErrorMessage, NotificationType.Validation);
                return null;
            }

            CheckPassword(request.Password, user.Senha);

            if (notificationManager.Any())
                return null;

            return await AuthenticateAsync(user);
        }

        public async Task<TokenAuthResult> AuthenticateAsync(AuthByRefreshTokenRequest request)
        {
            ValidateRequest(request);

            if (notificationManager.Any())
                return null;

            var user = await GetAsync(request.Email);

            if (user == null)
            {
                notificationManager.Add(defaultAuthErrorMessage, NotificationType.Validation);
                return null;
            }

            CheckRefreshToken(request, user);

            if (notificationManager.Any())
                return null;

            return await AuthenticateAsync(user);
        }



        private async Task<TokenAuthResult> AuthenticateAsync(Usuario user)
        {
            user.UltimoLogin = DateTime.UtcNow;
            user.RefreshToken = Guid.NewGuid().ToString();
            await usuarioDbRepository.SaveAndCommitAsync(user);

            var result = new TokenAuthResult
            {
                Token = jwtTokenManager.GenerateToken(
                    user.Id,
                    user.Nome,
                    user.Email,
                    user.RefreshToken,
                    user.TipoDeAcesso.ToString(),
                    settings
                )
            };

            return result;
        }

        private void ValidateRequest(AuthByMailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                notificationManager.Add("Informe um endereço de e-mail válido.", NotificationType.Validation);

            if (string.IsNullOrWhiteSpace(request.Password))
                notificationManager.Add("Informe uma senha válida.", NotificationType.Validation);
        }

        private void ValidateRequest(AuthByRefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                notificationManager.Add("Informe um endereço de e-mail válido.", NotificationType.Validation);

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                notificationManager.Add("Informe um Refreshtoken válido.", NotificationType.Validation);
        }

        private void CheckPassword(string rawPassword, string salvedHashedPassword)
        {
            var passwordEncrypted = rawPassword.ToMd5();

            if (salvedHashedPassword != passwordEncrypted)
                notificationManager.Add(defaultAuthErrorMessage, NotificationType.Error);
        }

        private void CheckRefreshToken(AuthByRefreshTokenRequest request, Usuario user)
        {
            var refreshTokenLimit = user.UltimoLogin.Value.AddMinutes(settings.JwtSetting.RefreshTokenTimeoutInMinutes);

            if (
                DateTime.UtcNow > refreshTokenLimit
                || user.RefreshToken != request.RefreshToken
                || user.Email != request.Email
            )
                notificationManager.Add(defaultAuthErrorMessage, NotificationType.Validation);
        }

        #endregion
    }
}
