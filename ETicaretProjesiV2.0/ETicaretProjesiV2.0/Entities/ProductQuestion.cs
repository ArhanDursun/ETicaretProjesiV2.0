using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class ProductQuestion :BaseEntity
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid AppUserId { get; set; }
       
        public AppUser AppUser { get; set; } 
        public string QuestionContent { get; set; }
        public string? AnswerContent { get; set; }
        public bool IsAnswered { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    }
}
