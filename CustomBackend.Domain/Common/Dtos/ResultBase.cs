using CustomBackend.Domain.Common.Models;

namespace CustomBackend.Domain.Common.Dtos
{
    public abstract class ResultBase<TEntity> where TEntity : EntityBase
    {
        public ResultBase()
        {

        }

        public ResultBase(TEntity e)
        {
            Id = e.Id;
            CreatedAt = e.CreatedAt;
            UpdatedAt = e.UpdatedAt;
            Active = e.Active;

            CreatorIp = e.CreatorIp;
            CreatorUserAgent = e.CreatorUserAgent;
            CreatorEmail = e.CreatorEmail;

            EditorIp = e.EditorIp;
            EditorUserAgent = e.EditorUserAgent;
            EditorEmail = e.EditorEmail;
        }

        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Active { get; set; }

        public virtual string CreatorIp { get; set; }
        public virtual string CreatorUserAgent { get; set; }
        public virtual string CreatorEmail { get; set; }

        public virtual string EditorIp { get; set; }
        public virtual string EditorUserAgent { get; set; }
        public virtual string EditorEmail { get; set; }
    }
}
