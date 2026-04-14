using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class OfferResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public decimal OfferedPrice { get; set; }
        public OfferStatus Status { get; set; }
        public string BuyerName { get; set; }
        public DateTime CreatedTime { get; set; }
        public decimal? CounterPrice { get; set; }
        public int Quantity { get; set; }

    }
}
