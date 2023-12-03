using CustomBackend.Domain.Common.Interfaces;
using CustomBackend.Domain.Usuarios.Dtos.Request;
using CustomBackend.Domain.Usuarios.Models;
using CustomBackend.Infra.Dtos.Request;
using CustomBackend.Infra.Dtos.Result;

namespace CustomBackend.Domain.Usuarios.Interfaces
{
    public interface IUsuarioService : IServiceBase
    {
        Task CreateAsync(NewUserRequest request);
        Task<Usuario> GetAsync(string email, bool active = true);



        Task<TokenAuthResult> AuthenticateAsync(AuthByMailRequest request);
        Task<TokenAuthResult> AuthenticateAsync(AuthByRefreshTokenRequest request);
    }
}
