using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> ForgotPasswordAsync(string email);
        Task SendRegistrationCodeAsync(RegisterRequestDto dto);

        Task<LoginResponseDto> VerifyAndCompleteRegisterAsync(VerifyCodeDto dto);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto);
        Task GenerateForgotPasswordTokenAsync(ForgotPasswordRequestDto dto);
        Task ResetPasswordAsync(ResetPasswordRequestDto dto);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
        Task UpdateUserAsync(Guid userId, UpdateUserDto dto);
        Task<List<UserListDto>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> ResendVerificationCodeAsync(string email);

        Task<AppUser> GetUserIdAsync(string userId);
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<UserProfileDto> GetPublicProfileAsync(Guid userId);
        Task<IEnumerable<UserSearchDto>> SearchUsersAsync(string keyword);
    }
}
