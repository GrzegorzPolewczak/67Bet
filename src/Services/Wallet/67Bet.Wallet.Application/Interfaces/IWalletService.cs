using System;
using System.Threading.Tasks;
using _67Bet.Wallet.Domain.Entities;

namespace _67Bet.Wallet.Application.Interfaces;

/*
 * Interfejs IWalletService zarzÄ…dza operacjami na portfelu uĹĽytkownika.
 * Definiuje metody do sprawdzania salda, wpĹ‚at, wypĹ‚at oraz obsĹ‚ugi pĹ‚atnoĹ›ci za zakĹ‚ady i wypĹ‚at wygranych.
 */
public interface IWalletService
{
    Task<decimal> GetBalanceAsync(Guid userId);
    Task<decimal> GetFreebetBalanceAsync(Guid userId);
    Task DepositAsync(Guid userId, decimal amount);
    Task DepositFreebetAsync(Guid userId, decimal amount);
    Task WithdrawAsync(Guid userId, decimal amount);
    Task<bool> ProcessStakeAsync(Guid userId, decimal amount);
    Task ProcessPayoutAsync(Guid userId, decimal amount);
    Task<_67Bet.Wallet.Domain.Entities.Wallet?> GetWalletByUserIdAsync(Guid userId);
}
