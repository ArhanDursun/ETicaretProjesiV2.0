using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<FavoriteToggleResponseDto> ToggleFavoriteAsync(string userId, string productId);
        Task<bool> CheckFavoriteAsync(string userId, string productId);

        Task<List<string>> GetUserFavoritedProductAsync(Guid productId);
    }
}
