using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories.Generic
{
    public interface IBaseRepository<T> where T : class
    {
        void SaveChangesAndDetach();
        Task SaveChangesAndDetachAsync();

        IQueryable<T> GetAllAsTracked();
        IQueryable<T> GetAllAsReadOnly();

        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);

        void AddRange(IEnumerable<T> entity);
        void DeleteRange(IEnumerable<T> entity);
        void UpdateRange(IEnumerable<T> entity);

        void Attach(T entity);
        void Detach(T entity);
        void DetachAll();

        void ChangeTrackingState(object entity, EntityState state);
        void MarkPropertyAsModified<TProperty>(T entity, Expression<Func<T, TProperty>> propertyExpression);
    }
}
