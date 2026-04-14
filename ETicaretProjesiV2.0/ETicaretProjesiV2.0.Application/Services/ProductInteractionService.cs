using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class ProductInteractionService : IProductInteractionService
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<ProductQuestion> _productQuestionRepo;
        private readonly IGenericRepository<ProductComment> _productCommentRepo;

        public ProductInteractionService(IGenericRepository<Product> productRepo, IGenericRepository<ProductQuestion> productQuestionRepo, IGenericRepository<ProductComment> productCommentRepo)
        {
            _productRepo = productRepo;
            _productQuestionRepo = productQuestionRepo;
            _productCommentRepo = productCommentRepo;

        }

        public async Task AddCommentAsync(Guid userId, CreateCommentDto dto)
        {
            var comment = new ProductComment
            {
                Id = Guid.NewGuid(),
                AppUserId = userId,
                ProductId = dto.ProductId,
                Content = dto.Content,
                StarCount = dto.StarCount,
                CreatedDate = DateTime.UtcNow
            };
            await _productCommentRepo.AddAsync(comment);
            await _productCommentRepo.SaveAsync();
        }

        public async Task AddQuestionAsync(Guid userId, CreateQuestionDto dto)
        {
            var question = new ProductQuestion
            {
               AppUserId = userId,
               ProductId= dto.ProductId,
               QuestionContent = dto.QuestionContent,
               IsAnswered = false,
                CreatedDate = DateTime.UtcNow
            };

            await _productQuestionRepo.AddAsync(question);
            await _productQuestionRepo.SaveAsync();
        }

        public async Task AnswerQuestionAsync(Guid sellerId, AnswerQuestionDto dto)
        {
            var question = await _productQuestionRepo.GetByIdAsync(dto.QuestionId);
            if (question == null) throw new Exception("Soru bulunamadı.");

            
            var product = await _productRepo.GetByIdAsync(question.ProductId);
            if (product.SellerId != sellerId) throw new Exception("Bu soruya sadece ürünün satıcısı cevap verebilir!");

            question.AnswerContent = dto.AnswerContent;
            question.IsAnswered = true;

            _productQuestionRepo.Update(question);
            await _productQuestionRepo.SaveAsync();
        }

        public async Task<List<CommentListDto>> GetCommentsByProductIdAsync(Guid productId)
        {
            return await _productCommentRepo.Where(c => c.ProductId == productId)
                .Include(c => c.AppUser) 
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CommentListDto
                {
                    UserName = c.AppUser.UserName,
                    Content = c.Content,
                    StarCount = c.StarCount,
                    CreatedDate = c.CreatedDate
                }).ToListAsync();
        }

        public async Task<List<QuestionListDto>> GetQuestionsByProductIdAsync(Guid productId)
        {
            return await _productQuestionRepo.Where(q => q.ProductId == productId)
                .Include(q => q.AppUser)
                .OrderByDescending(q => q.CreatedDate)
                .Select(q => new QuestionListDto
                {
                    Id = q.Id,
                    UserName = q.AppUser.UserName,
                    QuestionContent = q.QuestionContent,
                    AnswerContent = q.AnswerContent,
                    IsAnswered = q.IsAnswered,
                    CreatedDate = q.CreatedDate
                }).ToListAsync();
        }
    }
}
