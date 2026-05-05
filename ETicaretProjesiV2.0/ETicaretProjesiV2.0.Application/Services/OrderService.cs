using AutoMapper;
using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Events;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Enums;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using RedLockNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly UserManager<AppUser> _userRepo;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Offer> _offerRepo;
        private readonly IGenericRepository<Basket> _basketRepo;
        private readonly IGenericRepository<BasketItem> _basketItemRepo;
        private readonly IConfiguration _config;
        private readonly IGenericRepository<WalletTransaction> _walletTransactionRepo;
        private readonly IDistributedLockFactory _lockFactory;
        private readonly IDistributedCache _cache;
        private readonly IBasketService _basketService;
        private readonly IPublishEndpoint _publishEndpoint;
        public OrderService(IGenericRepository<Order> orderRepo,IGenericRepository<Product> productRepo, UserManager<AppUser> userRepo
            ,IMapper mapper, IGenericRepository<Offer> offerRepo,IGenericRepository<Basket> basketRepo,IGenericRepository<BasketItem> basketItemRepo
            ,IConfiguration config,IGenericRepository<WalletTransaction> walletTransactionRepo,IDistributedCache cache,IDistributedLockFactory lockFactory,IBasketService basketService,
            IPublishEndpoint publishEndpoint)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _offerRepo = offerRepo;
            _basketRepo = basketRepo;
            _basketItemRepo = basketItemRepo;
            _config = config;
            _walletTransactionRepo = walletTransactionRepo;
            _cache = cache;
            _lockFactory = lockFactory;
            _basketService = basketService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task CancelOrderAsync(Guid orderId)
        {
            var order = await _orderRepo.Where(x => x.Id == orderId).Include(x => x.OrderItems).FirstOrDefaultAsync();

            if (order == null)
                throw new Exception("Sipariş Bulunamadı");

            if (order.Status == OrderStatus.Cancelled) throw new Exception("Bu sipariş zaten iptal edilmiş!");

            var buyer = await _userRepo.FindByIdAsync(order.AppUserId.ToString());
            if (buyer == null) throw new Exception("Alıcı bulunamadı!");

           
            List<WalletTransaction> transactions = new List<WalletTransaction>();

           
            buyer.Balance += order.TotalPrices;
            await _userRepo.UpdateAsync(buyer);

            transactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                AppUserId = buyer.Id,
                Amount = order.TotalPrices, 
                Description = $"Sipariş İptal İadesi (Sipariş No: {order.Id})",
                CreatedDate = DateTime.UtcNow,
                TransactionType = TransactionType.Deposit 
            });

            Dictionary<Guid, AppUser> sellersToUpdate = new Dictionary<Guid, AppUser>();
            decimal totalRefundedCommission = 0; 

            
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null) continue;

                var sellerId = product.SellerId;

                if (!sellersToUpdate.ContainsKey(sellerId))
                {
                    var seller = await _userRepo.FindByIdAsync(sellerId.ToString());
                    if (seller != null) sellersToUpdate.Add(sellerId, seller);
                }

                if (sellersToUpdate.ContainsKey(sellerId))
                {
                    decimal itemTotal = item.UnitPrice * item.Quanity;
                    decimal sellerRefund = itemTotal * 0.90m; 
                    decimal adminRefund = itemTotal * 0.10m;  

                    totalRefundedCommission += adminRefund;
                    sellersToUpdate[sellerId].Balance -= sellerRefund;

                    
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = sellerId,
                        Amount = -sellerRefund,
                        Description = $"İptal Edilen Satış (Sipariş No: {order.Id})",
                        CreatedDate = DateTime.UtcNow,
                        TransactionType = TransactionType.Withdrawal
                    });
                }
            }

            foreach (var seller in sellersToUpdate.Values)
            {
                await _userRepo.UpdateAsync(seller);
            }

           
            string adminUserId = _config["AdminSettings:AdminUserId"];
            if (adminUserId != null)
            {
                var adminUser = await _userRepo.FindByIdAsync(adminUserId);
                if (adminUser != null)
                {
                    adminUser.Balance -= totalRefundedCommission; 
                    await _userRepo.UpdateAsync(adminUser);

                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = adminUser.Id,
                        Amount = -totalRefundedCommission,
                        Description = $"İptal Edilen Sipariş Komisyon İadesi",
                        CreatedDate = DateTime.UtcNow,
                        TransactionType = TransactionType.Withdrawal
                    });
                }
            }

           
            order.Status = OrderStatus.Cancelled;
            _orderRepo.Update(order);

            if (transactions.Any())
            {
                foreach (var t in transactions)
                {
                    await _walletTransactionRepo.AddAsync(t);
                }
            }

            await _orderRepo.SaveAsync();
        }

        public async Task<bool> CheckIfUserPurchasedProductAsync(Guid userId, Guid productId)
        {
            return await _orderRepo.Where(o=>o.AppUserId == userId && o.Status == OrderStatus.Delivered)
                                    .AnyAsync(o=>o.OrderItems.Any(oi=>oi.ProductId == productId));
        }

        public async Task CreateOrderAsync(Guid buyerId, CreateOrderRequestDto dto, bool isCreditCardPayment = false)
        {
            string lockKey = $"lock:order:create:{buyerId}";

            using (var redLock = await _lockFactory.CreateLockAsync(lockKey, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(1)))
            {
                if (!redLock.IsAcquired)
                    throw new Exception("İşleminiz şu an başka bir cihazdan yürütülüyor.");

                var buyer = await _userRepo.FindByIdAsync(buyerId.ToString());
                if (buyer == null)
                    throw new Exception("Alıcı Bulunamadı");

                var basket = await _basketService.GetBasketAsync(buyerId);
                if (basket == null || !basket.Items.Any())
                    throw new Exception("Sepetiniz Boş");

                var order = _mapper.Map<Order>(dto);
                order.AppUserId = buyerId;
                order.OrderDate = DateTime.UtcNow;
                order.Status = OrderStatus.Confirmed;
                order.OrderItems = new List<OrderItem>();

                decimal totalPrice = 0;
                decimal totalAdminCommission = 0;
                List<WalletTransaction> transactions = new List<WalletTransaction>();
                Dictionary<Guid, AppUser> sellersToUpdate = new Dictionary<Guid, AppUser>();

                foreach (var basketItems in basket.Items)
                {
                    var product = await _productRepo.GetByIdAsync(basketItems.ProductId);
                    if (product == null)
                        throw new Exception($"Ürün bulunamadı: {basketItems.ProductId}");

                    if (product.StockQuanity < basketItems.Quantity)
                        throw new Exception($"{product.Name} için yetersiz stok!");

                    decimal actualPrice = (product.DiscountedPrice.HasValue && product.DiscountEndDate >= DateTime.UtcNow)
                          ? product.DiscountedPrice.Value
                          : product.Price;

                    var seller = await _userRepo.FindByIdAsync(product.SellerId.ToString());
                    if (seller == null)
                        throw new Exception($"Satıcı bulunamadı: {product.SellerId}");
                    if (seller.Id == buyer.Id)
                        throw new Exception("Kendi ürününüzü satın alamazsınız!");

                    decimal lineTotal = 0;

                    var acceptedOffer = await _offerRepo.Where(o => o.ProductId == basketItems.ProductId && o.BuyerId == buyerId
                                                                    && o.Status == OfferStatus.Accepted && o.CreatedDate >= DateTime.UtcNow.AddHours(-24))
                                                        .OrderByDescending(o => o.CreatedDate).FirstOrDefaultAsync();
                    if (acceptedOffer != null)
                    {
                        int offerQty = Math.Min(acceptedOffer.Quantity, basketItems.Quantity);
                        if (offerQty > 0)
                        {
                            lineTotal += acceptedOffer.OfferedPrice * offerQty;
                            order.OrderItems.Add(new OrderItem
                            {
                                Id = Guid.NewGuid(),
                                ProductId = basketItems.ProductId,
                                Quanity = offerQty,
                                UnitPrice = acceptedOffer.OfferedPrice
                            });
                        }

                        int normalQty = basketItems.Quantity - offerQty;
                        if (normalQty > 0)
                        {
                            lineTotal += actualPrice * normalQty;
                            order.OrderItems.Add(new OrderItem
                            {
                                Id = Guid.NewGuid(),
                                ProductId = basketItems.ProductId,
                                Quanity = normalQty,
                                UnitPrice = actualPrice
                            });
                        }

                        acceptedOffer.Status = OfferStatus.Completed;
                        _offerRepo.Update(acceptedOffer);
                    }
                    else
                    {
                        lineTotal += actualPrice * basketItems.Quantity;
                        order.OrderItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = basketItems.ProductId,
                            Quanity = basketItems.Quantity,
                            UnitPrice = actualPrice
                        });
                    }

                    product.StockQuanity -= basketItems.Quantity;
                    _productRepo.Update(product);

                    totalPrice += lineTotal;

                    decimal sellerEarnings = lineTotal * 0.90m;
                    decimal adminCut = lineTotal * 0.10m;
                    totalAdminCommission += adminCut;

                    if (!sellersToUpdate.ContainsKey(seller.Id))
                    {
                        sellersToUpdate.Add(seller.Id, seller);
                    }

                    sellersToUpdate[seller.Id].Balance += sellerEarnings;

                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = seller.Id,
                        Amount = sellerEarnings,
                        Description = $"Sipariş satışı: {product.Name} (Komisyon kesilmiş net kazanç)",
                        CreatedDate = DateTime.UtcNow,
                        TransactionType = TransactionType.OrderEarning,
                    });
                }
                if (!isCreditCardPayment)
                {
                    if (buyer.Balance < totalPrice)
                        throw new Exception($"Yetersiz Bakiye! Toplam: {totalPrice} ₺, Bakiyeniz: {buyer.Balance} ₺");

                    buyer.Balance -= totalPrice;

                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = buyer.Id,
                        Amount = -totalPrice,
                        Description = "Sepet Ödemesi Yapıldı",
                        CreatedDate = DateTime.UtcNow,
                        TransactionType = TransactionType.OrderPayment,
                    });
                    await _userRepo.UpdateAsync(buyer);
                }
                foreach (var seller in sellersToUpdate.Values)
                {
                    await _userRepo.UpdateAsync(seller);
                }

                string adminUserId = _config["AdminSettings:AdminUserId"];
                if (adminUserId == null)
                    throw new Exception("Appsetting.json Dosyası okunamadı");

                var adminUser = await _userRepo.FindByIdAsync(adminUserId);
                if (adminUser != null)
                {
                    adminUser.Balance += totalAdminCommission;
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = adminUser.Id,
                        Amount = totalAdminCommission,
                        Description = $"Hesabınıza {totalAdminCommission} ₺ komisyon aktarıldı",
                        CreatedDate = DateTime.UtcNow,
                        TransactionType = TransactionType.Comission,
                    });
                    await _userRepo.UpdateAsync(adminUser);
                }
                else
                {
                    throw new Exception($"Kritik Hata: Admin kullanıcısı bulunamadı! Aranan ID: '{adminUserId}'");
                }

                order.TotalPrices = totalPrice;
                await _orderRepo.AddAsync(order);

                await _basketService.ClearBasketAsync(buyerId);

                if (transactions.Any())
                {
                    foreach (var t in transactions)
                    {
                        await _walletTransactionRepo.AddAsync(t);
                    }
                }

                await _orderRepo.SaveAsync();
                await _cache.RemoveAsync("showcase_all_products");

                await _publishEndpoint.Publish(new OrderCreatedEvent(
                        OrderId :order.Id,
                        BuyerEmail: buyer.Email,
                        TotalPrice : totalPrice
                    ));
            }
        }

        public async Task<Order> GetOrderDetailsAsync(Guid orderId)
        {
           return await _orderRepo.Where(x => x.Id == orderId)
                .Include(x => x.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(o=> o.ProductImages)
                .FirstOrDefaultAsync();    
        }

        public async Task<IEnumerable<OrderListResponseDto>> GetSellerOrdersAsync(Guid sellerId)
        {
            var orders = await _orderRepo.Where(x=> x.OrderItems.Any(i=>i.Product.SellerId == sellerId)).Include(x=>x.OrderItems)
                .ThenInclude(i =>i.Product).
                ThenInclude(p=> p.ProductImages)
                .OrderByDescending(x=>x.OrderDate)
                .ToListAsync();

            foreach (var order in orders)
            {
                order.OrderItems = order.OrderItems.Where(i => i.Product.SellerId == sellerId).ToList();

                order.TotalPrices = order.OrderItems.Sum(i => i.UnitPrice * i.Quanity);

            }

            return _mapper.Map<IEnumerable<OrderListResponseDto>>(orders);
        }

        public async Task<IEnumerable<OrderListResponseDto>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _orderRepo.Where(x => x.AppUserId == userId)
                                 .Include(x => x.OrderItems)
                                 .ThenInclude(i => i.Product)
                                 .ThenInclude(p => p.ProductImages)
                                 .OrderByDescending(x => x.OrderDate)
                                 .ToListAsync();
            return _mapper.Map<IEnumerable<OrderListResponseDto>>(orders);
        }

        public async Task<bool> UpdateOrderStatus(Guid orderId, int newStatus)
        {
            var order = await _orderRepo.Where(x => x.Id == orderId).FirstOrDefaultAsync();
            if (order == null)
                throw new Exception("Sipariş Bulunamadı");

            order.Status = (OrderStatus)newStatus;
            _orderRepo.Update(order);



            await _orderRepo.SaveAsync();
            return true;
        }
    }
}
