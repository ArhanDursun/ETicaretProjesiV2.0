using ETicaretProjesiV2._0.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class PaymentRequestDto
    {
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpireMonth { get; set; }
        public string ExpireYear { get; set; }
        public string Cvc { get; set; }
        public decimal Price { get; set; }
        public PaymentType PaymentType { get; set; }


        public string BuyerName { get; set; }
        public string BuyerSurname { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerGsmNumber { get; set; }
        public string BuyerIdentityNumber { get; set; }


        public string City { get; set; }
        public string Country { get; set; }
        public string AddressDescription { get; set; }
        public string ZipCode { get; set; }

        public string IpAddress { get; set; }
    }

    public class PaymentResultDto
    {
        public bool IsSuccess { get; set; }
        public string? BankTransactionId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
