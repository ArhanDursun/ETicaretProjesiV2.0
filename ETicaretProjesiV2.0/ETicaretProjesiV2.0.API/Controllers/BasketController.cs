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
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
               
                throw new Exception("Kullanıcı Kimliği Doğrulanamadı");
            }
            return Guid.Parse(userIdString);
        }

        [HttpGet]
        public async Task<IActionResult> GetBasket()
        {
            var userId = GetUserId();
            var basket = await _basketService.GetBasketAsync(userId);
            return Ok(basket);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddItemToBasket([FromBody] AddItemToBasketDto dto)
        {
            var userId = GetUserId();
            await _basketService.AddItemToBasketAsync(userId, dto);
            return Ok(new { Message = "Ürün sepete eklendi", Data = dto });
        }

        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveItemFromBasket(Guid productId)
        {
            var userId = GetUserId();
            await _basketService.RemoveItemFromBasketAsync(userId, productId);
            return Ok(new { Message = "Ürün sepetten kaldırıldı" });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearBasket()
        {
            var userId = GetUserId();
            await _basketService.ClearBasketAsync(userId);
            return Ok(new { Message = "Sepet başarıyla temizlendi" });
        }
    }
}