
using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Enums;
using Iyzipay.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class PaymentManager : IPaymentService
    {
       
        private readonly IPaymentTransactionRepository _transactionRepository;
        private readonly IConfiguration _config;


        public PaymentManager(IPaymentTransactionRepository paymentTransactionRepository,IConfiguration config)
        {
            _transactionRepository = paymentTransactionRepository;
            _config = config;
        }

        public async Task<PaymentResultDto> ProcessPaymentAsync(PaymentRequestDto request, Guid userId)
        {
            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = request.Price,
                Type = request.PaymentType,
                Status = PaymentStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveAsync();

            try
            {
                Iyzipay.Options options = new Iyzipay.Options
                {
                    ApiKey = "sandbox-6wNyCGWYshrwdw63PM4uHmgJJeuWcPlI",
                    SecretKey = "sandbox-QnYfSiCJ76cQJCJ9YblBCE0IIEk8Si0G",
                    BaseUrl = "https://sandbox-api.iyzipay.com" 
                };

                CreatePaymentRequest paymentRequest = new CreatePaymentRequest
                {
                    Locale = Iyzipay.Model.Locale.TR.ToString(),
                    ConversationId = transaction.Id.ToString(),
                    Price = request.Price.ToString("F2", CultureInfo.InvariantCulture),
                    PaidPrice = request.Price.ToString("F2", CultureInfo.InvariantCulture),
                    Currency = Iyzipay.Model.Currency.TRY.ToString(),
                    Installment = 1,
                    BasketId = "B" + Guid.NewGuid().ToString().Substring(0, 6),
                    PaymentChannel = Iyzipay.Model.PaymentChannel.WEB.ToString(),
                    PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString(),
                };

                Iyzipay.Model.PaymentCard paymentCard = new Iyzipay.Model.PaymentCard
                {
                    CardHolderName = request.CardHolderName,
                    CardNumber = request.CardNumber,
                    ExpireMonth = request.ExpireMonth,
                    ExpireYear = request.ExpireYear,
                    Cvc = request.Cvc,
                    RegisterCard = 0,
                };
                paymentRequest.PaymentCard = paymentCard;

                Iyzipay.Model.Buyer buyer = new Iyzipay.Model.Buyer
                {
                    Id = userId.ToString(),
                    Name = request.BuyerName,
                    Surname = request.BuyerSurname,
                    GsmNumber = request.BuyerGsmNumber,
                    Email = request.BuyerEmail,
                    IdentityNumber = request.BuyerIdentityNumber,
                    LastLoginDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegistrationDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    RegistrationAddress = request.AddressDescription,
                    Ip = request.IpAddress ?? "85.34.78.112",
                    City = request.City,
                    Country = request.Country,
                    ZipCode = request.ZipCode,
                };
                paymentRequest.Buyer = buyer;


                Iyzipay.Model.Address address = new Iyzipay.Model.Address
                {
                    ContactName = $"{request.BuyerName} {request.BuyerSurname}",
                    City = request.City,
                    Country = request.Country,
                    Description = request.AddressDescription,
                    ZipCode = request.ZipCode,
                };
                paymentRequest.ShippingAddress= address;
                paymentRequest.BillingAddress = address;

                List<Iyzipay.Model.BasketItem> basketItems = new List<Iyzipay.Model.BasketItem>();
                Iyzipay.Model.BasketItem firstBasketItem = new Iyzipay.Model.BasketItem
                {
                    Id = "ITEM-1",
                    Name = request.PaymentType == PaymentType.WalletTopUp ? "Cüzdan Bakiyesi" : "Sepet Ürünleri",
                    Category1 = request.PaymentType == PaymentType.WalletTopUp ? "Cüzdan" : "Fiziksel Ürün",
                    ItemType = Iyzipay.Model.BasketItemType.VIRTUAL.ToString(),
                    Price = request.Price.ToString("F2", CultureInfo.InvariantCulture)
                };
                basketItems.Add(firstBasketItem);
                paymentRequest.BasketItems = basketItems;

                Iyzipay.Model.Payment payment = await Iyzipay.Model.Payment.Create(paymentRequest, options);
                if (payment.Status == "success")
                {
                    transaction.Status = PaymentStatus.Sucess;
                    transaction.BankTransactionId = payment.PaymentId;

                    _transactionRepository.Update(transaction);
                    await _transactionRepository.SaveAsync();

                    return new PaymentResultDto { IsSuccess = true, BankTransactionId = payment.PaymentId };
                }
                else
                {
                    transaction.Status = PaymentStatus.Failed;
                    transaction.ErrorMessage = payment.ErrorMessage;

                    _transactionRepository.Update(transaction);
                    await _transactionRepository.SaveAsync();

                    return new PaymentResultDto { IsSuccess = false, ErrorMessage = payment.ErrorMessage };
                }

            }
            catch (Exception ex)
            {
                transaction.Status = PaymentStatus.Failed;
                transaction.ErrorMessage = "Sistem Hatası: " + ex.Message;

                _transactionRepository.Update(transaction);
                await _transactionRepository.SaveAsync();

                return new PaymentResultDto { IsSuccess = false, ErrorMessage = "Sistemsel bir hata oluştu." };
            }
        }
    }
}
