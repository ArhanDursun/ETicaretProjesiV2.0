using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class DepositRequestDto
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = "Kullanıcı Manuel bakiye yüklemesi";
    }
}
