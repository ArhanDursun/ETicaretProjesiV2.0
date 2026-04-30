using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Events
{
    public record OrderCreatedEvent(Guid OrderId, string BuyerEmail, decimal TotalPrice);
}
