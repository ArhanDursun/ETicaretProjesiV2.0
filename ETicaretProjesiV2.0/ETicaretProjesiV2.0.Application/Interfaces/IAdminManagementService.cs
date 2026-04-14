using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IAdminManagementService
    {
        Task<List<AdminProductDto>> GetAllProductsAsync();  
        Task DeleteProductWithReasonAsync(DeleteProductDto dto);
        Task<UserDetailDto> GetUserDetailsAsync(Guid userId);
        Task<List<AdminProductDto>> GetUserProductsAsync(Guid userId);
    }
}
