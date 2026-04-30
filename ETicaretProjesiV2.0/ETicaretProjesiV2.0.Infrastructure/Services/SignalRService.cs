using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Infrastructure.Services
{
    public class SignalRService:ISignalService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public SignalRService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string methodName, string message)
        {
            await _hubContext.Clients.All.SendAsync(methodName, message);
        }

        public async Task SendPriceAlertNotificationAsync(string userId, string message, string productId)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceivePriceAlert", new {
            Message = message,
            ProductId = productId
            });
        }

        public async Task SendReportNotificationAsync(string userId, string message, string downloadUrl)
        {

            Console.WriteLine($"[SignalR] Sinyal gönderilen ID: '{userId}'");

            await _hubContext.Clients.User(userId).SendAsync("ReceiveReportNotification", new { message, downloadUrl });
        }

    }
}
