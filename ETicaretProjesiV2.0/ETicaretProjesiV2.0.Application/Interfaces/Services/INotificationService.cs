using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces.Services
{
    public interface INotificationService 
    {
        Task CreateNotificationAsync(UserNotificationDto dto);
        Task SendTrendUpdateAsync(string message);
        Task SendReportNotificationAsync(string userId, string message, string downloadUrl);
        Task SendPriceAlertNotificationAsync(string userId, string message, string productId);

    }

}
