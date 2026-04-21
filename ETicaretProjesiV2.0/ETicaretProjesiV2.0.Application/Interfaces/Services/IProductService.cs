using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(Guid id,string? userId = null);
        Task CreateProductAsync(ProductDto dto,Guid SellerId);
        Task DeleteProductByIdAsync(Guid id);
        Task UpdateProductAsync(Guid id, ProductDto dto);
        Task<List<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
        Task<PaginatedResult<ProductDto>> GetFilteredProductsAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, string? searchTerm, bool onlyInStock,string? orderBy,int PageNumber,int PageSize);
        Task<List<ProductDto>> GetAllProductsForAdminAsync();
        Task<bool> DeleteProductByAdminAsync(Guid id);
        Task<IEnumerable<ProductListResponseDto>> GetMyProductsAsync(Guid sellerId);
        Task<PagedResult<ProductListResponseDto>> GetShowcaseProductsAsync(PaginationParams userParams);

    }
}
