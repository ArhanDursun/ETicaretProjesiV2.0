using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretProjesiV2._0.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminManagementService _adminService;
        private readonly IAdminDashboardService _adminDashboardService;
        public AdminController(IAdminManagementService adminService, IAdminDashboardService adminDashboardService)
        {
            _adminService = adminService;
            _adminDashboardService = adminDashboardService;
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
            try
            {
                await _adminService.DeleteProductWithReasonAsync(dto);
                return Ok(new { Message = "Ürün Başarıyla Silindi" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUsersDetails(Guid id)
        {
            try
            {
                var user = await _adminService.GetUserDetailsAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
        [HttpGet("users/{id}/products")]
        public async Task<IActionResult> GetUserProducts(Guid id)
        {
            var products = await _adminService.GetUserProductsAsync(id);
            return Ok(products);
        }
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminDashboardService.GetStatsAsync();
            return Ok(stats);
        }

        [HttpGet("dashboard/recent-orders")]
        public async Task<IActionResult> GetRecentOrders()
        {
            var recentOrders = await _adminDashboardService.GetRecentOrdersAsync();
            return Ok(recentOrders);
        }
        [HttpGet("dashboard/daily-commission")]
        public async Task<IActionResult> GetDailyComissions([FromQuery] string timeRange)
        {
            var data = await _adminDashboardService.GetDailyComissionsAsync(timeRange);
            return Ok(data);
        }
    }
}
