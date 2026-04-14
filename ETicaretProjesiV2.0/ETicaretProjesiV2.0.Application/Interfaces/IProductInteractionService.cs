using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IProductInteractionService
    {
        Task AddCommentAsync(Guid userId, CreateCommentDto dto);
        Task<List<CommentListDto>> GetCommentsByProductIdAsync(Guid productId);

        Task AddQuestionAsync(Guid userId, CreateQuestionDto dto);
        Task<List<QuestionListDto>> GetQuestionsByProductIdAsync(Guid productId);
        Task AnswerQuestionAsync(Guid sellerId, AnswerQuestionDto dto);
    }
}
