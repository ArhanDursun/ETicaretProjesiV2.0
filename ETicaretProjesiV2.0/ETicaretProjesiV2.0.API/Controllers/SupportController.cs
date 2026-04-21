using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupportController : ControllerBase
    {
        private readonly ISupportService _supportService;

        public SupportController(ISupportService supportService)
        {
            _supportService = supportService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Message = "Kullanıcı Kimliği Doğrulanamadı" });

            await _supportService.CreateTicketAsync(userId, dto);
            return Ok(new { Message = "Destek talebiniz başarıyla oluşturuldu." });
        }

        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Message = "Kullanıcı kimliği bulunamadı." });

            var tickets = await _supportService.GetUserTicketsAsync(userId);
            return Ok(tickets);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingTickets()
        {
            var tickets = await _supportService.GetPendingTicketsAsync();
            return Ok(tickets);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("my-active")]
        public async Task<IActionResult> GetMyActiveTickets()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId)) return Unauthorized();

            var tickets = await _supportService.GetActiveTicketsByAdminAsync(adminId);
            return Ok(tickets);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{ticketId}/assign")]
        public async Task<IActionResult> AssignTicket(Guid ticketId)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId)) return Unauthorized();

            await _supportService.AssignTicketToAdminAsync(ticketId, adminId);
            return Ok(new { Message = "Talep başarıyla üzerinize alındı ve aktif edildi." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{ticketId}/status")]
        public async Task<IActionResult> UpdateTicketStatus(Guid ticketId, [FromQuery] TicketStatus status)
        {
            await _supportService.UpdateTicketStatusAsync(ticketId, status);
            return Ok(new { Message = "Talep durumu güncellendi." });
        }

        [HttpGet("{ticketId}/messages")]
        public async Task<IActionResult> GetTicketMessages(Guid ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Message = "Kullanıcı kimliği bulunamadı." });

            bool isAdmin = User.IsInRole("Admin");
            var chatData = await _supportService.GetTicketMessagesAsync(ticketId, userId, isAdmin);

            return Ok(chatData);
        }

        [HttpPost("upload-support-file")]
        public async Task<IActionResult> UploadSupportFile(List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest(new { Message = "Dosya seçilmedi veya dosya boş." });

            var uploadedUrls = new List<string>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "support-uploads");

            if (!Directory.Exists(uploadDirectory)) Directory.CreateDirectory(uploadDirectory);

            foreach (var file in files)
            {
                if (file.Length <= 0) continue;

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension)) continue;

                var fileName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine(uploadDirectory, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"{Request.Scheme}://{Request.Host}/support-uploads/{fileName}";
                uploadedUrls.Add(fileUrl);
            }

            return Ok(new { Urls = uploadedUrls });
        }
    }
}