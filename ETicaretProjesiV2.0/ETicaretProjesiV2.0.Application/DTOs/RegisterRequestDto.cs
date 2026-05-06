using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class RegisterRequestDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Şifreler Birbirine Uyuşmuyor")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

    }
}
