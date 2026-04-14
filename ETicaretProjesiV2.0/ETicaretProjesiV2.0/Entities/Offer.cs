using ETicaretProjesiV2._0.Common;
using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class Offer : BaseEntity
    {
        
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public Guid BuyerId { get; set; }
        public AppUser Buyer { get; set; }
        public Guid SellerId { get; set; }
        public AppUser Seller { get; set; }
        public decimal OfferedPrice { get; set; }
        public OfferStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal? CounterPrice { get; set; }
        public int Quantity { get; set; }

    }
}
