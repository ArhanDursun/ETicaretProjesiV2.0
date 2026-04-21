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

        public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper, IEmailService emailService, RoleManager<AppRole> roleManager)
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
            if (user == null) throw new Exception("Kullanıcı bulunamadı");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return true;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Giriş bilgileri hatalı");
            if (user.IsDeleted) throw new Exception("Kullanıcı hesabı silinmiş");

            if (!user.EmailConfirmed)
                throw new Exception("Lütfen önce emailinize gelen doğrulama kodunu giriniz");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var role = await _roleManager.FindByNameAsync(userRoles.FirstOrDefault() ?? "User");
                var token = _tokenService.CreateTokenAsync(user, role, dto.RememberMe);

                return new LoginResponseDto
                {
                    Token = token,
                    Email = user.Email,
                    UserName = user.UserName!,
                    Balance = user.Balance.ToString("C")
                };
            }

            if (result.IsLockedOut)
                throw new Exception("Hesabınız geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.");

            throw new Exception("Giriş Bilgileri Hatalı");
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new Exception("Şifreler eşleşmiyor.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (user.PasswordResetCode != dto.Token)
                throw new Exception("Geçersiz token.");

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
                throw new Exception("Şifre sıfırlama işlemi başarısız oldu.");

            await _userManager.AddPasswordAsync(user, dto.NewPassword);
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpire = null;
            await _userManager.UpdateAsync(user);
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            string code = new Random().Next(100000, 999999).ToString();
            user.PasswordResetCode = code;
            user.PasswordResetCodeExpire = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateSecurityStampAsync(user);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception("Kod oluşturulurken bir hata oluştu.");

            string subject = "xyz Destek - Şifre Sıfırlama Kodu";
            string body = $"<h3>Merhaba,</h3><p>Doğrulama Kodunuz: <b style='font-size: 20px; color: #ff9800;'>{code}</b></p>";

            try
            {
                await _emailService.SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"E-posta gönderim hatası: {ex.Message}");
            }

            return code;
        }

        public async Task SendRegistrationCodeAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                if (!existingUser.IsDeleted) throw new Exception("Bu e-posta adresi zaten kullanılıyor");

                string suffix = $"_old_{DateTime.UtcNow.Ticks.ToString().Substring(12)}";
                existingUser.Email += suffix;
                existingUser.UserName += suffix;
                await _userManager.UpdateAsync(existingUser);
            }

            if (dto.Password != dto.ConfirmPassword) throw new Exception("Şifreler eşleşmiyor");

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
            if (!result.Succeeded)
                throw new Exception("Kayıt başarısız: " + result.Errors.FirstOrDefault()?.Description);

            await _userManager.AddToRoleAsync(user, "User");
            string body = $"<h1>Hoş Geldiniz!</h1><p>Doğrulama kodunuz: {user.EmailConfirmationCode}</p>";
            await _emailService.SendEmailAsync(user.Email, "Kayıt Doğrulama", body);
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
            if (!updateResult.Succeeded) throw new Exception("Onaylama işlemi başarısız.");

            var userRoles = await _userManager.GetRolesAsync(user);
            var role = await _roleManager.FindByNameAsync(userRoles.FirstOrDefault() ?? "User");

            return new LoginResponseDto
            {
                Token = _tokenService.CreateTokenAsync(user, role, false),
                Email = user.Email!,
                UserName = user.UserName!,
                Balance = user.Balance.ToString()
            };
        }

        public async Task UpdateUserAsync(Guid userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (user.Email != dto.Email)
            {
                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                    throw new Exception("Bu e-posta adresi zaten kullanılıyor.");

                user.Email = dto.Email;
                user.NormalizedEmail = dto.Email.ToUpper();
            }

            user.UserName = dto.UserName;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.ProfileImageUrl = dto.ProfileImageUrl;
            user.Description = dto.Description;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception("Güncelleme hatası: " + result.Errors.FirstOrDefault()?.Description);
        }

        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            return await _userManager.Users.Select(u => new UserListDto
            {
                Id = u.Id.ToString(),
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                UserName = u.UserName,
            }).ToListAsync();
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsDeleted = true;
            user.DeleteDate = DateTime.UtcNow;
            string suffix = $"_del_{DateTime.UtcNow.Ticks.ToString().Substring(12)}";
            user.Email += suffix;
            user.UserName += suffix;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(" | ", result.Errors.Select(e => e.Description)));

            return true;
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
                Description = user.Description
            };
        }

        public async Task<IEnumerable<UserSearchDto>> SearchUsersAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<UserSearchDto>();
            keyword = keyword.ToLower();

            return await _userManager.Users
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
        }

        public async Task<AppUser> GetUserIdAsync(string userId) => await _userManager.FindByIdAsync(userId);
    }
}