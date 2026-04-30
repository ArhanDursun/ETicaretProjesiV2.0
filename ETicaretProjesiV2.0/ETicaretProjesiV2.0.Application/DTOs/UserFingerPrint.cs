using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class UserFingerPrint
    {
        public string IpAddress { get; set; }
        public string UserDevice { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
