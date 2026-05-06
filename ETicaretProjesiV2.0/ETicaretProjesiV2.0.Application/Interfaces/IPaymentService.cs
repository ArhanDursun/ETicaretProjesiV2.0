using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResultDto> ProcessPaymentAsync(PaymentRequestDto request, Guid userId);
    }
}
