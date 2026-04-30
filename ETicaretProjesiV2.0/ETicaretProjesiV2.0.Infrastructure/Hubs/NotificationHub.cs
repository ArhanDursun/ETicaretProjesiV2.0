using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
