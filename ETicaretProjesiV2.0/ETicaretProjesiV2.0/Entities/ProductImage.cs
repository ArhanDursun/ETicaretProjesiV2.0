using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; }
        public bool IsMain { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
