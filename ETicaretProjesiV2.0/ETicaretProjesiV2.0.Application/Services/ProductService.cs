using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Application.Interfaces.Services;
using ETicaretProjesiV2._0.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IGenericRepository<UserFavorite> _favoriteRepo;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository,IGenericRepository<UserFavorite> favoriteRepo)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _favoriteRepo = favoriteRepo;
        }

        public async Task CreateProductAsync(ProductDto dto, Guid SellerId)
        {
            var categoryExists = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (categoryExists == null)
                throw new Exception("Category not found");

            
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuanity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                SellerId = SellerId,
                ProductImages = new List<ProductImage>() 
            };

            if (dto.ImageFiles != null && dto.ImageFiles.Any())
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                foreach (var file in dto.ImageFiles)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var newImageName = Guid.NewGuid().ToString() + extension;
                    var exactPath = Path.Combine(path, newImageName);

                    using (var stream = new FileStream(exactPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    
                    product.ProductImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ImagePath = "/images/" + newImageName,
                        ProductId = product.Id,
                        IsMain = product.ProductImages.Count == 0 
                    });
                }
            }
            else
            {
                
                product.ProductImages.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ImagePath = "/images/default-product.jpg",
                    ProductId = product.Id,
                    IsMain = true
                });
            }

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteProductByAdminAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task DeleteProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new Exception("Product not found");
            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetProductsWithCategoryAndSellerAsync();
            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuanity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                SellerId = p.SellerId,
                SellerName = p.Seller?.UserName,
                DiscountedPrice=p.DiscountedPrice,
                DiscountEndDate = p.DiscountEndDate,
                DiscountPercentage = p.DiscountPercentage,
                Images = p.ProductImages != null ? p.ProductImages.Select(img => img.ImagePath).ToList() : new List<string>(),

                AverageStar = p.ProductComments.Any()
                ? Math.Round(p.ProductComments.Average(c => c.StarCount), 1)
                : 0,

                CommentCount = p.ProductComments.Count
            }).ToList();
        }

        public async Task<List<ProductDto>> GetAllProductsForAdminAsync()
        {
            var products = await _productRepository.GetProductsForAdminWithSellerAsync();
            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                SellerName = p.Seller?.UserName,
                DiscountPercentage = p.DiscountPercentage,
                DiscountEndDate =p.DiscountEndDate,
                DiscountedPrice = p.DiscountedPrice,
                Images = p.ProductImages != null ? p.ProductImages.Select(img => img.ImagePath).ToList() : new List<string>()
            }).ToList();
        }

        public async Task<PaginatedResultDto<ProductDto>> GetFilteredProductsAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, string? searchTerm, bool onlyInStock, string? orderBy, int PageNumber, int PageSize)
        {
            var result = await _productRepository.GetFilteredProductsAsync(categoryId, minPrice, maxPrice, searchTerm, onlyInStock, orderBy, PageNumber, PageSize);
            var productsDtos = result.Products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuanity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                SellerId = p.SellerId,
                SellerName = p.Seller?.UserName,
                DiscountedPrice = p.DiscountedPrice,
                DiscountEndDate = p.DiscountEndDate,
                DiscountPercentage = p.DiscountPercentage,
                Images = p.ProductImages != null ? p.ProductImages.Select(img => img.ImagePath).ToList() : new List<string>(),
                AverageStar = p.ProductComments != null && p.ProductComments.Any()
            ? Math.Round(p.ProductComments.Average(c => c.StarCount), 1)
            : 0,

                CommentCount = p.ProductComments != null ? p.ProductComments.Count : 0
            }).ToList();

            int totalPages = (int)Math.Ceiling(result.totalCount / (double)PageSize);
            return new PaginatedResultDto<ProductDto>
            {
                Items = productsDtos,
                TotalCount = result.totalCount,
                TotalPages = totalPages,
                CurrentPages = PageNumber,
                PageSize = PageSize
            };
        }

        public async Task<IEnumerable<ProductListResponseDto>> GetMyProductsAsync(Guid sellerId)
        {
            var myProducts = await _productRepository.Where(o => o.SellerId == sellerId).OrderByDescending(x => x.CreatedDate).Select(p => new ProductListResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StockQuanity = p.StockQuanity,
                CategoryName = p.Category.Name,
                DiscountedPrice = p.DiscountedPrice,
                DiscountEndDate = p.DiscountEndDate,
                DiscountPercentage = p.DiscountPercentage,
                Images = p.ProductImages != null ? p.ProductImages.Select(img => img.ImagePath).ToList() : new List<string>()
            }).ToListAsync();

            return myProducts;
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id,string? userId = null)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(id);
            if (product == null)
                throw new Exception("Product not found");

            bool isFav = false;
            if (!string.IsNullOrEmpty(userId))
            {
                var uId = Guid.Parse(userId);
                isFav = await _favoriteRepo.Where(f => f.UserId == uId && f.ProductId == id).AnyAsync();
            }
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuanity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                DiscountedPrice = product.DiscountedPrice,
                DiscountEndDate = product.DiscountEndDate,
                DiscountPercentage = product.DiscountPercentage,
                SellerId = product.SellerId,
                SellerName = product.Seller?.UserName ?? "Bilinmeyen Satıcı",
                Images = product.ProductImages != null ? product.ProductImages.Select(img => img.ImagePath).ToList() : new List<string>(),
                IsFavorited = isFav,
            };
        }

        public async Task<List<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId);
            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryName = p.Category?.Name,
                DiscountPercentage = p.DiscountPercentage,
                DiscountEndDate = p.DiscountEndDate,
                DiscountedPrice = p.DiscountedPrice,
                Images = p.ProductImages != null ? p.ProductImages.Select(img => img.ImagePath).ToList() : new List<string>()
            }).ToList();
        }

        public async Task UpdateProductAsync(Guid id, ProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new Exception("Product not found");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuanity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            if(dto.DiscountPercentage.HasValue && dto.DiscountPercentage.Value > 0)
            {
                product.DiscountPercentage = dto.DiscountPercentage;
                product.DiscountEndDate = dto.DiscountEndDate;

                product.DiscountedPrice = product.Price -(product.Price * dto.DiscountPercentage.Value / 100m);
            }
            else
            {
                product.DiscountPercentage = null;
                product.DiscountEndDate = null;
                product.DiscountedPrice = null;
            }

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
        }
    }
}