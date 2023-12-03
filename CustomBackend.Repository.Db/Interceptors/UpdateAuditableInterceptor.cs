using CustomBackend.Domain.Common.Models;
using CustomBackend.Infra.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

namespace CustomBackend.Repository.Db.Interceptors
{
    public class UpdateAuditableInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor httpContext;

        public UpdateAuditableInterceptor(IHttpContextAccessor httpContext)
        {
            this.httpContext = httpContext;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
                UpdateAuditableEntities(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableEntities(DbContext context)
        {
            var items = context.ChangeTracker.Entries<EntityBase>().ToList();

            foreach (var item in items)
            {
                item.Entity.UpdatedAt = DateTime.UtcNow;
                item.Entity.EditorIp = httpContext?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                item.Entity.EditorUserAgent = httpContext?.HttpContext?.Request?.Headers[HttpUtil.UserAgentKey];
                item.Entity.EditorEmail = httpContext?.GetClaimValue(ClaimTypes.Email);

                if (item.State == EntityState.Added)
                {
                    item.Entity.CreatedAt = DateTime.UtcNow;
                    item.Entity.CreatorIp = httpContext?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    item.Entity.CreatorUserAgent = httpContext?.HttpContext?.Request?.Headers[HttpUtil.UserAgentKey];
                    item.Entity.CreatorEmail = httpContext?.GetClaimValue(ClaimTypes.Email);
                }
            }
        }
    }
}
