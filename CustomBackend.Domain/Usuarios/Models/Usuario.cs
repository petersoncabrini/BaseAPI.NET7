using CustomBackend.Domain.Common.Models;
using CustomBackend.Domain.Usuarios.Enums;

namespace CustomBackend.Domain.Usuarios.Models
{
    public class Usuario : EntityBase
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public TipoDeAcesso TipoDeAcesso { get; set; }
        public string? RefreshToken { get; set; }
    }
}
