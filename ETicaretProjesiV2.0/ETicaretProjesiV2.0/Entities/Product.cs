using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuanity { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; }
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        public Guid SellerId { get; set; }
        public AppUser? Seller { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public bool IsApproved { get; set; } = false;
        public ICollection<ProductComment> ProductComments { get; set; }
        public ICollection<ProductQuestion> ProductQuestions { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public DateTime? DiscountEndDate { get; set; }
    }
}
