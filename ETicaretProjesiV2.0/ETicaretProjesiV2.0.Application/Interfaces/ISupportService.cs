using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface ISupportService
    {
        Task CreateTicketAsync(string userId, CreateTicketDto dto);
        Task<List<TicketListDto>> GetUserTicketsAsync(string userId);
        Task<List<TicketListDto>> GetPendingTicketsAsync();
        Task<List<TicketListDto>> GetActiveTicketsByAdminAsync(string adminId);

        Task AssignTicketToAdminAsync(Guid ticketId, string adminId);
        Task UpdateTicketStatusAsync(Guid ticketId, TicketStatus newStatus);
        Task<MessageDto> SaveMessageAsync(Guid ticketId,string senderId, string messageBody);
        Task<TicketChatViewModelDto> GetTicketMessagesAsync(Guid ticketId, string userId, bool isUserAdmin);
    }
}
