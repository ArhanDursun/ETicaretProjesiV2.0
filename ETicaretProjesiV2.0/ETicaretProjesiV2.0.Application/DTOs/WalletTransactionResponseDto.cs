using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class WalletTransactionResponseDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } 
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
