using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
    public class AppRole : IdentityRole<Guid>
    {
       
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
