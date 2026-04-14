using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
    public class CustomUserIdProvider:IUserIdProvider
    {
        public virtual string? GetUserId(HubConnectionContext connection)
        {
           
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("nameid")?.Value;
        }
    }
}
