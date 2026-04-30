// Application/Consumers/GenerateReportEventConsumer.cs
using ClosedXML.Excel;
using ETicaretProjesiV2._0.Application.Events;
using ETicaretProjesiV2._0.Application.Interfaces; // 🔥 Interface'i kullanıyoruz
using ETicaretProjesiV2._0.Application.Interfaces.Services;
using ETicaretProjesiV2._0.Entities;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ETicaretProjesiV2._0.Application.Consumers
{
    public class GenerateReportEventConsumer : IConsumer<GenerateReportEvent>
    {
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly INotificationService _notificationService; 
        private readonly IWebHostEnvironment _env;

        public GenerateReportEventConsumer(IGenericRepository<Order> orderRepo, INotificationService notificationService, IWebHostEnvironment env)
        {
            _orderRepo = orderRepo;
            _notificationService = notificationService;
            _env = env;
        }

        public async Task Consume(ConsumeContext<GenerateReportEvent> context)
        {
            var message = context.Message;
            Console.WriteLine($"[RabbitMQ] 📊 {message.AdminUserId} için rapor hazırlanmaya başlandı...");

            var orders = await _orderRepo.Where(x => x.OrderDate >= message.StartDate && x.OrderDate <= message.EndDate)
                                         .Include(x => x.AppUser)
                                         .ToListAsync();

            string reportsFolder = Path.Combine(_env.WebRootPath, "reports");
            if (!Directory.Exists(reportsFolder)) Directory.CreateDirectory(reportsFolder);

            string fileName = $"SatisRaporu_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            string filePath = Path.Combine(reportsFolder, fileName);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Satışlar");

                worksheet.Cell(1, 1).Value = "Sipariş ID";
                worksheet.Cell(1, 2).Value = "Tarih";
                worksheet.Cell(1, 3).Value = "Müşteri";
                worksheet.Cell(1, 4).Value = "Tutar (TL)";
                worksheet.Cell(1, 5).Value = "Durum";
                worksheet.Range("A1:E1").Style.Font.Bold = true;
                worksheet.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.Blue;
                worksheet.Range("A1:E1").Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var order in orders)
                {
                    worksheet.Cell(row, 1).Value = order.Id.ToString().Substring(0, 8);
                    worksheet.Cell(row, 2).Value = order.OrderDate.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cell(row, 3).Value = order.AppUser?.UserName ?? "Bilinmiyor";
                    worksheet.Cell(row, 4).Value = order.TotalPrices;
                    worksheet.Cell(row, 5).Value = order.Status.ToString();
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }

            string fileUrl = $"https://localhost:7185/reports/{fileName}";

           
            await _notificationService.SendReportNotificationAsync(
                message.AdminUserId,
                "Raporunuz başarıyla oluşturuldu!",
                fileUrl
            );

            Console.WriteLine($"[RabbitMQ] ✅ Excel raporu çizildi ve Telsizden haber verildi!");
        }
    }
}