using ETicaretProjesiV2._0.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ETicaretProjesiV2._0.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to,string subject,string body)
        {
            var email = _configuration["EmailSettings:Email"];
            var password = _configuration["EmailSettings:Password"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
            var displayName = _configuration["EmailSettings:DisplayName"];

            using (var smtpClient = new SmtpClient(host, port))
            {
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false; 
                smtpClient.Credentials = new NetworkCredential(email, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email, displayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

    }
}
