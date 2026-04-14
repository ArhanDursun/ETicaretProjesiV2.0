using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
     public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmedNewPassword { get; set; }
    }
}
