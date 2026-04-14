using ETicaretProjesiV2._0.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Enums
{
    public enum TicketStatus
    {
        Pending = 1,
        Active = 2,
        Rejected = 3,
        Closed = 4
    }
    public enum TicketCategory
    {
        OrderIssue = 1,
        ReturnRequest = 2,
        TechnicalSupport = 3,
        GeneralQuestion = 4,
        Other = 5
    }
}
