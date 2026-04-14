using ETicaretProjesiV2._0.Application.DTOs;
using ETicaretProjesiV2._0.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Interfaces
{
    public interface IWalletService
    {
        Task<decimal> GetBalanceAsync(Guid userId);
        Task DepositAsync(Guid userId,decimal amount,string description);
        Task<IEnumerable<WalletTransactionResponseDto>> GetTransactionsHistoryAsync(Guid userId);
        Task<bool> AddBalanceAsync(string userId, AddBalanceRequestDto dto);
    }
}
