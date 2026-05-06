
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
    public class PaymentsController:ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IWalletService _walletService;
        private readonly IOrderService _orderService;

        public PaymentsController(IPaymentService paymentService, IWalletService walletService,IOrderService orderService)
        {
            _paymentService = paymentService;
            _walletService = walletService;
            _orderService = orderService;
        }

        [HttpPost("direct-checkout")]
        public async Task<IActionResult> DirectCheckout([FromBody] PaymentRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { Message = "Kullanıcı kimliği doğrulanamadı" });
            }

            request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "85.34.78.112";
            request.PaymentType = PaymentType.DirectCheckOut;

            var result = await _paymentService.ProcessPaymentAsync(request, userId);

            if (result.IsSuccess)
            {
                var orderDto = new CreateOrderRequestDto();
                await _orderService.CreateOrderAsync(userId, orderDto, isCreditCardPayment: true);

                return Ok(new { Message = "Ödeme Başarılı!", TransactionId = result.BankTransactionId });
            }
            return BadRequest(new { Message = result.ErrorMessage });
        }
        [HttpPost("top-up")]
        public async Task<IActionResult> TopUpWallet([FromBody] PaymentRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!Guid.TryParse(userIdString,out Guid userId))
            {
                return Unauthorized(new { Message = "Kullanıcı kimliği doğrulanamadı" });
            }

            request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "85.34.78.112";
            request.PaymentType = PaymentType.WalletTopUp;

            var result = await _paymentService.ProcessPaymentAsync(request, userId);
            if (result.IsSuccess)
            {
                string islemAciklamasi = $"Iyzico üzerinden {result.BankTransactionId} fiş numarası ile bakiye yüklendi.";
                await _walletService.DepositAsync(userId, request.Price, islemAciklamasi);

                return Ok(new { Message = "Bakiye Başarıyla Yüklendi!", TransactionId = result.BankTransactionId });
            }
            return BadRequest(new { Message = result.ErrorMessage });
        }

    }
}
