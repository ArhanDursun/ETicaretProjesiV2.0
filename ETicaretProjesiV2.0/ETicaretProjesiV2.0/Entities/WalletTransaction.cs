using ETicaretProjesiV2._0.Common;
using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class WalletTransaction : BaseEntity
    {
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; } 
        public string Description { get; set; } = string.Empty;
    }
}
