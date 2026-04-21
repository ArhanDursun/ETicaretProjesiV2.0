using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IDirectMessageService
    {
        Task<DirectMessageDto> SendMessageAsync(string senderId, string receiverId, string content,string messageType ="text");
        Task<List<DirectMessageDto>> GetChatHistoryAsync(string userId, string otherUserId);
        Task<List<ChatListItemDto>> GetRecentChatsAsync(string userId);
        Task<List<MessageableUserDto>> GetAvailableUsersAsync(string currentUserId);
        Task MarkAsReadAsync(string myId, string otherUserId);
    }
}
