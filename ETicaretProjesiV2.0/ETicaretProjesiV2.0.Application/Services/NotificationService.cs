using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Application.Interfaces.Services;
using ETicaretProjesiV2._0.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository=notificationRepository;
        }
        public async Task CreateNotificationAsync(UserNotificationDto dto)
        {
            var notification = new UserNotification
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Title = dto.Title,
                Type = dto.Type,
                RelatedId = dto.RelatedId,
                Message = dto.Message,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };
            
            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveAsync();
        }
    }
}
