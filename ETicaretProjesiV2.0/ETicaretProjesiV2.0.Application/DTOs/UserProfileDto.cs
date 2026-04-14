using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class UserProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string Description { get; set; }
    }
}
