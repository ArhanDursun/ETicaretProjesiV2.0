using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Application.Interfaces.Services;
using ETicaretProjesiV2._0.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR;
using MassTransit;
using ETicaretProjesiV2._0.Application.Events;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IGenericRepository<UserFavorite> _favoriteRepo;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly INotificationService _notificationService;
        private readonly IPublishEndpoint _publishEndpoint;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository,IGenericRepository<UserFavorite> favoriteRepo,
            IDistributedCache cache,IConnectionMultiplexer redis,INotificationService notificationService,IPublishEndpoint publishEndpoint)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _favoriteRepo = favoriteRepo;
            _cache = cache;
            _redis = redis;
            _notificationService = notificationService;
            _publishEndpoint = publishEndpoint;
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
                ProductImages = new List<ProductImage>() ,
                IsShowcase = true, 
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

            await _cache.RemoveAsync("showcase_all_products");
        }

        public async Task<bool> DeleteProductByAdminAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();
            await _cache.RemoveAsync("showcase_all_products");
            return true;
        }

        public async Task DeleteProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new Exception("Product not found");
            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();

            await _cache.RemoveAsync("showcase_all_products");
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {

            var query = _productRepository.GetAllAsQueryable(tracking: false);
           
            return await query.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuanity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                SellerId = p.SellerId,
                SellerName = p.Seller.UserName,
                DiscountedPrice=p.DiscountedPrice,
                DiscountEndDate = p.DiscountEndDate,
                DiscountPercentage = p.DiscountPercentage,
                Images = p.ProductImages != null ? p.ProductImages.Select(img => img.ImagePath).ToList() : new List<string>(),

                AverageStar = p.ProductComments.Any()
                ? Math.Round(p.ProductComments.Average(c => c.StarCount), 1)
                : 0,

                CommentCount = p.ProductComments.Count
            }).ToListAsync();
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

        public async Task<PaginatedResult<ProductDto>> GetFilteredProductsAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, string? searchTerm, bool onlyInStock, string? orderBy, int PageNumber, int PageSize)
        {
            var query = _productRepository.GetAllAsQueryable(tracking: false);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);
            if(minPrice.HasValue)
                query = query.Where(p=> p.Price >= minPrice.Value);
            if(maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);
            if(!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p=>p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            if(onlyInStock)
                query = query.Where(p => p.StockQuanity > 0);
            int totalCount = await query.CountAsync();

            query = orderBy switch
            {
                "priceAsc" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            var productDtos = await query.Skip((PageNumber -1 )*PageSize)
                                         .Take(PageSize)
                                         .Select(p=> new ProductDto
                                         {
                                             Id = p.Id,
                                             Name = p.Name,
                                             Description = p.Description,
                                             Price = p.Price,
                                             StockQuantity = p.StockQuanity,
                                             CategoryName = p.Category.Name,
                                             SellerId = p.SellerId,
                                             DiscountedPrice = p.DiscountedPrice,
                                             DiscountPercentage = p.DiscountPercentage,
                                             DiscountEndDate = p.DiscountEndDate,
                                             Images = p.ProductImages.Select(img => img.ImagePath).ToList(),

                                             AverageStar = p.ProductComments.Any()
                                             ? Math.Round(p.ProductComments.Average(c => c.StarCount), 1)
                                               : 0,
                                             CommentCount = p.ProductComments.Count(),
                                         }).ToListAsync();

            return new PaginatedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize),
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

        public async Task<ProductDto> GetProductByIdAsync(Guid id, string? userId = null)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(id);
            if (product == null)
                throw new Exception("Product not found");

            var db = _redis.GetDatabase();
            double newScore = await db.SortedSetIncrementAsync("trending_products", id.ToString(), 1);

            if (newScore % 10 == 0)
            {
                await _notificationService.SendTrendUpdateAsync($"{product.Name} şu an çok popüler! 🔥");
            }

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

        public async Task<PagedResult<ProductListResponseDto>> GetShowcaseProductsAsync(PaginationParams userParams)
        {
            
            string cacheKey = "showcase_all_products";
            List<ProductListResponseDto> allShowcaseProducts;

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
              
                allShowcaseProducts = JsonSerializer.Deserialize<List<ProductListResponseDto>>(cachedData);
            }
            else
            {
               
                var query = _productRepository.Where(p => p.IsShowcase && !p.IsDeleted);

                allShowcaseProducts = await query.OrderByDescending(p => p.CreatedDate)
                    .Select(p => new ProductListResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Description = p.Description,
                        CategoryName = p.Category.Name,
                        StockQuanity = p.StockQuanity,
                        Images = p.ProductImages.Select(img => img.ImagePath).ToList(),
                        AverageStar = p.ProductComments.Any() ? p.ProductComments.Average(c => c.StarCount) : 0,
                        CommentCount = p.ProductComments.Count(),
                        DiscountedPrice = p.DiscountedPrice,
                        DiscountEndDate = p.DiscountEndDate,
                        DiscountPercentage = p.DiscountPercentage,
                        SellerName = p.Seller.UserName
                    }).ToListAsync();

                
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(allShowcaseProducts), cacheOptions);
            }

            
            var totalCount = allShowcaseProducts.Count;
            var pagedItems = allShowcaseProducts
                .Skip((userParams.PageNumber - 1) * userParams.PageSize)
                .Take(userParams.PageSize)
                .ToList();

            return new PagedResult<ProductListResponseDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = userParams.PageNumber,
                PageSize = userParams.PageSize,
            };
        }

        public async Task<List<ProductListResponseDto>> GetTrendingProductsAsync()
        {
            var db = _redis.GetDatabase();

           
            var topProductIdsRedis = await db.SortedSetRangeByScoreAsync(
                key: "trending_products",
                start: double.PositiveInfinity, 
                stop: 50,                       
                exclude: Exclude.None,
                order: StackExchange.Redis.Order.Descending,
                skip: 0,
                take: 20                        
            );

            if (topProductIdsRedis.Length == 0)
                return new List<ProductListResponseDto>();

            var productIds = topProductIdsRedis.Select(id => Guid.Parse(id.ToString())).ToList();

            var trendingProducts = await _productRepository.Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                .Select(p => new ProductListResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    CategoryName = p.Category.Name,
                    StockQuanity = p.StockQuanity,
                    Images = p.ProductImages.Select(img => img.ImagePath).ToList(),
                    AverageStar = p.ProductComments.Any() ? p.ProductComments.Average(c => c.StarCount) : 0,
                    CommentCount = p.ProductComments.Count(),
                    DiscountedPrice = (p.DiscountEndDate.HasValue && p.DiscountEndDate >= DateTime.UtcNow)
                      ? p.DiscountedPrice
                      : null,
                    DiscountPercentage = (p.DiscountEndDate.HasValue && p.DiscountEndDate >= DateTime.UtcNow)
                         ? p.DiscountPercentage
                         : null,
                    DiscountEndDate = (p.DiscountEndDate.HasValue && p.DiscountEndDate >= DateTime.UtcNow)
                      ? p.DiscountEndDate
                      : null,
                    SellerName = p.Seller.UserName
                }).ToListAsync();

            
            var sortedTrendingProducts = trendingProducts.OrderBy(p => productIds.IndexOf(p.Id))
                .ToList();

            return sortedTrendingProducts;
        }

        public async Task UpdateProductAsync(Guid id, ProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new Exception("Product not found");
            decimal oldEffectivePrice = product.DiscountedPrice ?? product.Price;
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

            decimal newEffectivePrice = product.DiscountedPrice ?? product.Price;
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            if (newEffectivePrice < oldEffectivePrice)
            {
                await _publishEndpoint.Publish(new ProductPriceChangedEvent
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    OldPrice = oldEffectivePrice,
                    NewPrice = newEffectivePrice,
                });  
            }
            await _cache.RemoveAsync("showcase_all_products");
        }
    }
}