using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class SupportServices :ISupportService
    {
        private readonly IGenericRepository<SupportTicket> _ticketRepo;
        private readonly IGenericRepository<TicketMessage> _messageRepo;
        private readonly UserManager<AppUser> _userManager;

        public SupportServices(IGenericRepository<SupportTicket> ticketRepo, IGenericRepository<TicketMessage> messageRepo, UserManager<AppUser> userManager)
        {
            _ticketRepo = ticketRepo;
            _messageRepo = messageRepo;
            _userManager = userManager;
        }

        public async Task CreateTicketAsync(string userId, CreateTicketDto dto)
        {
            var ticket = new SupportTicket
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userId),
                Category = dto.Category,
                Subject = dto.Subject,
                InitialMessage = dto.InitialMessage,
                Status = TicketStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };

            await _ticketRepo.AddAsync(ticket);
            await _ticketRepo.SaveAsync();
        }

        public async Task<List<TicketListDto>> GetUserTicketsAsync(string userId)
        {
            var tickets = await _ticketRepo.Where(t => t.UserId.ToString() == userId)
            .OrderByDescending(t => t.CreatedDate)
            .Select(t => new TicketListDto {
                Id = t.Id,
                Subject = t.Subject,
                CategoryName = t.Category.ToString(),
                StatusName = t.Status.ToString(),
                CreatedDate = t.CreatedDate

            }).ToListAsync();
            return tickets;
        }

        public async Task<List<TicketListDto>> GetPendingTicketsAsync()
        {
            return await _ticketRepo.Where(t=> t.Status == TicketStatus.Pending)
                .Include(t=>t.User)
                .OrderBy(t=> t.CreatedDate)
                .Select(t=> new TicketListDto
                {
                    Id = t.Id,
                    Subject = t.Subject,
                    UserName = t.User.FirstName + " " + t.User.LastName,
                    CategoryName = t.Category.ToString(),
                    StatusName = t.Status.ToString(),
                    CreatedDate = t.CreatedDate
                }).ToListAsync();
        }
        public async Task AssignTicketToAdminAsync(Guid ticketId,string adminId)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) throw new Exception("Talep Bulunamadı");
            if(ticket.Status != TicketStatus.Pending) throw new Exception("Bu talep zaten işleme alınmış");


            ticket.AssignedAdminId = Guid.Parse(adminId);
            ticket.Status = TicketStatus.Active;

            _ticketRepo.Update(ticket);
            await _ticketRepo.SaveAsync();
        }
        public async Task UpdateTicketStatusAsync(Guid ticketId,TicketStatus newStatus)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) throw new Exception("Talep Bulunamadı");

            ticket.Status = newStatus;
            if(newStatus == TicketStatus.Closed)
            {
                ticket.ClosedDate = DateTime.UtcNow;
            }
            _ticketRepo.Update(ticket);
            await _ticketRepo.SaveAsync();
        }

        public async Task<List<TicketListDto>> GetActiveTicketsByAdminAsync(string adminId)
        {
            return await _ticketRepo.Where(t => t.AssignedAdminId.ToString() == adminId)
                .Select(t => new TicketListDto
                {
                    Id = t.Id,
                    Subject = t.Subject,
                    StatusName = t.Status.ToString(),
                    CreatedDate = t.CreatedDate
                }).ToListAsync();

        }

        public async Task<MessageDto> SaveMessageAsync(Guid ticketId, string senderId, string messageBody)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null || ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Rejected)
                throw new Exception("Bu talep mesajlaşmaya kapalı.");

            var sender = await _userManager.FindByIdAsync(senderId);

            var message = new TicketMessage
            {
                Id = Guid.NewGuid(),
                SupportTicketId = ticketId,
                SenderId = Guid.Parse(senderId),
                MessageBody = messageBody,
               
                IsRead = false
            };
            await _messageRepo.AddAsync(message);
            await _messageRepo.SaveAsync();

            return new MessageDto
            {
                Id = message.Id,
                TicketId = message.SupportTicketId,
                SenderId = message.SenderId,
                SenderName = $"{sender.FirstName} {sender.LastName}",
                MessageBody = message.MessageBody,
                
                IsAdmin = ticket.AssignedAdminId.ToString() == senderId,
            };
        }

        public async Task<TicketChatViewModelDto> GetTicketMessagesAsync(Guid ticketId, string userId, bool isUserAdmin)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) throw new Exception("Talep bulunamadı.");

            
            if (!isUserAdmin && ticket.UserId.ToString() != userId)
                throw new UnauthorizedAccessException("Bu talebi görüntüleme yetkiniz yok.");

            var messages = await _messageRepo.Where(m => m.SupportTicketId == ticketId)
                .Include(m => m.Sender)
                .OrderBy(m=>m.CreatedDate)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    TicketId = m.SupportTicketId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                    MessageBody = m.MessageBody,
                    IsAdmin = ticket.AssignedAdminId == m.SenderId,
                    SentAt = m.CreatedDate
                }).ToListAsync();

            return new TicketChatViewModelDto
            {
                Messages = messages,
                Status = ticket.Status,
                Subject = ticket.Subject
            };
        }
    }
}
