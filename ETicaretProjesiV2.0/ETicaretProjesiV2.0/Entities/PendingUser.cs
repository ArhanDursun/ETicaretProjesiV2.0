using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class PendingUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string VerificationCode { get; set; } =string.Empty;
        public DateTime ExpiryDate { get; set; }


    }
}
