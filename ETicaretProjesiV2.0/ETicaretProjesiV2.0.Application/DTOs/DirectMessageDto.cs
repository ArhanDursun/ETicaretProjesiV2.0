using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class DirectMessageDto : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentDate { get; set; }

        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
    }
}
