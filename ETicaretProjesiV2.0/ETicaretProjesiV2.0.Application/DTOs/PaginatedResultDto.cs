using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class PaginatedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPages { get; set; }
        public int PageSize { get; set; }
    }
}
