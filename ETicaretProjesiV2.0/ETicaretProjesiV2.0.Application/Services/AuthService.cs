using AutoMapper;
using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly RoleManager<AppRole> _roleManager;
        public AuthService(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,ITokenService tokenService,IMapper mapper ,IEmailService emailService,RoleManager<AppRole> roleManager )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _emailService = emailService;
            _roleManager = roleManager;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto)
        {
            if (dto.NewPassword != dto.ConfirmedNewPassword)
                throw new Exception("Yeni şifreler eşleşmiyor");
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null) throw new Exception("Kullanıcı bulunamadı");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
            return result.Succeeded;
        }
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Giriş bilgileri hatalı");

            if (!user.EmailConfirmed)
            {
                throw new Exception("Lütfen Giriş yapmadan önce emailinize gelen doğrulama kodunu giriniz");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password,lockoutOnFailure:true);

            if (result.Succeeded)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var firstRoleName = userRoles.FirstOrDefault();
                var role = await _roleManager.FindByNameAsync(firstRoleName);

                var token = _tokenService.CreateTokenAsync(user, role,dto.RememberMe);

                return new LoginResponseDto
                {
                    Token = token,
                    Email = user.Email,
                    UserName = user.UserName!,
                    Balance = user.Balance.ToString("C")
                };
            }
            if( result.IsLockedOut)
            {
                throw new Exception("Hesabınız geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.");
            }
            throw new Exception("Giriş Bilgileri Hatalı");

        }

        

        public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new Exception("Şifreler eşleşmiyor.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (user.PasswordResetCode != dto.Token)
            {
                throw new Exception("Geçersiz token.");
            }
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (removeResult.Succeeded)
            {
                await _userManager.AddPasswordAsync(user, dto.NewPassword);

                user.PasswordResetCode = null;
                user.PasswordResetCodeExpire= null;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                throw new Exception("Şifre sıfırlama işlemi başarısız oldu.");
            }
        }
       
        
        public async Task GenerateForgotPasswordTokenAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            string mailbody = $"Merhaba {user.FirstName},<br>Şifrenizi sıfırlamak için aşağıdaki kodu kullanabilirsiniz: <b>{token}</b>";
            await _emailService.SendEmailAsync(user.Email, "Şifre Sıfırlama Talebi", mailbody);
        }


        public async Task<string> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null) throw new Exception("Kullanıcı bulunamadı.");

                string code = new Random().Next(100000, 999999).ToString();
                user.PasswordResetCode = code;
                user.PasswordResetCodeExpire = DateTime.UtcNow.AddMinutes(15);

                await _userManager.UpdateSecurityStampAsync(user);
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var error = result.Errors.FirstOrDefault()?.Description;
                    throw new Exception($"Kod kaydedilemedi: {error}");
                }

                Console.WriteLine($"E-posta: {email} | Kod: {code}");
                string subject = "xyz Destek - Şifre Sıfırlama Kodu";
                string body = $@"
            <h3>Merhaba,</h3>
            <p>Şifrenizi sıfırlamak için talebinizi aldık.</p>
            <p>Doğrulama Kodunuz: <b style='font-size: 20px; color: #ff9800;'>{code}</b></p>
            <p><i>Bu kod 15 dakika boyunca geçerlidir.</i></p>";

                
                await _emailService.SendEmailAsync(email, subject, body);

                return code;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HATA: {ex.Message}");
                throw;
            }
        }

        public async Task SendRegistrationCodeAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null) throw new Exception("Bu e-posta adresi zaten kayıtlı.");
            if (dto.Password != dto.ConfirmPassword)
            {
                throw new Exception("Girdiğiniz şifreler eşleşmiyor");
            }
            var user = new AppUser
            {
                Email = dto.Email,
                UserName = dto.Username,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailConfirmed = false,
                EmailConfirmationCode = new Random().Next(100000, 999999).ToString(),
                EmailConfirmationCodeExpire = DateTime.UtcNow.AddMinutes(15)
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                string subject = "Kayıt Doğrulama Kodu";
                string body = $@"<div style='font-family: Arial;'>
                            <h2>Hoş Geldiniz!</h2>
                            <p>Kayıt işleminizi tamamlamak için doğrulama kodunuz:</p>
                            <h1 style='color: #2c3e50;'>{user.EmailConfirmationCode}</h1>
                            <p>Bu kod 15 dakika geçerlidir.</p>
                         </div>";
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }else
                throw new Exception("Kullanıcı oluşturulamadı: " + result.Errors.FirstOrDefault()?.Description);
        }

        public async Task<LoginResponseDto> VerifyAndCompleteRegisterAsync(VerifyCodeDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.Trim());

            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (user.EmailConfirmationCode != dto.Code || user.EmailConfirmationCodeExpire < DateTime.UtcNow)
                throw new Exception("Geçersiz veya süresi dolmuş kod.");

            user.EmailConfirmed = true;
            user.EmailConfirmationCode = null;
            user.EmailConfirmationCodeExpire = null;

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var firstRoleName = userRoles.FirstOrDefault() ?? "User"; 
                var role = await _roleManager.FindByNameAsync(firstRoleName);
                return new LoginResponseDto
                {
                    Token = _tokenService.CreateTokenAsync(user, role,false),
                    Email = user.Email!,
                    UserName = user.UserName!,
                    Balance = user.Balance.ToString()

                };
            }

            throw new Exception("Kullanıcı onaylanırken bir hata oluştu.");
        }

        public async Task UpdateUserAsync(Guid userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");
            if (user.Email != dto.Email)
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingEmail != null) throw new Exception("Bu e-posta adresi zaten kullanılıyor.");
                user.Email = dto.Email;
                user.NormalizedEmail = dto.Email.ToUpper();
            }
            user.UserName = dto.UserName;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.NormalizedUserName = dto.UserName.ToUpper();
            user.ProfileImageUrl = dto.ProfileImageUrl;
            user.Description = dto.Description;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception("Güncelleme hatası: " + result.Errors.FirstOrDefault()?.Description);
            }
        }

        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            var userList = users.Select(u => new UserListDto
            {
                Id = u.Id.ToString(),
                FirstName= u.FirstName,
                LastName= u.LastName,
                Email = u.Email,
                UserName = u.UserName,
            }).ToList();

            return userList;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
            return true;
        }

        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("Kullanıcı bulunamadı");

            if (user.EmailConfirmed == true) throw new Exception("Bu mail zaten onaylı");

            string newCode = Random.Shared.Next(100000, 999999).ToString();
            user.EmailConfirmationCode = newCode;
            await _userManager.UpdateAsync(user);
            string subject = "Kayıt Doğrulama Kodu";
            string body = $@"<div style='font-family: Arial;'>
                            <h2>Hoş Geldiniz!</h2>
                            <p>Kayıt işleminizi tamamlamak için doğrulama kodunuz:</p>
                            <h1 style='color: #2c3e50;'>{user.EmailConfirmationCode}</h1>
                            <p>Bu kod 15 dakika geçerlidir.</p>
                         </div>";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return true;
        }

        public async Task<AppUser> GetUserIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");

            return new UserProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Balance = user.Balance,
                ProfileImageUrl = user.ProfileImageUrl,
                Description = user.Description,
               
            };
        }

        public async Task<UserProfileDto> GetPublicProfileAsync(Guid userId)
        {
           var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");
            return new UserProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                ProfileImageUrl = user.ProfileImageUrl,
                Description = user.Description,
            };
        }

        public async Task<IEnumerable<UserSearchDto>> SearchUsersAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<UserSearchDto>();

            keyword = keyword.ToLower();

            var users = await _userManager.Users
        .Where(u => u.UserName.ToLower() != "admin" &&
                   (u.UserName.ToLower().Contains(keyword) ||
                    u.FirstName.ToLower().Contains(keyword) ||
                    u.LastName.ToLower().Contains(keyword)))
        .Take(5)
        .Select(u => new UserSearchDto
        {
            Id = u.Id,
            FullName = u.FirstName + " " + u.LastName,
            UserName = u.UserName,
            ProfileImageUrl = u.ProfileImageUrl
        }).ToListAsync();

            return users;
        }
    }
}
