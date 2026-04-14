using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class Basket : BaseEntity
    {
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public ICollection<BasketItem> Items { get; set; } = new List<BasketItem>();
    }
}
