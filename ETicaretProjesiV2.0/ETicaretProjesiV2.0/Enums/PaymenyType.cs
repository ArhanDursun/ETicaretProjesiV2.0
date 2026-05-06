using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Enums
{
    public enum PaymentType
    {
        DirectCheckOut = 1,
        WalletTopUp = 2,
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Sucess = 2,
        Failed = 3,
    }
}
