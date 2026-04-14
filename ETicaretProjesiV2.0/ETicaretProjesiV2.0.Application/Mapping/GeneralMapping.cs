using AutoMapper;
using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Mapping
{
    public class GeneralMapping : Profile
    {
        public GeneralMapping() {
            CreateMap<Product, ProductListResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<CreateProductRequestDto, Product>();

            CreateMap<Category, CategoryResponseDto>();

            CreateMap<AppUser, AuthResponseDto>();
            
            CreateMap<Order,OrderListResponseDto>()
                .ForMember(dest => dest.TotalPrice,opt => opt.MapFrom(src=> src.TotalPrices))
                .ForMember(dest => dest.SellerEarning,opt => opt.MapFrom(src=>src.TotalPrices*0.90m));
            CreateMap<OrderItem,OrderItemDto>().ReverseMap();
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                    src.ProductImages != null
                    ? src.ProductImages.Select(p => p.ImagePath).ToList()
                    : new List<string>()));
            CreateMap<CreateOrderRequestDto, Order>();
            CreateMap<OrderItemDto, OrderItem>();
            CreateMap<WalletTransaction, WalletTransactionResponseDto>()
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.TransactionType));
        }
    }
}
