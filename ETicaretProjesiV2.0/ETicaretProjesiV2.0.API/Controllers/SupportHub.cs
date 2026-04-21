using ETicaretProjesiV2._0.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
    [Authorize]
    public class SupportHub : Hub
    {
        private readonly ISupportService _supportService;

        public SupportHub(ISupportService supportService)
        {
            _supportService = supportService;
        }

        public async Task JoinTicketGroup(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticketId.ToLower());
        }

        public async Task LeaveTicketGroup(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId.ToLower());
        }

        public async Task SendMessage(string ticketId, string messageBody, string messageType = "text")
        {
            var senderId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (senderId == null) return;

            var messageDto = await _supportService.SaveMessageAsync(Guid.Parse(ticketId), senderId, messageBody, messageType);
            await Clients.Group(ticketId.ToLower()).SendAsync("RecieveMessage", messageDto);
        }
    }
}