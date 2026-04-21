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
    public class OfferController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OfferController(IOfferService offerService)
        {
            _offerService = offerService;
        }
        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");
            return Guid.Parse(userId);
        }
        [HttpPost("make-offer")]
        public async Task<IActionResult> MakeOffer([FromBody] CreateOfferRequestDto dto)
        {
            var buyerId = GetUserId(); 
            await _offerService.MakeOfferAsync(buyerId, dto);
            return Ok(new { Message = "Teklifiniz satıcıya başarıyla iletildi! 🚀" });
        }
        [HttpGet("my-offers")]
        public async Task<IActionResult> GetMyOffers()
        {
            var buyerId = GetUserId();
            var offers = await _offerService.GetOffersMadeByMeAsync(buyerId);
            return Ok(offers);
        }
        [HttpGet("received-offers")]
        public async Task<IActionResult> GetReceivedOffers()
        {
            var sellerId = GetUserId(); 
            var offers = await _offerService.GetOfferRecievedByMeAsync(sellerId);
            return Ok(offers);
        }
        [HttpPut("{offerId}/respond")]
        public async Task<IActionResult> RespondToOffer(Guid offerId, [FromQuery] bool isAccepted)
        {
            var currentUserId = GetUserId();

            await _offerService.RespondToOfferAsync(offerId, isAccepted, currentUserId.ToString());
            var statusMessage = isAccepted ? "Teklif onaylandı! ✅" : "Teklif reddedildi. ❌";
            return Ok(new { Message = statusMessage });
        }
        [HttpPost("counter-offer")]
        [Authorize]
        public async Task<IActionResult> CounterOffer([FromBody] CounterOfferDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

           
                await _offerService.MakeCounterOfferAsync(request.OfferId, request.CounterPrice, userId);
                return Ok();
           
        }
    }   
}
