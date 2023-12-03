using CustomBackend.Domain.Common.Models;
using CustomBackend.Infra.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomBackend.Repository.Db.Map.Common
{
    internal abstract class EfMappingBase<T> : IEntityTypeConfiguration<T> where T : EntityBase
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property(p => p.CreatedAt).ToUtc();
            builder.Property(p => p.UpdatedAt).ToUtc();

            builder.Property(p => p.CreatorIp).HasMaxLength(200);
            builder.Property(p => p.CreatorUserAgent).HasMaxLength(1000);
            builder.Property(p => p.CreatorEmail).HasMaxLength(500);

            builder.Property(p => p.EditorIp).HasMaxLength(200);
            builder.Property(p => p.EditorUserAgent).HasMaxLength(1000);
            builder.Property(p => p.EditorEmail).HasMaxLength(500);
        }
    }
}
