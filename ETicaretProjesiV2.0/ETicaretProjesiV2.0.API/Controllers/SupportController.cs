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
    public class SupportController :ControllerBase
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
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Kullanıcı Kimliği Doğrulanamadı");

            await _supportService.CreateTicketAsync(userId, dto);

            return Ok(new { message = "Destek talebiniz başarıyla oluşturuldu." });

        }

        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Kullanıcı kimliği bulunamadı.");

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

            try
            {
                await _supportService.AssignTicketToAdminAsync(ticketId, adminId);
                return Ok(new { message = "Talep başarıyla üzerinize alındı ve aktif edildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{ticketId}/status")]
        public async Task<IActionResult> UpdateTicketStatus(Guid ticketId, [FromQuery] TicketStatus status)
        {
            try
            {
                await _supportService.UpdateTicketStatusAsync(ticketId, status);
                return Ok(new { message = "Talep durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{ticketId}/messages")]
        public async Task<IActionResult> GetTicketMessages(Guid ticketId)
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Kullanıcı kimliği bulunamadı.");

           
            bool isAdmin = User.IsInRole("Admin");

            try
            {
                
                var chatData = await _supportService.GetTicketMessagesAsync(ticketId, userId, isAdmin);

                return Ok(chatData);
            }
            catch (UnauthorizedAccessException ex)
            {
               
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
               
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
