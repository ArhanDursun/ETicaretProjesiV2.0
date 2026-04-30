using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
     public interface ISignalService
    {
        Task SendNotificationAsync(string methodName, string message);
        Task SendReportNotificationAsync(string userId, string message, string downloadUrl);
        Task SendPriceAlertNotificationAsync(string userId, string message, string productId);
    }
}
