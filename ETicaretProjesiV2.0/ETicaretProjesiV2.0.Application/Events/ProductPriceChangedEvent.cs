using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Events
{
    public class ProductPriceChangedEvent
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
    }
}