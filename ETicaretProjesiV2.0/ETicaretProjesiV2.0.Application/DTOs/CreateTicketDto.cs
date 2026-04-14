using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class CreateTicketDto
    {
        public TicketCategory Category { get; set; }
        public string Subject { get; set; }
        public string InitialMessage { get; set; }
    }

    public class TicketListDto
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string CategoryName { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
    }
}
