using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
namespace ETicaretProjesiV2._0.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DirectMessageController : ControllerBase
    {
        private readonly IDirectMessageService _messageService;
        private readonly IHubContext<ChatHub> _chatHub;


       public DirectMessageController(IDirectMessageService messageService, IHubContext<ChatHub> chatHub)
        {
            _messageService = messageService;
            _chatHub = chatHub;
        }

        [HttpGet("recent-chats")]
        public async Task<IActionResult> GetRecentChats()
        {
            try
            {
                var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(myId))
                    return BadRequest("Kullanıcı kimliği (Token) bulunamadı.");

                var chats = await _messageService.GetRecentChatsAsync(myId);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = "Sistem Patladı: " + errorMsg });
            }
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(string otherUserId)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var history = await _messageService.GetChatHistoryAsync(myId, otherUserId);
            return Ok(history);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendDirectMessage request)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

           
            var savedMessage = await _messageService.SendMessageAsync(myId, request.ReceiverId, request.Content);

            
            var cleanMessage = new
            {
                Id = savedMessage.Id, 
                SenderId = savedMessage.SenderId,
                ReceiverId = savedMessage.ReceiverId,
                Content = savedMessage.Content,
                SentDate = savedMessage.SentDate,
                IsRead = savedMessage.IsRead
            };

            
            await _chatHub.Clients.User(request.ReceiverId).SendAsync("ReceiveDirectMessage", cleanMessage);
            await _chatHub.Clients.User(myId).SendAsync("ReceiveDirectMessage", cleanMessage);

            return Ok(savedMessage);
        }
        [HttpGet("available-users")]
        public async Task<IActionResult> GetAvailableUsers()
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var users = await _messageService.GetAvailableUsersAsync(myId);
            return Ok(users);
        }
        [HttpPost("mark-read/{otherUserId}")]
        public async Task<IActionResult> MarkAsRead(string otherUserId)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(myId))
                return Unauthorized();

            // Servisteki okundu yapma metodunu tetikliyoruz
            await _messageService.MarkAsReadAsync(myId, otherUserId);

            return Ok();
        }
    }
}
