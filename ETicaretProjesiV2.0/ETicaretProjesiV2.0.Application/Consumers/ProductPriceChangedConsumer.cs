using DocumentFormat.OpenXml.Spreadsheet;
using ETicaretProjesiV2._0.Application.Events;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Entities;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Consumers
{
    public class ProductPriceChangedConsumer : IConsumer<ProductPriceChangedEvent>
    {
        private readonly IFavoriteService _favoriteService;
        private readonly ISignalService _signalService;
        private readonly INotificationRepository _notificationRepository;

        public ProductPriceChangedConsumer(IFavoriteService favoriteService, ISignalService signalService,INotificationRepository notificationRepository)
        {
            _favoriteService = favoriteService;
            _signalService = signalService;
            _notificationRepository = notificationRepository;
        }

        public async Task Consume(ConsumeContext<ProductPriceChangedEvent> context)
        {
            var productId = context.Message.ProductId;
            var productName = context.Message.ProductName;

            var userIds = await _favoriteService.GetUserFavoritedProductAsync(productId);
            foreach (var userIdStr in userIds)
            {
                var userId = Guid.Parse(userIdStr);
                var message = $"Favorindeki {productName} ürününün fiyatı {context.Message.NewPrice} TL'ye düştü!";
                await _signalService.SendPriceAlertNotificationAsync(userId.ToString(), message, productId.ToString());
                var notification = new UserNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "Fiyat Alarmı!",
                    Message = message,
                    Type = "StockAlert", 
                    RelatedId = productId.ToString(), 
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(notification);
            }

            if (userIds.Any())
            {
                await _notificationRepository.SaveAsync();
            }
        }

    }
}
