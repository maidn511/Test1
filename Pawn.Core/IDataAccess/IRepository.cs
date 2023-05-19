using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Pawn.Core.IDataAccess
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        int Count { get; }

        void Insert(TEntity entity);

        void InsertRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);

        void Delete(TEntity entity);

        void Delete(object id);

        void Delete(Expression<Func<TEntity, bool>> condition);
        decimal Sum(Expression<Func<TEntity, decimal?>> condition);
        IQueryable<TEntity> GetAllData();

        TEntity FindById(object id);

        TEntity Find(Expression<Func<TEntity, bool>> condition);

        IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> condition);
        bool HasRows(Expression<Func<TEntity, bool>> condition);
        IQueryable<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> condition);
        IQueryable<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> condition);
       
    }
}
