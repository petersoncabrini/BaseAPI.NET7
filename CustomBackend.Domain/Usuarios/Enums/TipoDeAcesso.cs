using System.ComponentModel.DataAnnotations;

namespace CustomBackend.Domain.Usuarios.Enums
{
    public enum TipoDeAcesso
    {
        [Display(Name = "Administrador")]
        Admin = 1,

        Padrao = 2


    }
}
