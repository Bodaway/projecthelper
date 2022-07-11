using projecthelper.EfCore;
using projecthelper.Result;
using System.Linq.Expressions;

namespace projecthelper.EfCore;

public interface IGenericRepository<TEntity> where TEntity : IBaseEntity
{
    Task Delete(Guid id);
    Task<Result<TEntity>> Get(Guid id);
    Task<Result<(TEntity, bool isCreated)>> Upsert(TEntity dashboard);
    Task<Result<TEntity>> GetFirstByComparer(Expression<Func<TEntity, bool>> predicate);
    Task<Result<TEntity>> InsertIfNotExist(TEntity dashboard);
    Task<Result<TEntity>> Insert(TEntity dashboard);
    Task<Result<TEntity>> Update(TEntity dashboard);
    Task<Result<TEntity>> Update(Guid id, Action<TEntity> update);
    Task<Result<IEnumerable<TEntity>>> GetAllByComparer(Expression<Func<TEntity, bool>> predicate);
    Task Delete(Expression<Func<TEntity, bool>> predicate);
    Task Delete(TEntity entity);
}
