using ETicaretProjesiV2._0.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Entities
{
        public class Category : BaseEntity
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public ICollection<Product> Products { get; set; }
            public Guid? ParentCategoryId { get; set; }
            public ICollection<Category> SubCategories { get; set; } = new List<Category>();
            public Category? ParentCategory { get; set; }
        }
}
