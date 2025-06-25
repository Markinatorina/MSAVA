using M_SAVA_DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly BaseDataContext _context;
        protected readonly DbSet<T> _entities;
        public BaseRepository(BaseDataContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        public IQueryable<T> GetAll()
        {
            return _entities.AsQueryable();
        }

        public IQueryable<T> GetAllAsReadOnly()
        {
            return _entities.AsNoTracking();
        }

        public void Insert(T entity)
        {
            if (entity == null) throw new ArgumentNullException("Repository: Parameter entity is null.");

            _entities.Add(entity);
        }

        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException("Repository: Parameter entity is null.");

            _entities.Update(entity);
        }

        public void Delete(T entity)
        {
            if (entity == null) throw new ArgumentNullException("Repository: Parameter entity is null.");

            _entities.Remove(entity);
        }

        public void AddRange(IEnumerable<T> entity)
        {
            if (entity == null) throw new ArgumentNullException("Repository: Parameter entity is null.");

            _entities.AddRange(entity);
        }

        public void DeleteRange(IEnumerable<T> entity)
        {
            if (entity == null) throw new ArgumentNullException("Repository: Parameter entity is null.");

            _entities.RemoveRange(entity);
        }

        public void UpdateRange(IEnumerable<T> entity)
        {
            if (entity == null) throw new ArgumentNullException("Repository: Parameter entity is null.");

            _entities.UpdateRange(entity);
        }

        public void Attach(T entity)
        {
            _context.Attach(entity);
        }

        public void Detach(T entity)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        public void DetachAll()
        {
            _context.ChangeTracker.Clear();
        }

        public void ChangeTrackingState(object entity, EntityState state)
        {
            _context.Entry(entity).State = state;
        }

        public void MarkPropertyAsModified<TProperty>(T entity, Expression<Func<T, TProperty>> propertyExpression)
        {
            _context.Entry(entity).Property(propertyExpression).IsModified = true;
        }
    }
}
