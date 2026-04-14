using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class AddBalanceRequestDto
    {
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpireMonth { get; set; }
        public string ExpireYear { get; set; }
        public string Cvv { get; set; }

        public decimal Amount { get; set; }
    }
}
