using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }
    public class UpdateCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }
}
