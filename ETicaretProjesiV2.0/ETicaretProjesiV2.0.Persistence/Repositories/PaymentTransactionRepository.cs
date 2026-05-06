using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Persistence.Repositories
{
    public class PaymentTransactionRepository : GenericRepository<PaymentTransaction>,IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(ApplicationDbContext context) : base(context) { }
    }
}
