using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Application.Interfaces.Services;
using ETicaretProjesiV2._0.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class AdminManagementService :IAdminManagementService
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public AdminManagementService(IGenericRepository<Product> productRepository, UserManager<AppUser> userManager, IEmailService emailService, INotificationService notificationService)
        {
            _productRepo = productRepository;
            _userManager = userManager;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task DeleteProductWithReasonAsync(DeleteProductDto dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null) throw new Exception($"Ürün bulunamadı. Gelen ID: {dto.ProductId}");

            var seller =  await _userManager.FindByIdAsync(product.SellerId.ToString());
            if (seller == null) throw new Exception("Satıcı bulunamadı");
            _productRepo.Delete(product);
            await _productRepo.SaveAsync();

            var notificationDto = new UserNotificationDto
            {
                UserId = product.SellerId,
                Title = "Ürününüz Yönetim Tarafından Kaldırıldı",
                Message = $"'{product.Name}' isimli ürününüz şu gerekçeyle kaldırılmıştır: {dto.Reason}",
                Type = "System"
            };

            await _notificationService.CreateNotificationAsync(notificationDto);

            string subject = "Ürün Yayından Kaldırıldı";
           
            string mailBody = $@"
        <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
            <h2 style='color: #dc3545;'>Merhaba {seller.FirstName} {seller.LastName},</h2>
            <p>Mağazanızda yer alan <strong>{product.Name}</strong> isimli ürün, yapılan incelemeler sonucunda platform kurallarımıza uymadığı gerekçesiyle yayından kaldırılmıştır.</p>
            <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #dc3545; margin: 20px 0;'>
                <strong>Yönetici Notu / Silinme Gerekçesi:</strong><br>
                {dto.Reason}
            </div>
            <p>Bu işlemin bir hata olduğunu düşünüyorsanız destek ekibimizle iletişime geçebilirsiniz.</p>
            <br>
            <p>Saygılarımızla,<br><strong>Brandcorner Yönetim Ekibi</strong></p>
        </div>";
            try
            {
                await _emailService.SendEmailAsync(seller.Email, subject, mailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ürün silindi ama mail atılamadı: " + ex.Message);
            }


        }

        public async Task<List<AdminProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepo.GetAllAsQueryable(tracking:false).OrderByDescending(p=>p.CreatedDate)
                                                .Select(p=>new AdminProductDto
                                                {
                                                    Id =p.Id,
                                                    Name =p.Name,
                                                    SellerName = p.Seller.UserName,
                                                    Price =p.Price,
                                                    Stock =p.StockQuanity,
                                                    Images =p.ProductImages != null ? p.ProductImages.Select(x=>x.ImagePath).ToList():new List<string>(),   
                                                    Status = "Aktif",
                                                    DiscountedPrice = p.DiscountedPrice,
                                                    DiscountPercentage = p.DiscountPercentage,
                                                    DiscountEndDate = p.DiscountEndDate

                                                }).ToListAsync();
            return products;
        }

        public async Task<UserDetailDto> GetUserDetailsAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("Kullanıcı bulunamadı");

            return new UserDetailDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Balance = user.Balance,
                JoinDate = user.CreatedDate,
                Role = "Kullanıcı"
            };
        }

        public async Task<List<AdminProductDto>> GetUserProductsAsync(Guid userId)
        {
            var products = await _productRepo.Where(p=>p.SellerId == userId).Select(p => new AdminProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SellerName = "",
                Price = p.Price,
                Stock = p.StockQuanity,
                Status = "Aktif"
            })
            .ToListAsync();


            return products;
        }
    }
}
