using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IGenericRepository<UserFavorite> _favoriteRepo;
        private readonly IGenericRepository<UserNotification> _notificationRepo;
        private readonly IGenericRepository<Product> _productRepo;
        public FavoriteService(IGenericRepository<UserFavorite> favoriteRepo, IGenericRepository<UserNotification> notificationRepo,IGenericRepository<Product> productRepo)
        {
            _favoriteRepo = favoriteRepo;
            _notificationRepo = notificationRepo;
            _productRepo = productRepo;
        }

        public async Task<bool> CheckFavoriteAsync(string userId, string productId)
        {
            var uId = Guid.Parse(userId);
            var pId = Guid.Parse(productId);

            return await _favoriteRepo.Where(f => f.UserId == uId && f.ProductId == pId).AnyAsync();
        }

        public async Task<List<string>> GetUserFavoritedProductAsync(Guid productId)
        {
           var query = _favoriteRepo.Where(f=>f.ProductId == productId);
            return query.Select(f => f.UserId.ToString()).ToList();

        }

        public async Task<FavoriteToggleResponseDto> ToggleFavoriteAsync(string userId, string productId)
        {
            var uId = Guid.Parse(userId);
            var cleanProductId = productId.ToLower().Trim();
            var pId = Guid.Parse(cleanProductId);
            var product = await _productRepo.GetByIdAsync(pId);

            var existingFavorite = await _favoriteRepo
                                    .Where(f => f.UserId == uId && f.ProductId == pId)
                                    .FirstOrDefaultAsync();

            if(existingFavorite != null)
            {
                _favoriteRepo.Delete(existingFavorite);

                var relatedNotification = await _notificationRepo
                    .Where(n => n.UserId == uId && n.RelatedId.ToLower() == cleanProductId && n.Type == "FavoriteAdded")
                    .ToListAsync();

                if (relatedNotification.Any())
                {
                    _notificationRepo.RemoveRange(relatedNotification);
                }

                await _favoriteRepo.SaveAsync();
                await _notificationRepo.SaveAsync();

                return new FavoriteToggleResponseDto { IsFavorited = false, Message = "Favorilerden ve bildirimlerden kaldırıldı." };
            }else
            {
                var hasNotification = await _notificationRepo.Where(n=>n.UserId == uId && n.RelatedId.ToLower() ==cleanProductId && n.Type == "FavoriteAdded").AnyAsync();

                var newFav = new UserFavorite
                {
                    UserId = uId,
                    ProductId = pId,
                };
                await _favoriteRepo.AddAsync(newFav);

                if (!hasNotification)
                {
                    var notif = new UserNotification
                    {
                        Id = Guid.NewGuid(),
                        UserId = uId,
                        Title = product.Name,
                        Message = $"{product.Name} ürününü favorilere eklediniz.",
                        Type = "FavoriteAdded",
                        RelatedId = cleanProductId,
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow,
                    };
                    await _notificationRepo.AddAsync(notif);
                }
                await _favoriteRepo.SaveAsync();
                await _notificationRepo.SaveAsync();

                return new FavoriteToggleResponseDto { IsFavorited = true, Message = "Favorilere eklendi." };
            }





        }
    }
}
