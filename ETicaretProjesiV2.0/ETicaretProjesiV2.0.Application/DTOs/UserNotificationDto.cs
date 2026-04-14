using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    
    public class UserNotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Type { get; set; }
        public string? RelatedId { get; set; }
    }
}
