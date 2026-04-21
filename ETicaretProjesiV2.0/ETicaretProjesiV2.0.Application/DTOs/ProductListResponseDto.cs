using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class ProductListResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int StockQuanity { get; set; }
        public string CategoryName { get; set; }
        public List<string> Images { get; set; }
        public double AverageStar { get; set; }
        public int  CommentCount { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public string SellerName { get; set; }

    }
}
