using CustomBackend.Domain.Common.Interfaces;
using CustomBackend.Domain.Common.Models;
using CustomBackend.Infra.Dtos.Result;
using CustomBackend.Repository.Db.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Transactions;

namespace CustomBackend.Repository.Db.Repositories.Common
{
    public abstract class CrudRepositoryBase<TEntity> : RepositoryDbBase, ICrudRepositoryBase<TEntity> where TEntity : EntityBase
    {
        protected MainDbContext context;
        protected DbSet<TEntity> set;

        public CrudRepositoryBase(MainDbContext context)
        {
            this.context = context;
            this.set = this.context.Set<TEntity>();
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate) => Query().AnyAsync(predicate);

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate) => Query().FirstOrDefaultAsync(predicate);

        public Task<TEntity> GetAsync(Guid id) => FirstOrDefaultAsync(e => e.Id == id);



        public Task<PagedResult<TEntity>> PageAsync(Expression<Func<TEntity, bool>> predicate, int? page, int? pageSize = null) => PageAsync(this.Query(predicate), page, pageSize);

        public async Task<PagedResult<T>> PageAsync<T>(IQueryable<T> query, int? page = 1, int? pageSize = null)
        {
            page = page ?? 1;
            var totalItems = query.Count();
            var totalPages = 1;
            PagedResult<T> result;

            if (pageSize.HasValue)
            {
                totalPages = (int)Math.Ceiling(totalItems / (decimal)pageSize);
                var startIndex = ((page.Value - 1) * pageSize.Value);
                var items = await query.Skip(startIndex).Take(pageSize.Value).ToArrayAsync();
                result = new PagedResult<T>(page.Value, totalPages, pageSize.Value, items.Length, totalItems, items);
            }
            else
            {
                var items = await query.ToArrayAsync();
                result = new PagedResult<T>(page.Value, totalPages, totalItems, items.Length, totalItems, items);
            }

            return result;
        }



        public IQueryable<TEntity> Query() => set.AsQueryable();

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate) => Query().Where(predicate);



        public IQueryable<TEntity> Include<TProperty>(IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> navigationPropertyPath) => source.Include(navigationPropertyPath);



        public virtual async Task SaveAndCommitAsync(TEntity entity)
        {
            await SaveAsync(entity);
            await CommitAsync();
        }

        public virtual async Task SaveAsync(TEntity entity)
        {
            if (entity.Id == default || entity.Id == Guid.Empty)
                await set.AddAsync(entity);
            else
                context.Entry(entity).State = EntityState.Modified;
        }

        public virtual async Task DeleteAndCommitAsync(params Guid[] ids)
        {
            await set.Where(e => ids.Contains(e.Id)).ExecuteDeleteAsync();
            await CommitAsync();
        }

        public virtual async Task DeactivateAndCommitAsync(params Guid[] ids)
        {
            set.ExecuteUpdate(s => s.SetProperty(e => e.Active, e => false));
            await CommitAsync();
        }

        public virtual async Task ActivateAndCommitAsync(params Guid[] ids)
        {
            set.ExecuteUpdate(s => s.SetProperty(e => e.Active, e => true));
            await CommitAsync();
        }

        public Task CommitAsync() => context.SaveChangesAsync();

        public void Rollback() => RollbackEntries(ListPendingEntries());

        public void Rollback<T>() => RollbackEntries(ListPendingEntries().Where(x => x.State != EntityState.Unchanged && x.Entity.GetType() == typeof(T)).ToArray());

        public T[] ToArrayWithNoLock<T>(IQueryable<T> query)
        {
            T[] result = default;

            using (var scope = CreateTransaction())
            {
                result = query.ToArray();
                scope.Complete();
            }

            return result;
        }

        public TResult MaxWithNoLock<TSource, TResult>(IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector)
        {
            TResult result = default;

            using (var scope = CreateTransaction())
            {
                result = query.Max(selector);
                scope.Complete();
            }

            return result;
        }

        private EntityEntry[] ListPendingEntries() => context
                .ChangeTracker
                .Entries()
                .Where(x => x.State != EntityState.Unchanged)
                .ToArray();

        private void RollbackEntries(EntityEntry[] entries)
        {
            Parallel.ForEach(entries, entry =>
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            });
        }

        private static TransactionScope CreateTransaction() => new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled
        );
    }
}
