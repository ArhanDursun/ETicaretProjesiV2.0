using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IGenericRepository<WalletTransaction> _walletTransactionRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IConfiguration _config;

        public AdminDashboardService(IGenericRepository<Order> orderRepo, IGenericRepository<WalletTransaction> walletTransactionRepo, IGenericRepository<Product> productRepo,IConfiguration config)
        {
            _orderRepo = orderRepo;
            _walletTransactionRepo = walletTransactionRepo;
            _productRepo = productRepo;
            _config = config;
        }

        public async Task<List<DailyComissionDto>> GetDailyComissionsAsync(string timeRange)
        {
            DateTime startDate = DateTime.MinValue;
            bool groupByMonth = false;

            if(timeRange == "7days")
            {
                startDate = DateTime.UtcNow.AddDays(-7);
            }else if(timeRange == "1month")
            {
                startDate = DateTime.UtcNow.AddMonths(-1);
            }else if(timeRange == "all")
            {
                groupByMonth = true;
            }

            var recentOrders = await _orderRepo.Where(o => o.OrderDate >= startDate && o.Status == OrderStatus.Confirmed).ToListAsync();

            var groupedData = recentOrders.GroupBy(o => groupByMonth ? new DateTime(o.OrderDate.Year, o.OrderDate.Month, 1) : o.OrderDate.Date);

            return groupedData.OrderBy(g => g.Key).Select(g => new DailyComissionDto
            {
                Date = groupByMonth ? g.Key.ToString("MMM yyyy") : g.Key.ToString("dd MMM"),
                TotalComission = g.Sum(o=>o.TotalPrices * 0.10m)
            }).ToList();
                                



        }

        public async Task<List<RecentOrderDto>> GetRecentOrdersAsync()
        {
            var recentOrders = await _orderRepo.Where(o => true).Include(o => o.AppUser).Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.Product).ThenInclude(p => p.Seller)
                                .OrderByDescending(o => o.OrderDate).Take(10).Select(o => new RecentOrderDto
                                {
                                    Id = o.Id.ToString().Substring(0, 8).ToUpper(),
                                    Customer = o.AppUser.FirstName + " " + o.AppUser.LastName,
                                    Seller = o.OrderItems.FirstOrDefault().Product.Seller.FirstName + " " + o.OrderItems.FirstOrDefault().Product.Seller.LastName,
                                    SellerId = o.OrderItems.FirstOrDefault().Product.SellerId.ToString(),
                                    Amount = o.TotalPrices,
                                    Commission = o.TotalPrices * 0.10m,
                                    Date = o.OrderDate,
                                    Status = o.Status == OrderStatus.Confirmed ? "Tamamlandı" :
                         o.Status == OrderStatus.Pending ? "Bekliyor" :
                         o.Status == OrderStatus.Shipped ? "Kargoda" : "İptal Edildi"
                                }).ToListAsync();

            return recentOrders;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {

            string adminUserId = _config["AdminSettings:AdminUserId"];
            var totalVolume = await _orderRepo.Where(o => o.Status != OrderStatus.Cancelled).SumAsync(o => o.TotalPrices);
            var totalCommission = await _walletTransactionRepo
        .Where(w => w.AppUserId == Guid.Parse(adminUserId))
        .SumAsync(w => w.Amount);

            var totalProducts = await _productRepo.Where(p => true).CountAsync();

            var activeSellers = await _productRepo.Where(p=> true).Select(p=> p.SellerId).Distinct().CountAsync();
            return new DashboardStatsDto
            {
                TotalPlatformVolume = totalVolume,
                TotalCommissionEarned = totalCommission,
                TotalProducts = totalProducts,
                ActiveSellers = activeSellers
            };
        }
    }
}
