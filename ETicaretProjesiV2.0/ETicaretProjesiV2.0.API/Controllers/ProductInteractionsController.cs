using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductInteractionsController :ControllerBase
    {
        private readonly IProductInteractionService _interactionService;

        public ProductInteractionsController(IProductInteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        [HttpPost("comment")]
        [Authorize]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized("Kullanıcı Kimliği Doğrulanamadı");
            }
            var userId = Guid.Parse(userIdStr);

            await _interactionService.AddCommentAsync(userId, dto);
            return Ok(new { Message = "Yorumunuz Başarıyla eklendi" });
        }
        [HttpGet("comment/{productId}")]
        public async Task<IActionResult> GetCommentById(Guid productId)
        {
            var comments = await _interactionService.GetCommentsByProductIdAsync(productId);

            return Ok(comments);
        }

        [HttpPost("question")]
        [Authorize]
        public async Task<IActionResult> AddQuestion([FromBody] CreateQuestionDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Kullanıcı Kimliği Doğrulanamadı");

            var userId = Guid.Parse(userIdStr);

            await _interactionService.AddQuestionAsync(userId, dto);
            return Ok(new { Message = "Sorunuz Başarıyla eklendi" });
        }
        [HttpGet("question/{productId}")]
        public async Task<IActionResult> GetQuestionById(Guid productId)
        {
            var questions = await _interactionService.GetQuestionsByProductIdAsync(productId);
            return Ok(questions);
        }

        [HttpPut("question/answer")]
        public async Task<IActionResult> AnswerQuestion([FromBody] AnswerQuestionDto dto)
        {
            var sellerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(sellerIdStr))
                return Unauthorized("Kullanıcı Kimliği Doğrulanamadı");

            var sellerId = Guid.Parse(sellerIdStr);

            try
            {
                await _interactionService.AnswerQuestionAsync(sellerId, dto);
                return Ok(new { Message = "Sorunuz Başarıyla cevaplandı" });
            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }

    }
}
