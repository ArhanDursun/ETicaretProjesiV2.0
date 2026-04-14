using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class ChatListItemDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageDate { get; set; }
        public int UnreadCount { get; set; }
    }
}
