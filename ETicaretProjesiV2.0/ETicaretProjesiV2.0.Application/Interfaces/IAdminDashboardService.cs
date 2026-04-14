using ETicaretProjesiV2._0.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();
        Task<List<RecentOrderDto>> GetRecentOrdersAsync();
        Task<List<DailyComissionDto>> GetDailyComissionsAsync(string timeRange);
    }
}
