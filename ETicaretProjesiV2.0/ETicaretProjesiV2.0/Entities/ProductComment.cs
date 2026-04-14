using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class ProductComment :BaseEntity
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public double StarCount { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
