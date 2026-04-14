using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IBasketService
    {
        Task<BasketDto> GetBasketAsync(Guid userId);
        Task AddItemToBasketAsync(Guid userId, AddItemToBasketDto dto);
        Task RemoveItemFromBasketAsync(Guid userId, Guid productId);
        Task ClearBasketAsync(Guid userId);

    }
}
