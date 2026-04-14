using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost("createOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı" });
            }

            var buyerId = Guid.Parse(userIdClaim);

            await _orderService.CreateOrderAsync(buyerId, dto);
            return Ok(new { message = "Siparişiniz Başarıyla oluşturuldu" });
        }
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaims = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaims))
                return Unauthorized(new { message = "Kullanıcı Kimliği Doğrulanamadı" });
            var userId = Guid.Parse(userIdClaims);
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(Guid id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id);
            if (order == null)
                return NotFound(new { message = "Sipariş bulunamadı" });

            return Ok(order);
        }
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            await _orderService.CancelOrderAsync(id);
            return Ok(new { message = "Sipariş iptal edildi" });
        }

        [HttpGet("GetSellersOrders/{sellerId}")]
        public async Task<IActionResult> GetSellerOrders(Guid sellerId)
        {
            try
            {
                var orders = await _orderService.GetSellerOrdersAsync(sellerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        [HttpPut("UpdateStatus/{orderId}")]
        public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] StatusUpdateRequestDto request)
        {
            try
            {
                await _orderService.UpdateOrderStatus(orderId, request.NewStatus);
                return Ok(new { message = "Sipariş durumu güncellendi" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("check-purchase/{productId}")]
        public async Task<IActionResult> CheckUserProduct(Guid productId)
        {
            var userIdClaims = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(string.IsNullOrEmpty(userIdClaims))
                return Unauthorized(new { message = "Kullanıcı Kimliği Doğrulanamadı" });
            var userId = Guid.Parse(userIdClaims);

            var hasPurchased = await _orderService.CheckIfUserPurchasedProductAsync(userId, productId);

            return Ok(hasPurchased);
        }
    }  
}
