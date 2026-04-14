using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
            : base("İstenilen kayıt bulunamadı.")
        {
        }

        public NotFoundException(string name, object key)
            : base($"\"{name}\" isimli varlık, ({key}) anahtarı ile sistemde bulunamadı.")
        {
        }
    }
}
