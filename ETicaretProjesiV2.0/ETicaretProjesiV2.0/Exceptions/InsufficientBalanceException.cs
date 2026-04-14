using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Exceptions
{
    public class  InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException(): base("Yetersiz Bakiye") { }
        public InsufficientBalanceException(decimal currentBalance,decimal requiredAmount) : base($"Yetersiz Bakiye Güncel Bakiyeniz{currentBalance} gereken {requiredAmount}") { }
    }
}
