using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Common;
using ETicaretProjesiV2._0.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ETicaretProjesiV2._0.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {

        protected readonly ApplicationDbContext _dbContext;
        private readonly DbSet<T> _dbset;

        public GenericRepository(ApplicationDbContext dbContext){
            _dbContext = dbContext;
            _dbset = _dbContext.Set<T>();

        }
        public async Task AddAsync(T entity)
        {
             await _dbset.AddAsync(entity);
        }
        public void RemoveRange(IEnumerable<T> entities) {
            _dbContext.Set<T>().RemoveRange(entities);
        }
        public void Delete(T entity)
        {
            _dbset.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbset.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            if(_dbset is not null)
                return await _dbset.FirstOrDefaultAsync(x=>x.Id == id);
            throw new Exception("DbSet Null getirdi");
        }

        public async Task<int> SaveAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            _dbset.Update(entity);
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return _dbset.Where(expression);
        }
    }
}
