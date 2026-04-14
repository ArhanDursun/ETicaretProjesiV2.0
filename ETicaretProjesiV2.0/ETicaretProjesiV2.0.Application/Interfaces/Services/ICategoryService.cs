using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(Guid id);
        Task CreateCategoryAsync(CategoryDto dto);
        Task UpdateCategoryAsync(Guid id, CategoryDto dto);
        Task DeleteCategoryAsync(Guid id);
    }
}
