using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class OrderListResponseDto
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal SellerEarning { get; set; }
        public string Status { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
