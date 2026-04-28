using System;
using System.Threading.Tasks;
using _67Bet.Wallet.Domain.Entities;

namespace _67Bet.Wallet.Application.Interfaces;

/*
 * Interfejs IWalletService zarządza operacjami na portfelu użytkownika.
 * Definiuje metody do sprawdzania salda, wpłat, wypłat oraz obsługi płatności za zakłady i wypłat wygranych.
 */
public interface IWalletService
{
    Task<decimal> GetBalanceAsync(Guid userId);
    Task DepositAsync(Guid userId, decimal amount);
    Task WithdrawAsync(Guid userId, decimal amount);
    Task<bool> ProcessStakeAsync(Guid userId, decimal amount);
    Task ProcessPayoutAsync(Guid userId, decimal amount);
    Task<_67Bet.Wallet.Domain.Entities.Wallet?> GetWalletByUserIdAsync(Guid userId);
}
