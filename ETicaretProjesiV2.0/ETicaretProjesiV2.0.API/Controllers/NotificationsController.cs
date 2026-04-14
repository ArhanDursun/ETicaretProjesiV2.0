using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController: ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Unauthorized("Kullanıcı Kimliği Doğrulanamadı");
                }

                var userId = Guid.Parse(userIdStr);

                var unreadList = await _notificationRepository.Where(n => n.UserId == userId && n.IsRead == false).OrderByDescending(n => n.CreatedDate)
                                                                .Select(n => new UserNotificationDto
                                                                {
                                                                    Id = n.Id,
                                                                    UserId = userId,
                                                                    Title = n.Title,
                                                                    Message = n.Message,
                                                                    CreatedDate = n.CreatedDate,
                                                                }).ToListAsync();
                return Ok(unreadList);
            }
            catch (Exception ex)
            {

                return BadRequest("Bildirimler alınırken hata oluştu: " + ex.Message);
            }
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(Guid Id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(Id);

                if (notification == null)
                    return NotFound("Bildirim bulunamadı.");
                var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (notification.UserId != currentUserId)
                    return Forbid("Bu işlem için yetkiniz yok.");
                notification.IsRead = true;
                _notificationRepository.Update(notification);
                await _notificationRepository.SaveAsync();
                return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
            }
            catch (Exception ex)
            {
                return BadRequest("İşlem başarısız: " + ex.Message);
            }
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(myId)) return Unauthorized("Kullanıcı Kimliği Doğrulanamadı");

            var userId = Guid.Parse(myId);

            var notifications = await _notificationRepository.Where(n => n.UserId == userId)
                                                             .OrderByDescending(n => n.CreatedDate).ToListAsync();

            return Ok(notifications);

            
        }

        [HttpPost("test-yolla")]
        public async Task<IActionResult> SendTestNotification()
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(myId)) return Unauthorized();

            
            var testNotif = new UserNotification
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(myId),
                Title = "Sistem Testi",
                Message = "Aga sistem jilet gibi çalışıyor! 🚀",
                Type = "OfferAccepted",
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(testNotif);
            await _notificationRepository.SaveAsync();
            
            return Ok("Test bildirimi ateşlendi!");
        }
    }
}
