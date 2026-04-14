using ETicaretProjesiV2._0.Common;
using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class Order : BaseEntity
    {
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        public decimal TotalPrices { get; set; }
        public OrderStatus Status { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
