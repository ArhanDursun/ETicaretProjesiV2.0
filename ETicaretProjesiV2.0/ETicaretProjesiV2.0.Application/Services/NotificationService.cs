using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Application.Interfaces.Services;
using ETicaretProjesiV2._0.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ISignalService _signalService;
       
        


        public NotificationService(INotificationRepository notificationRepository,ISignalService signalService)
        {
            _notificationRepository=notificationRepository;
            _signalService=signalService;
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

        public async Task SendPriceAlertNotificationAsync(string userId, string message, string productId)
        {
           await _signalService.SendPriceAlertNotificationAsync(userId, message, productId);
        }

        public async Task SendReportNotificationAsync(string userId, string message, string downloadUrl)
        {
             await _signalService.SendReportNotificationAsync(userId,message,downloadUrl);
        }

       
        public async Task SendTrendUpdateAsync(string message)
        {
            await _signalService.SendNotificationAsync("ReceiveTrendUpdate", message);
        }

        

    }
}
