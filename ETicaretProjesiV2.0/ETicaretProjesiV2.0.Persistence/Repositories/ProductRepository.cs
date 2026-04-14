using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ETicaretProjesiV2._0.Persistence.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<Product>> GetProductsWithCategoryAndSellerAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p=>p.ProductImages)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Product?> GetProductWithDetailsAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<List<Product>> GetProductsByCategoryIdAsync(Guid categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<(List<Product> Products,int totalCount)> GetFilteredProductsAsync(Guid? categoryId, decimal? minPrice,decimal? maxPrice ,string? searchTerm,
            bool onlyInStock = false,string? orderBy = null,int PageNumber = 1,int PageSize= 10)
        {
            var query = _context.Products.Include(p => p.Category).Include(p => p.Seller).Include(p => p.ProductImages).Include(p=>p.ProductComments).AsQueryable();

            if (categoryId.HasValue) { 
            query = query.Where(p=> p.CategoryId == categoryId.Value);
            }
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if(onlyInStock)
                query = query.Where(p => p.StockQuanity > 0);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            int totalCount = await query.CountAsync();
            query = orderBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedDate),
                _ => query
            };
            var products = await query.Skip((PageNumber - 1) * PageSize).Take(PageSize).AsNoTracking().ToListAsync();
            return (products, totalCount);
        }
        public async Task SaveChangesAsync()
        {   
          await _context.SaveChangesAsync();
        }

        public IQueryable<Product> GetQueryable()
        {
            return _context.Products.AsQueryable();
        }

        public async Task<List<Product>> GetProductsForAdminWithSellerAsync()
        {
           return await _context.Products
        .Include(p => p.Seller)
        .Include(p => p.ProductImages)
        .OrderByDescending(p => p.CreatedDate)
        .ToListAsync();
        }
    }
}