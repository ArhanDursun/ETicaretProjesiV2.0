using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class CreateOrderRequestDto
    {
        public List<OrderItemDto> Items {  get; set; }
    }
    public class OrderItemDto
    {
        public Guid ProductId { get; set; }

       
        public int Quanity { get; set; }

        
        public decimal UnitPrice { get; set; }

       
        public ProductDto Product { get; set; }
    }
}
