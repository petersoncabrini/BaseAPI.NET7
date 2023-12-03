using CustomBackend.Domain.Usuarios.Enums;

namespace CustomBackend.Domain.Usuarios.Dtos.Request
{
    public class NewUserRequest
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string ConfirmacaoDeSenha { get; set; }
        public TipoDeAcesso TipoDeAcesso { get; set; }
    }
}
