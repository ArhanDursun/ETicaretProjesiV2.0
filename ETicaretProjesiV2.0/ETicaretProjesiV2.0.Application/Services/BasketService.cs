using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class BasketService : IBasketService
    {
        private readonly IGenericRepository<Basket> _basketRepo;
        private readonly IGenericRepository<BasketItem> _basketItemRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<Offer> _offerRepo;

        public BasketService(IGenericRepository<Basket> basketRepo, IGenericRepository<BasketItem> basketItemRepo, IGenericRepository<Product> productRepo,
            IGenericRepository<Offer> offerRepo)
        {
            _basketRepo = basketRepo;
            _basketItemRepo = basketItemRepo;
            _productRepo = productRepo;
            _offerRepo = offerRepo;
        }

        public async Task AddItemToBasketAsync(Guid userId, AddItemToBasketDto dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new Exception("Ürün bulunamadı!");
            

            decimal activePrice = product.Price;
            if(product.DiscountedPrice.HasValue&& product.DiscountEndDate.HasValue && product.DiscountEndDate.Value > DateTime.UtcNow)
            {
                activePrice = product.DiscountedPrice.Value;
            }
            var basket = await _basketRepo.Where(b => b.AppUserId == userId)
                                          .Include(b => b.Items)
                                          .FirstOrDefaultAsync();
            if (basket == null)
            {
                basket = new Basket { AppUserId = userId };
                await _basketRepo.AddAsync(basket);
                await _basketRepo.SaveAsync();
            }

            var existingItem = basket.Items?.FirstOrDefault(i => i.ProductId == dto.ProductId);

           
            int quantityToAdd = dto.Quantity > 0 ? dto.Quantity : 1;

            int currentInBasket = existingItem?.Quantity ?? 0;
            int totalRequestedQuantity = currentInBasket + quantityToAdd;

            if (totalRequestedQuantity > product.StockQuanity)
            {
                throw new Exception($"Stok yetersiz! Bu üründen en fazla {product.StockQuanity} adet alabilirsiniz. Sepetinizde zaten {currentInBasket} adet var.");
            }

            product.StockQuanity -= quantityToAdd;
            _productRepo.Update(product);

            if (existingItem != null)
            {
                existingItem.Quantity += quantityToAdd;
                existingItem.UnitPrice = activePrice;
                _basketItemRepo.Update(existingItem);
            }
            else
            {
                var newItem = new BasketItem
                {
                    BasketId = basket.Id,
                    ProductId = dto.ProductId,
                    Quantity = quantityToAdd,
                    UnitPrice = activePrice,
                };
                await _basketItemRepo.AddAsync(newItem);
            }

            await _basketItemRepo.SaveAsync();
        }

        public async Task ClearBasketAsync(Guid userId)
        {
            var basket = await _basketRepo.Where(b => b.AppUserId == userId).
                                            Include(b => b.Items).
                                            FirstOrDefaultAsync();

            if (basket == null || basket.Items == null || !basket.Items.Any()) return;

            foreach (var item in basket.Items.ToList())
            {
                _basketItemRepo.Delete(item);

            }
            await _basketItemRepo.SaveAsync();
        }

        public async Task<BasketDto> GetBasketAsync(Guid userId)
        {
            var basket = await _basketRepo.Where(b => b.AppUserId == userId)
                                          .Include(b => b.Items).ThenInclude(i => i.Product)
                                          .ThenInclude(p=>p.ProductImages)
                                          .FirstOrDefaultAsync();

            if (basket == null) return new BasketDto();

            var finalItems = new List<BasketItemDto>();

            foreach (var item in basket.Items)
            {
                
                var acceptedOffer = await _offerRepo.Where(o => o.ProductId == item.ProductId
                                                        && o.BuyerId == userId
                                                        && o.Status == OfferStatus.Accepted)
                                               .OrderByDescending(o => o.CreatedDate)
                                               .FirstOrDefaultAsync();
                var productImages = item.Product?.ProductImages != null
                            ? item.Product.ProductImages.Select(pi => pi.ImagePath).ToList()
                            : new List<string>();
                decimal activePrice = item.Product?.Price ?? item.UnitPrice;

                if(item.Product?.DiscountedPrice.HasValue == true 
                    && item.Product.DiscountEndDate.HasValue == true
                    && item.Product.DiscountEndDate.Value > DateTime.UtcNow)
                {

                    activePrice = item.Product.DiscountedPrice.Value; 
                }

                if (acceptedOffer != null)
                {
                    
                    int offerQty = Math.Min(item.Quantity, acceptedOffer.Quantity);

                   
                    if (offerQty > 0)
                    {
                        finalItems.Add(new BasketItemDto
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Product?.Name + " (Teklifli)",
                            UnitPrice = acceptedOffer.OfferedPrice,
                            Quantity = offerQty,
                            IsOfferItem = true,
                            Images = productImages
                        });
                    }

                   
                    int remainingQty = item.Quantity - offerQty;
                    if (remainingQty > 0)
                    {
                        finalItems.Add(new BasketItemDto
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Product?.Name,
                            UnitPrice = activePrice, 
                            Quantity = remainingQty,
                            IsOfferItem = false,
                            Images = productImages
                        });
                    }
                }
                else
                {
                   
                    finalItems.Add(new BasketItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name,
                        UnitPrice = activePrice,
                        Quantity = item.Quantity,
                        IsOfferItem = false,
                        Images = productImages,
                    });
                }
            }

            
            decimal total = finalItems.Sum(x => x.Quantity * x.UnitPrice);
            return new BasketDto
            {
                BasketId = basket.Id,
                Items = finalItems,
                TotalBasketPrices = total
            };
        }




        public async Task RemoveItemFromBasketAsync(Guid userId, Guid productId)
        {
            var basket = await _basketRepo.Where(b=>b.AppUserId == userId).Include(b=>b.Items).FirstOrDefaultAsync();

            if (basket == null) return;

            var itemToRemove = basket.Items?.FirstOrDefault(i=> i.ProductId == productId);
            if (itemToRemove != null)
            {
                var product = await _productRepo.GetByIdAsync(productId);

                if(product != null)
                {
                    product.StockQuanity += itemToRemove.Quantity;
                    _productRepo.Update(product);
                    await _productRepo.SaveAsync();
                }
                _basketItemRepo.Delete(itemToRemove);
                await _basketItemRepo.SaveAsync();
            } 
        }
    }

}
