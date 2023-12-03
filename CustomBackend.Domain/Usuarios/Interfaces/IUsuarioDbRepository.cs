using CustomBackend.Domain.Common.Interfaces;
using CustomBackend.Domain.Usuarios.Models;

namespace CustomBackend.Domain.Usuarios.Interfaces
{
    public interface IUsuarioDbRepository : ICrudRepositoryBase<Usuario>
    {
    }
}
