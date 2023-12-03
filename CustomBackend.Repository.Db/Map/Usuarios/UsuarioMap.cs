using CustomBackend.Domain.Usuarios.Models;
using CustomBackend.Infra.Utils;
using CustomBackend.Repository.Db.Map.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomBackend.Repository.Db.Map.Usuarios
{
    internal class UsuarioMap : EfMappingBase<Usuario>, IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.Nome).HasMaxLength(500);
            builder.Property(p => p.Email).HasMaxLength(500);
            builder.Property(p => p.Senha).HasMaxLength(250);
            builder.Property(p => p.TipoDeAcesso);
            builder.Property(p => p.UltimoLogin).ToUtc();
            builder.Property(p => p.RefreshToken).HasMaxLength(500);
        }
    }
}
