using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity 
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();

        IQueryable<T> Where(Expression<Func<T,bool>> expression);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task<int> SaveAsync();

    }
}
