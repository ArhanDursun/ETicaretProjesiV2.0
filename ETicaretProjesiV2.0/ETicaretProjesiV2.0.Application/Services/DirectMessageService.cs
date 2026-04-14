using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class DirectMessageService : IDirectMessageService
    {

        private readonly IGenericRepository<DirectMessage> _messageRepo;
        private readonly UserManager<AppUser> _userManager;

        public DirectMessageService(IGenericRepository<DirectMessage> messageRepo, UserManager<AppUser> userManager)
        {
            _messageRepo = messageRepo;
            _userManager = userManager;
        }

        
        public async Task<DirectMessageDto> SendMessageAsync(string senderId, string receiverId, string content)
        {
            var message = new DirectMessage
            {
                Id = Guid.NewGuid(),
                SenderId = Guid.Parse(senderId),
                ReceiverId = Guid.Parse(receiverId),
                Content = content,
                IsRead = false,
                SentDate = DateTime.UtcNow
            };

            await _messageRepo.AddAsync(message);
            await _messageRepo.SaveAsync();

            return new DirectMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                SentDate = message.SentDate
            };
        }

        
        public async Task<List<DirectMessageDto>> GetChatHistoryAsync(string userId, string otherUserId)
        {
            var uId = Guid.Parse(userId);
            var oId = Guid.Parse(otherUserId);

            return await _messageRepo
                .Where(m => (m.SenderId == uId && m.ReceiverId == oId) ||
                            (m.SenderId == oId && m.ReceiverId == uId))
                .OrderBy(m => m.SentDate)
                .Select(m => new DirectMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    SentDate = m.SentDate,
                    IsRead = m.IsRead
                }).ToListAsync();
        }


        public async Task<List<ChatListItemDto>> GetRecentChatsAsync(string userId)
        {
            var uId = Guid.Parse(userId);

            var allMyMessages = await _messageRepo
                .Where(m => m.SenderId == uId || m.ReceiverId == uId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.SentDate)
                .ToListAsync();

            var recentChats = allMyMessages
                .GroupBy(m => m.SenderId == uId ? m.ReceiverId : m.SenderId)
                .Select(group =>
                {
                    var lastMsg = group.First();
                    var otherUser = lastMsg.SenderId == uId ? lastMsg.Receiver : lastMsg.Sender;

                    return new ChatListItemDto
                    {
                        UserId = otherUser.Id,
                        UserName = $"{otherUser.FirstName} {otherUser.LastName}",
                        LastMessage = lastMsg.Content,
                        LastMessageDate = lastMsg.SentDate,
                        UnreadCount = group.Count(m => m.ReceiverId == uId && !m.IsRead)
                    };
                })
                .ToList();

            return recentChats;
        }

        public async Task<List<MessageableUserDto>> GetAvailableUsersAsync(string currentUserId)
        {
            var users = await _userManager.GetUsersInRoleAsync("User");

            return users.Where(u=> u.Id.ToString() != currentUserId)
                        .Select(u=> new MessageableUserDto
                        {
                            Id = u.Id.ToString(),
                            FullName = $"{u.FirstName} {u.LastName}",
                        }).ToList();
        }

        public async Task MarkAsReadAsync(string myId, string otherUserId)
        {
            var mId = Guid.Parse(myId);
            var oId = Guid.Parse(otherUserId);

            var unreadMessages = await _messageRepo
                .Where(m => m.ReceiverId == mId && m.SenderId == oId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true; 
                    _messageRepo.Update(msg); 
                }

                await _messageRepo.SaveAsync(); 
            }
        }
    }
}
