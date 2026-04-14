using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quanity { get; set; }
        public decimal UnitPrice { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
