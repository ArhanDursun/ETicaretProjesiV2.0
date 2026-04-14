using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Balance { get; set; }
       
    }
}
