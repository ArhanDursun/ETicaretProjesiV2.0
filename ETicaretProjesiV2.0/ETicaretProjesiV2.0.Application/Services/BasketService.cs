using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class BasketService : IBasketService
    {
        private readonly IDistributedCache _cache;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<Offer> _offerRepo;

        public BasketService(IDistributedCache cache, IGenericRepository<Product> productRepo, IGenericRepository<Offer> offerRepo)
        {
            _cache = cache;
            _productRepo = productRepo;
            _offerRepo = offerRepo;
        }

        public async Task AddItemToBasketAsync(Guid userId, AddItemToBasketDto dto)
        {
            var product = await _productRepo.Where(p => p.Id == dto.ProductId)
                .Include(p => p.ProductImages).FirstOrDefaultAsync();

            if (product == null) throw new Exception("Ürün Bulunamadı");

            decimal activePrice = product.Price;
            if(product.DiscountedPrice.HasValue && product.DiscountEndDate.HasValue && product.DiscountEndDate.Value > DateTime.UtcNow)
            {
                activePrice = product.DiscountedPrice.Value;
            }

            var cacheKey = GetBasketKey(userId);
            var basketJson = await _cache.GetStringAsync(cacheKey);

            BasketDto basket = string.IsNullOrEmpty(basketJson)
                ? new BasketDto { BasketId = userId}:JsonSerializer.Deserialize<BasketDto>(basketJson);

            var existingItem = basket.Items.FirstOrDefault(i=>i.ProductId ==dto.ProductId);
            int quantityToAdd = dto.Quantity>0 ? dto.Quantity : 1;
            int currentInBasket = existingItem?.Quantity ?? 0;
            int totalRequestedQuantity = currentInBasket + quantityToAdd;

            if(totalRequestedQuantity> product.StockQuanity)
            {
                throw new Exception($"Stok Yetersiz bu üründen en fazla {product.StockQuanity} adet alabilirsiniz. Sepetinizde zaten {currentInBasket} adet var.");
            }
            if(existingItem != null)
            {
                existingItem.Quantity = quantityToAdd;
                existingItem.UnitPrice = activePrice;
            }
            else
            {
                basket.Items.Add(new BasketItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = activePrice,
                    Quantity = quantityToAdd,
                    IsOfferItem = false,
                    Images = product.ProductImages?.Select(x => x.ImagePath).ToList() ?? new List<string>()

                });
            }

            basket.TotalBasketPrices = basket.Items.Sum(x=>x.Quantity *x.UnitPrice);

            var options = new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(30) };
            await _cache.SetStringAsync(cacheKey,JsonSerializer.Serialize(basket),options);
        }

        public async Task ClearBasketAsync(Guid userId)
        {
            var cacheKey = GetBasketKey(userId);
            await _cache.RemoveAsync(cacheKey);
        }

        public async Task<BasketDto> GetBasketAsync(Guid userId)
        {
            var basketJson = await _cache.GetStringAsync(GetBasketKey(userId));
            if (string.IsNullOrEmpty(basketJson)) return new BasketDto { BasketId = userId };

            var basket = JsonSerializer.Deserialize<BasketDto>(basketJson);

            
            if (basket.Items == null || !basket.Items.Any()) return basket;

           
            var productIds = basket.Items.Select(x => x.ProductId).ToList();
            var currentProducts = await _productRepo.Where(p => productIds.Contains(p.Id)).ToListAsync();

            var finalItems = new List<BasketItemDto>();

            foreach (var item in basket.Items)
            {
               
                var currentProduct = currentProducts.FirstOrDefault(p => p.Id == item.ProductId);
                if (currentProduct == null) continue; 

              
                decimal activePrice = currentProduct.Price;
                if (currentProduct.DiscountedPrice.HasValue
                    && currentProduct.DiscountEndDate.HasValue
                    && currentProduct.DiscountEndDate.Value > DateTime.UtcNow)
                {
                    activePrice = currentProduct.DiscountedPrice.Value;
                }

                var acceptedOffer = await _offerRepo.Where(o => o.ProductId == item.ProductId
                                                             && o.BuyerId == userId
                                                             && o.Status == OfferStatus.Accepted)
                                                    .OrderByDescending(o => o.CreatedDate)
                                                    .FirstOrDefaultAsync();

                if (acceptedOffer != null)
                {
                    int offerQty = Math.Min(item.Quantity, acceptedOffer.Quantity);
                    if (offerQty > 0)
                    {
                        var offerItem = JsonSerializer.Deserialize<BasketItemDto>(JsonSerializer.Serialize(item)); 
                        offerItem.ProductName += " (Teklifli)";
                        offerItem.UnitPrice = acceptedOffer.OfferedPrice;
                        offerItem.Quantity = offerQty;
                        offerItem.IsOfferItem = true;
                        finalItems.Add(offerItem);
                    }

                    int remainingQty = item.Quantity - offerQty;
                    if (remainingQty > 0)
                    {
                        var normalItem = JsonSerializer.Deserialize<BasketItemDto>(JsonSerializer.Serialize(item)); 
                        normalItem.Quantity = remainingQty;
                        normalItem.UnitPrice = activePrice; 
                        finalItems.Add(normalItem);
                    }
                }
                else
                {
                    
                    item.UnitPrice = activePrice;
                    finalItems.Add(item);
                }
            }

            basket.Items = finalItems;
            basket.TotalBasketPrices = basket.Items.Sum(x => x.Quantity * x.UnitPrice);

           
            var options = new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(30) };
            await _cache.SetStringAsync(GetBasketKey(userId), JsonSerializer.Serialize(basket), options);

            return basket;
        }

        public async Task RemoveItemFromBasketAsync(Guid userId, Guid productId)
        {
            var cacheKey = GetBasketKey(userId);
            var basketJson = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(basketJson)) return;
            var basket = JsonSerializer.Deserialize<BasketDto>(basketJson);
            var itemToRemove = basket.Items.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove != null)
            {
                basket.Items.Remove(itemToRemove);
                basket.TotalBasketPrices = basket.Items.Sum(x => x.Quantity * x.UnitPrice);

                var options = new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(30) };
                await _cache.SetStringAsync(cacheKey,JsonSerializer.Serialize(basket),options);
            }
        }

        private string GetBasketKey(Guid userId) => $"basket_{userId}";
    }
}

