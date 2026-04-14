using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class UserFavorite : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public AppUser User { get; set; }
        public Product Product { get; set; }
    }
}
