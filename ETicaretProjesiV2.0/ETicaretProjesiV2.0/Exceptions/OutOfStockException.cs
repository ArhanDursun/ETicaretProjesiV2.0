using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Exceptions
{
    public class OutOfStockException : Exception
    {
        public OutOfStockException() : base("Yetersiz Stok") { }
        public OutOfStockException(string currentStock , string requiredStock) : base($"Yeteri kadar ürün yok, Almak istediğiniz ürün sayısı {requiredStock}, elde olan ürün sayısı {currentStock}") {
    }
}
}

