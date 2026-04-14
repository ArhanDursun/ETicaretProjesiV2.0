using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class CreateCommentDto
    {
        public Guid ProductId { get; set; }
        public string Content { get; set; }
        public double StarCount { get; set; }
    }
}
