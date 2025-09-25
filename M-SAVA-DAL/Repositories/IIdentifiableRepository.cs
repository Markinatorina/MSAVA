using M_SAVA_DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories
{
    public interface IIdentifiableRepository<T> : IBaseRepository<T> where T : class, IIdentifiableDB
    {
        T GetById(Guid id);
        Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        T GetByIdAsTracked(Guid id);

        T GetById(Guid id, params Expression<Func<T, object>>[] includes);
        Task<T> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);

        IQueryable<T> GetRangeByIds(IEnumerable<Guid> ids);

        void DeleteById(Guid id);
        Task DeleteByIdAsync(Guid id);

        void DeleteRangeByIds(IEnumerable<Guid> ids);
        Task DeleteRangeByIdsAsync(IEnumerable<Guid> ids);
    }
}
