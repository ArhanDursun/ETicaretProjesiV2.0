using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class AddItemToBasketDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }
}
