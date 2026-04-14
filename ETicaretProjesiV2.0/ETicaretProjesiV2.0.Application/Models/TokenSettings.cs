using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Models
{
    public class TokenSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecurityKey { get; set; }
    }
}
