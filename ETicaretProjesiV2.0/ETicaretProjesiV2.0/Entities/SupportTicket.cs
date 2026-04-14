using ETicaretProjesiV2._0.Common;
using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class SupportTicket : BaseEntity
    {
        
        public Guid UserId { get; set; }
        public AppUser User { get; set; }
        public Guid? AssignedAdminId { get; set; }
        public AppUser AssignedAdmin { get; set; }

        public TicketCategory Category { get; set; }
        public string Subject { get; set; }
        public string InitialMessage { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Pending;
        
        public DateTime ClosedDate { get; set; }
        public ICollection<TicketMessage> Messages { get; set; }
    }
}
