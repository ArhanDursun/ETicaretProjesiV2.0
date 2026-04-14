using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class BasketDto
    {
        public Guid BasketId { get; set; }
        public List<BasketItemDto> Items { get; set; } = new List<BasketItemDto>();

        public decimal TotalBasketPrices { get; set; }
    }

    public class BasketItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public List<string>  Images { get; set; }

        public bool IsOfferItem { get; set; }
        public Guid? OfferId { get; set; } 
    }
}
