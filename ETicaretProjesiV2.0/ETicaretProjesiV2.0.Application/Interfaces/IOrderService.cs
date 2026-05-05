using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IOrderService
    {
        Task CreateOrderAsync(Guid buyerId, CreateOrderRequestDto dto, bool isCreditCardPayment = false);
        Task<Order> GetOrderDetailsAsync(Guid orderId);
        Task<IEnumerable<OrderListResponseDto>> GetUserOrdersAsync(Guid userId);
        Task CancelOrderAsync(Guid orderId);
        Task<IEnumerable<OrderListResponseDto>> GetSellerOrdersAsync(Guid sellerId);
        Task<bool> UpdateOrderStatus(Guid orderId,int newStatus);
        Task<bool> CheckIfUserPurchasedProductAsync(Guid userId, Guid productId);
    }
}
