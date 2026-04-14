using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public List<IFormFile> ImageFiles { get; set; }
        public List<string> Images { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid SellerId { get; set; }
        public string? SellerName { get; set; }
        public double AverageStar { get; set; }
        public int CommentCount { get; set; }
        public bool IsFavorited { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public DateTime? DiscountEndDate { get; set; }
    }
}
