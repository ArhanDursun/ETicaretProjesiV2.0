using ETicaretProjesiV2._0.Infrastructure.Hubs;
using ETicaretProjesiV2._0.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace ETicaretProjesiV2._0.Infrastructure.Middlewares
{
    public class VisitorTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly VisitorStorage _storage;
        private readonly IHubContext<TrafficHub> _hubContext;

        public VisitorTrackingMiddleware(RequestDelegate next, VisitorStorage storage,IHubContext<TrafficHub> hubContext)
        {
            _next = next;
            _storage = storage;
            _hubContext = hubContext;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _storage.AddOrUpdateUser(userId);

                await _hubContext.Clients.All.SendAsync("UpdateOnlineStatus",_storage.GetOnlineUserIds());
                
            }

            await _next(context);
        }

    }
}
