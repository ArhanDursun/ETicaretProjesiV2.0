using ETicaretProjesiV2._0.Common;
using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? BankTransactionId { get; set; }
        public PaymentType Type { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
