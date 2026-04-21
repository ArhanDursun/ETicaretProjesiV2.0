using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class TicketMessage : BaseEntity
    {
        
        public Guid SupportTicketId { get; set; }
        public SupportTicket SupportTicket { get; set; }
        public Guid SenderId { get; set; }
        public AppUser Sender { get; set; }
        public string MessageBody { get; set; }
        
        public bool IsRead { get; set; } = false;
        public string MessageType { get; set; } = "text";
    }
}
