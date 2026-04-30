using ETicaretProjesiV2._0.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace ETicaretProjesiV2._0.Infrastructure.Hubs
{
    public class TrafficHub : Hub
    {
        private readonly VisitorStorage _storage;

        public TrafficHub(VisitorStorage storage)
        {
            _storage = storage;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _storage.AddOrUpdateUser(userId);
                await Clients.Others.SendAsync("UpdateOnlineStatus", _storage.GetOnlineUserIds());
                await Clients.Caller.SendAsync("UpdateOnlineStatus", _storage.GetOnlineUserIds());
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {

                _storage.RemoveUser(userId);
                await Clients.All.SendAsync("UpdateOnlineStatus", _storage.GetOnlineUserIds());
                await Clients.Caller.SendAsync("UpdateOnlineStatus",_storage.GetOnlineUserIds());
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
