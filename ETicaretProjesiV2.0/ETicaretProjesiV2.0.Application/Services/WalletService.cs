using AutoMapper;
using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Services
{
    public class WalletService : IWalletService
    {

        private readonly IGenericRepository<WalletTransaction> _repository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        public WalletService(IGenericRepository<WalletTransaction> repository, UserManager<AppUser> userManager,IMapper mapper)
        {
            _repository = repository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<bool> AddBalanceAsync(string userId, AddBalanceRequestDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");
            if (dto.CardNumber.Length < 16 || dto.Amount < 0) throw new Exception("Geçersiz kredi kartı veya tutar");

            user.Balance += dto.Amount;
            await _userManager.UpdateAsync(user);

            var transaction = new WalletTransaction
            {
                AppUserId = Guid.Parse(userId),
                Amount = dto.Amount,
                TransactionType = TransactionType.Deposit,
                Description = $"Sanal Pos Üzerinden {dto.CardNumber.Substring(12,4)} sonu kart ile bakiye yüklendi."
            };
            await _repository.AddAsync(transaction);
            await _repository.SaveAsync();
            return true;
        }

        public async Task DepositAsync(Guid userId, decimal amount, string description)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("Kullanıcı bulunamadı");

            user.Balance += amount;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var transactions = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    AppUserId = userId,
                    Amount = amount,
                    TransactionType = TransactionType.Deposit,
                    Description = description,
                    CreatedDate = DateTime.UtcNow
                };
                await _repository.AddAsync(transactions);
                await _repository.SaveAsync();
            }
        }

        public async Task<decimal> GetBalanceAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user?.Balance ?? 0;
        }

        public async Task<IEnumerable<WalletTransactionResponseDto>> GetTransactionsHistoryAsync(Guid userId)
        {
           var transactions = await _repository.Where(x=> x.AppUserId == userId)
                                               .OrderByDescending(x=>x.CreatedDate)
                                               .ToListAsync();
           return _mapper.Map<IEnumerable<WalletTransactionResponseDto>>(transactions);
        }
    }
}
