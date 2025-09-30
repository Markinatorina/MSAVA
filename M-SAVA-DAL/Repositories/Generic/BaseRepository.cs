using M_SAVA_DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories.Generic
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly BaseDataContext _context;
        protected readonly DbSet<T> _entities;

        public BaseRepository(BaseDataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entities = context.Set<T>();
        }

        public void SaveChangesAndDetach()
        {
            if (_context.ChangeTracker.HasChanges())
            {
                _context.SaveChanges();
            }

            _context.ChangeTracker.Clear();
        }

        public async Task SaveChangesAndDetachAsync()
        {
            if (_context.ChangeTracker.HasChanges())
            {
                await _context.SaveChangesAsync();
            }

            _context.ChangeTracker.Clear();
        }

        public IQueryable<T> GetAllAsTracked()
        {
            return _entities;
        }

        public IQueryable<T> GetAllAsReadOnly()
        {
            return _entities.AsNoTracking();
        }

        public void Insert(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Repository: Parameter entity is null.");

            var entry = _context.Entry(entity);
            if (entry.State != EntityState.Detached)
                throw new InvalidOperationException("Repository: Entity is already being tracked. Detach before insert.");

            _entities.Add(entity);
        }

        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Repository: Parameter entity is null.");

            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Modified)
            {
                // already marked modified; nothing to do
                return;
            }

            if (entry.State == EntityState.Detached)
            {
                // Attach and mark modified
                _entities.Update(entity);
                return;
            }

            // For other tracked states set to Modified explicitly
            entry.State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Repository: Parameter entity is null.");

            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Deleted)
                throw new InvalidOperationException("Repository: Entity is already marked for deletion.");

            // Remove will attach when necessary and mark Deleted
            _entities.Remove(entity);
        }

        public void AddRange(IEnumerable<T> entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Repository: Parameter entity is null.");

            if (entity is ICollection<T> col && col.Count == 0) return;
            if (!entity.Any()) return;

            _entities.AddRange(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities), "Repository: Parameter entity is null.");
            
            if (entities is ICollection<T> col && col.Count == 0) return;
            if (!entities.Any()) return;

            _entities.RemoveRange(entities);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities), "Repository: Parameter entity is null.");
            if (entities is ICollection<T> col && col.Count == 0) return;
            if (!entities.Any()) return;

            // Attach each and mark modified to avoid surprising bulk semantics of UpdateRange.
            foreach (var e in entities)
            {
                var entry = _context.Entry(e);
                if (entry.State == EntityState.Detached)
                {
                    _context.Attach(e);
                    _context.Entry(e).State = EntityState.Modified;
                }
                else
                {
                    entry.State = EntityState.Modified;
                }
            }
        }

        public void Attach(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Repository: Parameter entity is null.");

            var entry = _context.Entry(entity);
            if (entry.State != EntityState.Detached)
                throw new InvalidOperationException("Repository: Entity is already being tracked.");

            _context.Attach(entity);
        }

        public void Detach(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Repository: Parameter entity is null.");

            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached) return; // no-op because already detached

            entry.State = EntityState.Detached;
        }

        public void DetachAll()
        {
            _context.ChangeTracker.Clear();
        }

        public void ChangeTrackingState(object entity, EntityState state)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Repository: Parameter entity is null.");
            if (!Enum.IsDefined(typeof(EntityState), state))
                throw new ArgumentException("Repository: Invalid EntityState value.", nameof(state));

            _context.Entry(entity).State = state;
        }

        public void MarkPropertyAsModified<TProperty>(T entity, Expression<Func<T, TProperty>> propertyExpression)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Repository: Parameter entity is null.");
            if (propertyExpression == null) throw new ArgumentNullException(nameof(propertyExpression), "Repository: Property expression is null.");

            _context.Entry(entity).Property(propertyExpression).IsModified = true;
        }
    }
}
