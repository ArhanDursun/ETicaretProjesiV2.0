using ETicaretProjesiV2._0.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoriteController :ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpPost("toggle/{productId}")]
        public async Task<IActionResult> ToggleFavorite(string productId)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(myId)) return Unauthorized();

            
            var result = await _favoriteService.ToggleFavoriteAsync(myId, productId);

            return Ok(result);
        }

        [HttpGet("check/{productId}")]
        public async Task<IActionResult> CheckFavorite(string productId)
        {
            
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(myId)) return Ok(false);

            var isFav = await _favoriteService.CheckFavoriteAsync(myId, productId);
            return Ok(isFav);
        }
    }
}
