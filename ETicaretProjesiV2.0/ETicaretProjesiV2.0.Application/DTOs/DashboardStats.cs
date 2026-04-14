using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.DTOs
{
    public class DashboardStatsDto
    {
        public decimal TotalPlatformVolume { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public int ActiveSellers { get; set; }
        public int TotalProducts { get; set; }
    }

    public class RecentOrderDto {
        public string Id { get; set; }
        public string Seller { get; set; }
        public string SellerId { get; set; }
        public string Customer { get; set; }
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}
