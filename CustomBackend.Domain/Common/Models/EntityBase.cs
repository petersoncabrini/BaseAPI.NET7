namespace CustomBackend.Domain.Common.Models
{
    public abstract class EntityBase
    {
        public EntityBase()
        {
            CreatedAt = UpdatedAt = DateTime.UtcNow;
            Active = true;
        }

        public virtual Guid Id { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
        public virtual bool Active { get; set; }

        public virtual string? CreatorIp { get; set; }
        public virtual string? CreatorUserAgent { get; set; }
        public virtual string? CreatorEmail { get; set; }

        public virtual string? EditorIp { get; set; }
        public virtual string? EditorUserAgent { get; set; }
        public virtual string? EditorEmail { get; set; }

        public bool IsNew() => Id == Guid.Empty;
    }
}
