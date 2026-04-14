using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ETicaretProjesiV2._0.Persistence.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<List<Category>> GetAllWithSubCategoriesAsync()
        {
            return await _context.Categories.Include(c => c.SubCategories).ToListAsync();
        }

    }
}
