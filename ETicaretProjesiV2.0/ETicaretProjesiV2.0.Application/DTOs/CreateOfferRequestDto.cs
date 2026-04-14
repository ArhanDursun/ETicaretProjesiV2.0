using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class CreateOfferRequestDto
    {
        public Guid ProductId { get; set; }
        public decimal OfferedPrice { get; set; }
        public int OfferQuantity { get; set; }
    }
}
