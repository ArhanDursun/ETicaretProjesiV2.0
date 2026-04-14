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
    public class WalletController :ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı" });
            }

            var userId = Guid.Parse(userIdClaim);
            var balance = await _walletService.GetBalanceAsync(userId);

            return Ok(new {currentBalance = balance });
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions() {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) 
                return Unauthorized("Kullanıcı kimliği doğrulanamadı");

            var userId = Guid.Parse(userIdClaim);
            var transactions = await _walletService.GetTransactionsHistoryAsync(userId);
            return Ok(transactions);
        }
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı" });
            }
            var userId = Guid.Parse(userIdClaim);
            if (dto.Amount <= 0)
                return BadRequest(new { message = "Yüklenecek tutar 0 dan büyük olmalı" });
            await _walletService.DepositAsync(userId, dto.Amount, dto.Description);
            return Ok(new {message = $"{dto.Amount} TL cüzdanınıza başarıyla yüklenmiştir"});
        }
        [HttpPost("add-balance")]
        public async Task<IActionResult> AddBalance([FromBody] AddBalanceRequestDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "Kullanıcı Kimliği Doğrulanamadı" });
                
                var result = await _walletService.AddBalanceAsync(userId, dto);

                if (result)
                {
                    return Ok(new { message = "İşlem Başarılı Cüzdanınızı kontrol edebilirsiniz" });
                }

                return BadRequest(new { message = "Bakiye Yükleme işlemi başarısız oldu" });

            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
