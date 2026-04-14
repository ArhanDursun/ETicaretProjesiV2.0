using AutoMapper;
using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class OfferService : IOfferService
    {
        private readonly IGenericRepository<Offer> _offerRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IMapper _mapper;
        private readonly IBasketService _basketService;
        
        public OfferService(IGenericRepository<Offer> offerRepository, IGenericRepository<Product> productRepository, IMapper mapper,IBasketService basketService)
        {
            _offerRepository = offerRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _basketService = basketService;
        }
         
        public async Task<IEnumerable<OfferResponseDto>> GetOfferRecievedByMeAsync(Guid sellerId)
        {
            var offers = await _offerRepository.Where(o => o.SellerId == sellerId).Include(o => o.Product).Include(o => o.Buyer)
                .OrderByDescending(o => o.CreatedDate).ToListAsync();

            return offers.Select(o => new OfferResponseDto
            {
                Id = o.Id,
                ProductId = o.ProductId,
                ProductName = o.Product.Name,
                OfferedPrice = o.OfferedPrice,
                Status = o.Status,
                BuyerName = o.Buyer.UserName,
                CreatedTime = o.CreatedDate,
                CounterPrice = o.CounterPrice,
                Quantity= o.Quantity
            }).ToList();
        }

        public async Task<IEnumerable<OfferResponseDto>> GetOffersMadeByMeAsync(Guid buyerId)
        {
            var offers = await _offerRepository.Where(o => o.BuyerId == buyerId).Include(o => o.Product)
                .OrderByDescending(o => o.CreatedDate).ToListAsync();

            return offers.Select(o => new OfferResponseDto
            {
                Id = o.Id,
                ProductId = o.ProductId,
                ProductName = o.Product?.Name,
                OfferedPrice = o.OfferedPrice,
                Status = o.Status,
                BuyerName = o.Buyer?.UserName,
                CreatedTime = o.CreatedDate,
                CounterPrice = o.CounterPrice,
                Quantity = o.Quantity,
            }).ToList();


        }

        public async Task<bool> MakeCounterOfferAsync(Guid offerId, decimal counterPrice, string currentUserId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new Exception("Teklif bulunamadı");
            var product =await _productRepository.GetByIdAsync(offer.ProductId);
            if (product == null) throw new Exception("Teklif edilen ürün bulunamadı");

            if (counterPrice > product.Price)
                throw new Exception("Teklif ürünün normal fiyatından fazla olamaz");

            if(OfferStatus.Pending == offer.Status)
            {
                if(product.SellerId.ToString() != currentUserId)
                {
                    throw new Exception("Bu teklife sadece satıcı karşılık verebilir");
                }
                if (counterPrice <= offer.OfferedPrice)
                    throw new Exception("Karşı teklif alıcının verdiği fiyattan düşük ya da eşit olamaz");

                offer.Status = OfferStatus.Countered;
                offer.CounterPrice = counterPrice;
            }else if (offer.Status == OfferStatus.Countered)
            {
                if (offer.BuyerId.ToString() != currentUserId)
                    throw new Exception("Bu teklife sadece alıcı karşılık verebilir");
                if (counterPrice >= offer.CounterPrice)
                    throw new Exception("Teklifiniz satcının verdiği fiyattan yüksek ya da eşit olamaz");
                if(counterPrice <= offer.OfferedPrice)
                    throw new Exception("Yeni teklifiniz, kendi eski teklifinizden düşük olamaz!");

                offer.Status = OfferStatus.Pending;
                offer.OfferedPrice = counterPrice;
                offer.CounterPrice = null;
            }
            else
            {
                throw new Exception("Bu durumdaki bir teklife karşı teklif verilemez!");
            }

            _offerRepository.Update(offer);
            await _offerRepository.SaveAsync();

            return true;


        }

        public async Task MakeOfferAsync(Guid BuyerId, CreateOfferRequestDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null) throw new Exception("Ürün Bulunamadı");

            if (product.SellerId == BuyerId)
                throw new Exception("Kendi ürününüze teklif veremezsiniz");
            if (dto.OfferedPrice >= product.Price)
                throw new Exception("Teklif ettiğiniz tutar,ürünün normal fiyatından düşük olmalıdır.");
            var offer = new Offer
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                BuyerId = BuyerId,
                Quantity = dto.OfferQuantity,
                SellerId = product.SellerId,
                OfferedPrice = dto.OfferedPrice,
                Status = OfferStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };
            await _offerRepository.AddAsync(offer);
            await _offerRepository.SaveAsync();
        }

        public async Task<bool> RespondToOfferAsync(Guid offerId, bool isAccepted,string currentUserId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null) throw new Exception("Teklif Bulunamadı");

            var product = await _productRepository.GetByIdAsync(offer.ProductId);
            if (product == null) throw new Exception ("Ürün Bulunamadı");

            if(offer.Status == OfferStatus.Pending)
            {
                if (product.SellerId.ToString() != currentUserId)
                    throw new Exception("Bu teklifi yanıtlama yetkiniz yok (Sadece satıcı yanıtlayabilir)");


            }else if (offer.Status == OfferStatus.Countered)
            {
                if(offer.BuyerId.ToString()!= currentUserId) 
                    throw new Exception("Bu karşı teklifi yanıtlama yetkiniz yok (Sadece alıcı yanıtlayabilir)!");
            }
            else
            {
                throw new Exception("Bu teklif zaten daha önce yanıtlanmış!");
            }

            if(offer.Status == OfferStatus.Countered && isAccepted)
            {
                offer.OfferedPrice = offer.CounterPrice.Value;
            }
            offer.Status = isAccepted ? OfferStatus.Accepted : OfferStatus.Rejected;
            _offerRepository.Update(offer);
            await _offerRepository.SaveAsync();

            if (isAccepted)
            {
                var basketDto = new AddItemToBasketDto
                {
                    ProductId = offer.ProductId,
                    Quantity = offer.Quantity,
                    UnitPrice = offer.OfferedPrice,
                };

                await _basketService.AddItemToBasketAsync(offer.BuyerId, basketDto);
            }

            return true;
        }
    }
}
