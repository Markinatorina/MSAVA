using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories
{
    public class IdentifiableRepository<T> : BaseRepository<T>, IIdentifiableRepository<T> where T : class, IIdentifiableDB
    {
        public IdentifiableRepository(BaseDataContext context) : base(context)
        {

        }

        public T? GetById(Guid id)
        {
            return _entities.AsNoTracking().SingleOrDefault(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _entities.AsNoTracking().SingleOrDefaultAsync(s => s.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
        }

        public T GetById(Guid id, params Expression<Func<T, object>>[] includes)
        {
            var query = _entities.AsNoTracking().AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query.SingleOrDefault(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
        }

        public async Task<T> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            var query = _entities.AsNoTracking().AsQueryable();

            // Apply each include expression to the query
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.SingleOrDefaultAsync(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
        }

        public IQueryable<T> GetRangeByIds(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any())
                return Enumerable.Empty<T>().AsQueryable();

            return _entities.AsNoTracking().Where(entity => idList.Contains(entity.Id));
        }

        public void DeleteById(Guid id)
        {
            var entity = _entities.SingleOrDefault(s => s.Id == id);
            if (entity != null)
            {
                _entities.Remove(entity);
            }
            else
            {
                throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
            }
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            var entity = await _entities.SingleOrDefaultAsync(s => s.Id == id);
            if (entity != null)
            {
                _entities.Remove(entity);
            }
            else
            {
                throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
            }
        }

        public void DeleteRangeByIds(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any())
                return;
            var entitiesToDelete = _entities.Where(entity => idList.Contains(entity.Id)).ToList();
            if (entitiesToDelete.Any())
            {
                _entities.RemoveRange(entitiesToDelete);
            }
            else
            {
                throw new KeyNotFoundException($"Repository: Entities with ids {string.Join(", ", idList)} not found.");
            }
        }

        public async Task DeleteRangeByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any())
                return;
            var entitiesToDelete = await _entities.Where(entity => idList.Contains(entity.Id)).ToListAsync();
            if (entitiesToDelete.Any())
            {
                _entities.RemoveRange(entitiesToDelete);
            }
            else
            {
                throw new KeyNotFoundException($"Repository: Entities with ids {string.Join(", ", idList)} not found.");
            }
        }
    }
}
