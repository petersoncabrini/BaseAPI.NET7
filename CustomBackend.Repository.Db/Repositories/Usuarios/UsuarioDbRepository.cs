using CustomBackend.Domain.Usuarios.Interfaces;
using CustomBackend.Domain.Usuarios.Models;
using CustomBackend.Repository.Db.Context;
using CustomBackend.Repository.Db.Repositories.Common;

namespace CustomBackend.Repository.Db.Repositories.Usuarios
{
    public class UsuarioDbRepository : CrudRepositoryBase<Usuario>, IUsuarioDbRepository
    {
        public UsuarioDbRepository(MainDbContext context) : base(context)
        {
        }
    }
}
