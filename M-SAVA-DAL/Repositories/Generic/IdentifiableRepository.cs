using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories.Generic
{
    public class IdentifiableRepository<T> : BaseRepository<T>, IIdentifiableRepository<T> where T : class, IIdentifiableDB
    {
        public IdentifiableRepository(BaseDataContext context) : base(context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context), "Repository: BaseDataContext cannot be null.");
        }

        public T GetById(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException("Repository: id cannot be empty.", nameof(id));

            var result = _entities.AsNoTracking().SingleOrDefault(s => s.Id == id);
            if (result == null)
                throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
            return result;
        }

        public T GetByIdAsTracked(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException("Repository: id cannot be empty.", nameof(id));

            var result = _entities.SingleOrDefault(s => s.Id == id);
            if (result == null)
                throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
            return result;
        }

        public T GetById(Guid id, params Expression<Func<T, object>>[] includes)
        {
            if (id == Guid.Empty) throw new ArgumentException("Repository: id cannot be empty.", nameof(id));

            IQueryable<T> query = _entities.AsNoTracking();

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    if (include == null) continue;
                    query = query.Include(include);
                }
            }

            var result = query.SingleOrDefault(s => s.Id == id);
            if (result == null)
                throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
            return result;
        }

        public IQueryable<T> GetRangeByIds(IEnumerable<Guid> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids), "Repository: ids cannot be null.");
            var idList = ids as IList<Guid> ?? ids.ToList();
            if (!idList.Any())
                return Enumerable.Empty<T>().AsQueryable();

            return _entities.AsNoTracking().Where(entity => idList.Contains(entity.Id));
        }

        public void DeleteById(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException("Repository: id cannot be empty.", nameof(id));
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

        public void DeleteRangeByIds(IEnumerable<Guid> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids), "Repository: ids cannot be null.");
            var idList = ids as IList<Guid> ?? ids.ToList();
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
    }
}
