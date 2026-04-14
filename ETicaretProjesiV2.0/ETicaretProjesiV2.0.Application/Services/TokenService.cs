using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Application.Models;
using ETicaretProjesiV2._0.Entities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly TokenSettings _settings;
        public TokenService(TokenSettings settings)
        {
            _settings = settings; 
        }

        public string CreateTokenAsync(AppUser user ,AppRole role,bool rememberMe)
        {
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.NameId,user.Id.ToString()),
                new Claim("UserName",user.UserName),
                new Claim("FirstName",user.FirstName),
                new Claim("LastName",user.LastName)
            };
            if (role != null && !string.IsNullOrEmpty(role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = rememberMe ? DateTime.UtcNow.AddDays(7):DateTime.UtcNow.AddHours(1),
                SigningCredentials = creds,
                Issuer = _settings.Issuer,
                Audience = _settings.Audience
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}
