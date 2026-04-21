using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class SendMessageDto
    {
        public Guid TicketId { get; set; }
        public string MessageBody { get; set; }

    }
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string MessageBody { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsAdmin { get; set; }
        public string MessageType { get; set; }
    }
}
