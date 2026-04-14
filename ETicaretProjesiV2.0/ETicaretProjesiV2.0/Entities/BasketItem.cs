using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class BasketItem : BaseEntity
    {
        public Guid BasketId { get; set; }
        public  Basket Basket { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
