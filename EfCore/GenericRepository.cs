using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using postalCrisisV2.Core;
using projecthelper.Result;
using System.Linq.Expressions;

namespace projecthelper.EfCore;


public class Repository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IBaseEntity
{
    public readonly DbContext _ctx;
    public readonly DbSet<TEntity> _set;
    private readonly ILogger<Repository<TEntity>> _logger;

    public Repository(DbContext context, ILogger<Repository<TEntity>> logger)
    {
        _ctx = context;
        _logger = logger;
        _set = context.Set<TEntity>();
    }

    public async Task<Result<(TEntity, bool isCreated)>> Upsert(TEntity dashboard)
    {
        try
        {
            EntityEntry<TEntity> entry;
            bool isCreated = false;
            if (dashboard.Id == Guid.Empty)
            {
                entry = await _set.AddAsync(dashboard);
                isCreated = true;
            }
            else
            {
                entry = _set.Update(dashboard);
            }

            await _ctx.SaveChangesAsync();
            return (entry.Entity, isCreated).isResultOk();
        }
        catch (Exception ex)
        {
            return Result<(TEntity, bool)>.setFail(ex, _logger);
        }
    }

    public async Task<Result<TEntity>> InsertIfNotExist(TEntity dashboard)
    {
        try
        {
            _ctx.Entry(dashboard).State = EntityState.Detached;
            var entity = await (await Get(dashboard.Id))
                .AsyncMap(
                async dash =>
                {
                    dash = dashboard;
                    return await Update(dashboard);
                },
                async ex => await Insert(dashboard));

            await _ctx.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            return Result<TEntity>.setFail(ex, _logger);
        }
    }

    public async Task<Result<TEntity>> Insert(TEntity dashboard)
    {
        try
        {
            EntityEntry<TEntity> entry;
            entry = await _set.AddAsync(dashboard);
            await _ctx.SaveChangesAsync();

            return entry.Entity.isResultOk();
        }
        catch (Exception ex)
        {
            return Result<TEntity>.setFail(ex, _logger);
        }
    }

    public async Task<Result<TEntity>> Update(TEntity dashboard)
    {
        try
        {
            EntityEntry<TEntity> entry;
            entry = _set.Update(dashboard);
            await _ctx.SaveChangesAsync();

            return entry.Entity.isResultOk();
        }
        catch (Exception ex)
        {
            return Result<TEntity>.setFail(ex, _logger);
        }
    }

    public async Task<Result<TEntity>> Update(Guid id, Action<TEntity> update)
    {
        return (await (await Get(id)).AsyncMap(async (existing) =>
        {
            update(existing);
            return await Update(existing);
        }, _logger));
    }

    public async Task<Result<TEntity>> Get(Guid id)
    {
        return await GetFirstByComparer(entity => entity.Id == id);
    }

    public async Task<Result<TEntity>> GetFirstByComparer(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            var entity = await _set.AsNoTracking().FirstOrDefaultAsync(predicate);
            if (entity == null)
            {
                return Result<TEntity>.setFail(new NullReferenceException(), _logger);
            }
            else
            {
                return entity.isResultOk();
            }
        }
        catch (Exception ex)
        {
            return ex.isResultFail<TEntity>(_logger);
        }
    }
    public async Task<Result<IEnumerable<TEntity>>> GetAllByComparer(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {

            var entity = await _set.Where(predicate).ToListAsync();
            if (entity == null)
            {
                return Result<IEnumerable<TEntity>>.setFail(new NullReferenceException(), _logger);
            }
            else
            {
                return ((IEnumerable<TEntity>)entity).isResultOk();
            }
        }
        catch (Exception ex)
        {
            return ex.isResultFail<IEnumerable<TEntity>>(_logger);
        }
    }

    public async Task Delete(Guid id)
    {
        var entity = Get(id);
        _ctx.Remove(entity);
        await _ctx.SaveChangesAsync();
    }
    public async Task Delete(TEntity entity)
    {
        _ctx.Remove(entity);
        await _ctx.SaveChangesAsync();
    }

    public async Task Delete(Expression<Func<TEntity, bool>> predicate)
    {
        var entity = await _set.Where(predicate).ToListAsync();
        if (entity != null)
        {
            _ctx.RemoveRange(entity);
            await _ctx.SaveChangesAsync();
        }
    }
}
