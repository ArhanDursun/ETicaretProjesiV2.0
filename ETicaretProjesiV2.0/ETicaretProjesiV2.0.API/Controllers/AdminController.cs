using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Events;
using ETicaretProjesiV2._0.Application.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminManagementService _adminService;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IPublishEndpoint _publishEndpoint;
        public AdminController(IAdminManagementService adminService, IAdminDashboardService adminDashboardService,IPublishEndpoint publishEndpoint)
        {
            _adminService = adminService;
            _adminDashboardService = adminDashboardService;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _adminService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpPost("products/delete-with-reason")]
        public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductDto dto)
        {
            await _adminService.DeleteProductWithReasonAsync(dto);
            return Ok(new { Message = "Ürün Başarıyla Silindi" });
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUsersDetails(Guid id)
        {
            var user = await _adminService.GetUserDetailsAsync(id);
            return Ok(user);
        }

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminDashboardService.GetStatsAsync();
            return Ok(stats);
        }

        [HttpGet("dashboard/daily-commission")]
        public async Task<IActionResult> GetDailyComissions([FromQuery] string timeRange)
        {
            var data = await _adminDashboardService.GetDailyComissionsAsync(timeRange);
            return Ok(data);
        }
        [HttpGet("dashboard/recent-orders")]
        public async Task<IActionResult> GetRecentOrders()
        {
            var data = await _adminDashboardService.GetRecentOrdersAsync();
            return Ok(data);
        }
        [HttpPost("generate-sales-report")]
        public async Task<IActionResult> GenerateSalesReport()
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized("Kullanıcı Kimliği Doğrulanamadı");

            await _publishEndpoint.Publish(new GenerateReportEvent(
                AdminUserId: adminId,
                StartDate: DateTime.UtcNow.AddYears(-1),
                EndDate: DateTime.UtcNow
                ));

            return Accepted(new
            {
                Message = "Rapor Talebi başarıyla alındı"
            });
        }
    }
}