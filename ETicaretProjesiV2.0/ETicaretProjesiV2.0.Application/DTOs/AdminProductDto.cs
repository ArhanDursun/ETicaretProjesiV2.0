using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class AdminProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SellerName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; }
        public List<string> Images { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public DateTime? DiscountEndDate { get; set; }

    }
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public string Role { get; set; }
        public DateTime JoinDate { get; set; }
    }

    public class DeleteProductDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
    }
}
