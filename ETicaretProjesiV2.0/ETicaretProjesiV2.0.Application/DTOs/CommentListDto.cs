using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class CommentListDto
    {
        public string UserName { get; set; }
        public string Content { get; set; }
        public double StarCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class CreateQuestionDto
    {
        public Guid ProductId { get; set; }
        public string QuestionContent { get; set; }
    }
    public class QuestionListDto
    {
        public Guid Id { get; set; } 
        public string UserName { get; set; }
        public string QuestionContent { get; set; }
        public string? AnswerContent { get; set; }
        public bool IsAnswered { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class AnswerQuestionDto
    {
        public Guid QuestionId { get; set; }
        public string? AnswerContent { get; set; }
    }
}
