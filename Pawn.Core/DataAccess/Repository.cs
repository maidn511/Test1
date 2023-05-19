using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Pawn.Core.DataAccess
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly PawnEntities _dbContext;
        private DbSet<TEntity> _dbSet;

        public Repository(IUnitOfWork unitOfWork)
        {
            _dbContext = unitOfWork.DbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        public Repository(PawnEntities dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        public virtual int Count => _dbSet.Count();

        public virtual void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void InsertRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }
        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            foreach (var item in entities)
            {
                _dbSet.Attach(item);
                _dbContext.Entry(item).State = EntityState.Modified;
            }
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual void Delete(object id)
        {
            var entity = FindById(id);
            _dbSet.Remove(entity);
        }

        public virtual decimal Sum(Expression<Func<TEntity, decimal?>> condition)
        {
            return _dbSet.Sum(condition) ?? 0;
        }

        public virtual void Delete(Expression<Func<TEntity, bool>> condition)
        {
            IQueryable<TEntity> entities = Filter(condition);
            _dbSet.RemoveRange(entities);
        }

        public virtual IQueryable<TEntity> GetAllData() => _dbSet;

        public virtual TEntity FindById(object id) => _dbSet.Find(id);

        public virtual TEntity Find(Expression<Func<TEntity, bool>> condition) => _dbSet.FirstOrDefault(condition);

        public virtual IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> condition) => _dbSet.Where(condition);
        public virtual IQueryable<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> condition) => _dbSet.OrderBy(condition);
        public virtual IQueryable<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> condition) => _dbSet.OrderByDescending(condition);


        public virtual bool HasRows(Expression<Func<TEntity, bool>> condition) => _dbSet.Any(condition);

        private bool _disposed = false;

        /// <summary>  
        /// Dispose the object  
        /// </summary>  
        /// <param name="disposing">IsDisposing</param>  
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
                _dbContext?.Dispose();
        }

        /// <inheritdoc />
        /// <summary>  
        /// Dispose the object  
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
