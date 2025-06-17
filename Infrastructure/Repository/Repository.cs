using Entities.EntityBase;
using Infrastructure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class, IEntityBase
    {
        private readonly DbSet<T> _dbSet;
        private readonly DbContext _dbContext;
        private readonly IHttpContextAccessor _context;

        public Repository(DbContext dbContext, IHttpContextAccessor context)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
            _context = context;
        }
        public virtual async Task<bool> CheckExist(Expression<Func<T, bool>> predicate, Ref<CheckError> checkError = null, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.Set<T>().AnyAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                if (checkError != null)
                {
                    checkError.Value = new CheckError() { IsError = true, Exception = ex, Message = ex.Message };
                }
                return false;
            }
        }
        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<T> GetById(object id, Ref<CheckError> checkError = null)
        {
            try
            {
                if (id == null) return null;
                return await _dbContext.Set<T>().FindAsync(id);
            }
            catch (Exception ex)
            {
                if (checkError != null)
                {
                    checkError.Value = new CheckError() { IsError = true, Exception = ex, Message = ex.Message };
                }
                throw ex;
            }
        }

        public virtual async Task<int> GetCount(System.Linq.Expressions.Expression<Func<T, bool>> predicate, Ref<CheckError> checkError = null)
        {
            try
            {
                return await _dbContext.Set<T>().Where(predicate).CountAsync();
            }
            catch (Exception ex)
            {
                if (checkError != null)
                {
                    checkError.Value = new CheckError() { IsError = true, Exception = ex, Message = ex.Message };
                }
                return -1;
            }
        }

        public IQueryable<T> GetQueryable()
        {
            return _dbContext.Set<T>();
        }

        public IQueryable<T> GetQueryable(System.Linq.Expressions.Expression<Func<T, bool>> condition)
        {
            return _dbSet.Where(condition);
        }

        public void Insert(T entity)
        {
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            entity.CreatedBy = this.CurrentUserId;
            entity.CreatedDate = DateTime.Now;
            entity.IsActive = true;
            _dbSet.Add(entity);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Update(T entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _dbContext.Entry(entityToUpdate).State = EntityState.Modified;
            entityToUpdate.UpdatedBy = this.CurrentUserId;
            entityToUpdate.UpdatedDate = DateTime.Now;
        }

        protected Guid CurrentUserId
        {
            get
            {
                ClaimsIdentity? identity = _context.HttpContext.User.Identity as ClaimsIdentity;
                var userClaim = identity?.Claims.SingleOrDefault(x => x.Type.Equals("Id"));
                if (userClaim != null)
                {
                    return Guid.Parse(userClaim.Value);
                }

                return Guid.Empty;
            }
        }
    }
}
