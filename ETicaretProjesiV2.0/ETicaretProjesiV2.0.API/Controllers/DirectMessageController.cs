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
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(myId))
                return Unauthorized(new { Message = "Kullanıcı kimliği bulunamadı." });

            var chats = await _messageService.GetRecentChatsAsync(myId);
            return Ok(chats);
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

            var savedMessage = await _messageService.SendMessageAsync(myId, request.ReceiverId, request.Content, request.MessageType ?? "text");

            var cleanMessage = new
            {
                Id = savedMessage.Id,
                SenderId = savedMessage.SenderId,
                ReceiverId = savedMessage.ReceiverId,
                Content = savedMessage.Content,
                MessageType = savedMessage.MessageType,
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
            if (string.IsNullOrEmpty(myId)) return Unauthorized();

            await _messageService.MarkAsReadAsync(myId, otherUserId);
            return Ok();
        }

        [HttpPost("upload-chat-files")]
        public async Task<IActionResult> UploadChatFiles(List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest(new { Message = "Dosya seçilmedi." });

            var uploadedFiles = new List<object>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "chat-uploads");

            if (!Directory.Exists(uploadDirectory)) Directory.CreateDirectory(uploadDirectory);

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension)) continue;

                string type = extension == ".pdf" ? "pdf" : "image";
                var fileName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine(uploadDirectory, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"{Request.Scheme}://{Request.Host}/chat-uploads/{fileName}";
                uploadedFiles.Add(new { url = fileUrl, type = type });
            }

            return Ok(new { files = uploadedFiles });
        }
    }
}