using Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public interface IRepository<T>
    {
        void Insert(T entity);
        void Remove(T entity);
        void Update(T entityToUpdate);
        IQueryable<T> GetQueryable();
        Task<T> GetByIdAsync(object id, Ref<CheckError> checkError = null);
        IQueryable<T> GetQueryable(Expression<Func<T, bool>> condition);
        Task<bool> CheckExist(Expression<Func<T, bool>> predicate, Ref<CheckError> checkError = null, CancellationToken cancellationToken = default);
        Task<int> GetCount(Expression<Func<T, bool>> predicate, Ref<CheckError> checkError = null);
        Task<int> CommitAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
