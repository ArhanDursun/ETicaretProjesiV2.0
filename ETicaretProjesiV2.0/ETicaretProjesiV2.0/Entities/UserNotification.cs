using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class UserNotification : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } 
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public string Type { get; set; }
        public string? RelatedId { get; set; }
    }
}
