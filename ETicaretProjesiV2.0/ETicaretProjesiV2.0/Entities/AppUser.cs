using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public decimal Balance { get; set; } = 0;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpire { get; set; }
        public string? EmailConfirmationCode { get; set; }  
        public DateTime? EmailConfirmationCodeExpire { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public string? ProfileImageUrl { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public  bool IsDeleted { get; set; } = false;
        public DateTime? DeleteDate { get; set; }

    }
}
