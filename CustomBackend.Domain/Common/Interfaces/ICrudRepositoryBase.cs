using CustomBackend.Domain.Common.Models;
using CustomBackend.Infra.Dtos.Result;
using System.Linq.Expressions;

namespace CustomBackend.Domain.Common.Interfaces
{
    public interface ICrudRepositoryBase<TEntity> : IRepositoryBase where TEntity : EntityBase
    {
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetAsync(Guid id);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        IQueryable<TEntity> Query();
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate);

        IQueryable<TEntity> Include<TProperty>(IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> navigationPropertyPath);

        Task<PagedResult<TEntity>> PageAsync(Expression<Func<TEntity, bool>> predicate, int? page = 1, int? pageSize = null);
        Task<PagedResult<T>> PageAsync<T>(IQueryable<T> query, int? page = 1, int? pageSize = null);

        Task CommitAsync();
        void Rollback();
        void Rollback<T>();

        Task SaveAndCommitAsync(TEntity entity/*, string userEmail = null, string ip = null, string userAgent = null*/);
        Task SaveAsync(TEntity entity/*, string userEmail = null, string ip = null, string userAgent = null*/);
        Task DeleteAndCommitAsync(params Guid[] ids);
        Task DeactivateAndCommitAsync(params Guid[] ids);
        Task ActivateAndCommitAsync(params Guid[] ids);

        T[] ToArrayWithNoLock<T>(IQueryable<T> query);
        TResult MaxWithNoLock<TSource, TResult>(IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector);
    }
}
