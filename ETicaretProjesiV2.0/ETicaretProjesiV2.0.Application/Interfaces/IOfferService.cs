using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IOfferService
    {
        Task MakeOfferAsync(Guid BuyerId, CreateOfferRequestDto dto);
        Task<IEnumerable<OfferResponseDto>> GetOffersMadeByMeAsync(Guid buyerId);
        Task<bool> MakeCounterOfferAsync(Guid offerId,decimal counterPrice,string currentUserId);
        Task<IEnumerable<OfferResponseDto>> GetOfferRecievedByMeAsync(Guid sellerId);
        Task<bool> RespondToOfferAsync(Guid offerId,bool isAccepted,string currentUserId);
    }
}
