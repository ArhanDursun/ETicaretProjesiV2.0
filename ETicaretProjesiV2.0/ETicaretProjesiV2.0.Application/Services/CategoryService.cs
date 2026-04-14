using ETicaretProjesiV2._0.Application.Interfaces.Services;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Entities;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class CategoryService : ICategoryService
    {
        public readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task CreateCategoryAsync(CategoryDto dto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ParentCategoryId = dto.ParentCategoryId
            };
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category =  await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new Exception("Silinecek kategori bulunamadı");
            }
            if (category.SubCategories != null && category.SubCategories.Any())
            {
                throw new Exception("Bu kategorinin alt kategorileri var önce onları sil"); 
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveAsync();

        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllWithSubCategoriesAsync();

            return categories.Where(c => c.ParentCategoryId == null).Select(c => MapToDto(c)).ToList();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if(category == null)
                throw new Exception("Kategori bulunamadı");
            return MapToDto(category);
        }

        public async Task UpdateCategoryAsync(Guid id, CategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new Exception("Güncellenecek Kategori bulunamadı");
            if (dto.ParentCategoryId == id)
                throw new Exception("Bir kategori kendisinin üst kategorisi olamaz");

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ParentCategoryId = dto.ParentCategoryId;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveAsync();
        }

        private static CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                SubCategories = category.SubCategories?.Select(MapToDto).ToList()
            };
        }
    }
}
