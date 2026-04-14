using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class TicketChatViewModelDto
    {
        public List<MessageDto> Messages { get; set; }
        public TicketStatus Status { get; set; }
        public string Subject { get; set; }
    }
}
