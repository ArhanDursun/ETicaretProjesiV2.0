using ETicaretProjesiV2._0.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<List<Product>> GetProductsWithCategoryAndSellerAsync();
        Task<Product?> GetProductWithDetailsAsync(Guid id);
        Task<List<Product>> GetProductsByCategoryIdAsync(Guid categoryId);
        Task SaveChangesAsync();
        Task<(List<Product> Products,int totalCount)> GetFilteredProductsAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice,
            string? searchTerm,bool onlyInStock = false,string? orderBy = null,int PageNumber=1 , int PageSize=10);
        IQueryable<Product> GetQueryable();
        Task<List<Product>> GetProductsForAdminWithSellerAsync();
    }
}
