using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETicaretProjesiV2._0.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService authService, IWebHostEnvironment env)
        {
            _authService = authService;
            _env = env;
        }

        [HttpPost("register-request")]
        public async Task<IActionResult> RegisterRequest(RegisterRequestDto dto)
        {
           
                await _authService.SendRegistrationCodeAsync(dto);
                return Ok(new { Message = "Kayıt kodu mailinize gönderildi" });
            
        }
        [HttpPost("register-verification")]
        public async Task<IActionResult> VerifyRegister(VerifyCodeDto dto)
        {
           
                var response = await _authService.VerifyAndCompleteRegisterAsync(dto);
                return Ok(response);
           
        }
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { Message = "Kullanıcı kimliği bulunamadı" });

            var userId = Guid.Parse(userIdString);
            
                var result = await _authService.ChangePasswordAsync(userId, dto);
                if (result)
                    return Ok(new { Message = "Şifre başarıyla değiştirildi" });

                return BadRequest(new { Message = "Şifre değiştirme başarısız" });
           
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            
                await _authService.ForgotPasswordAsync(dto.Email);
                return Ok(new { Message = "Şifre sıfırlama kodu mailinize gönderildi" });
           
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
           
                await _authService.ResetPasswordAsync(dto);
                return Ok(new { Message = "Şifre başarıyla sıfırlandı" });
            
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            
                var result = await _authService.LoginAsync(dto);


                return Ok(result);

        }
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { Message = "Kullanıcı kimliği bulunamadı" });
            }
           
                await _authService.UpdateUserAsync(userId, dto);
                return Ok(new { Message = "Profil başarıyla güncellendi" });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
          
                var users = await _authService.GetAllUsersAsync();
                return Ok(users);
            
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            
                var isDeleted = await _authService.DeleteUserAsync(id);
                if (!isDeleted)
                {
                    return BadRequest(new { Message = "Kullanıcı Bulunamadı Veya Silinemedi" });
                }
                return Ok(new { Message = "Kullanıcı Başarıyla Silindi" });
            
        }
        [HttpPost("resend-verification-code")]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendCodeDto dto)
        {
           
                await _authService.ResendVerificationCodeAsync(dto.Email);
                return Ok(new { Message = "Yeni doğrulama kodu e-posta adresinize gönderildi." });
            
        }

        [Authorize]
        [HttpGet("getProfile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Kullanıcı Kimliği Doğrulanamadı" });

            var user = await _authService.GetUserIdAsync(userIdClaim);

            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                balance = user.Balance,
            });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Kullanıcı kimliği doğrulanamadı");

            var profile = await _authService.GetUserProfileAsync(userId);

            return Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Kullanıcı kimliği doğrulanamadı");

            await _authService.UpdateUserAsync(Guid.Parse(userId), dto);

            return Ok(new { message = "Profil bilgileri başarıyla güncellendi." });

        }
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePasswordByMe([FromBody] ChangePasswordRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Kullanıcı kimliği doğrulamadı");

            var result = await _authService.ChangePasswordAsync(Guid.Parse(userId), dto);

            if (result)
                return Ok(new { message = "Şifreniz başarıyla değiştirildi." });

            return BadRequest(new { message = "Şifre depiştirme işlemi başarısız oldu" });
        }

        [Authorize]
        [HttpPost("upload-profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Lütfen bir resim dosyası seçin.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();


            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);


            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);


            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }


            var imageUrl = $"/uploads/profiles/{uniqueFileName}";
            return Ok(new { imageUrl });
        }
        [HttpGet("public-profile/{sellerId}")]
        public async Task<IActionResult> GetPublicProfile(string sellerId)
        {

            var profile = await _authService.GetPublicProfileAsync(Guid.Parse(sellerId));
            return Ok(profile);
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
        {
            var users = await _authService.SearchUsersAsync(q);
            return Ok(users);
        }
    }

}