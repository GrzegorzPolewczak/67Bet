using System;
using _67Bet.Shared.Kernel;

namespace _67Bet.Wallet.Domain.Enums
{
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Stake,
        Payout
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed
    }
}

namespace _67Bet.Wallet.Domain.Entities
{
    using _67Bet.Wallet.Domain.Enums;

    public class Wallet : BaseEntity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public decimal Balance { get; private set; }
        public string Currency { get; private set; } = null!;
        public int Version { get; private set; } // Optimistic Locking

        public Wallet(Guid userId, string currency = "PLN")
        {
            UserId = userId;
            Currency = currency;
            Balance = 0;
            Version = 1;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.");
            Balance += amount;
            Version++;
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.");
            if (Balance < amount) throw new InvalidOperationException("Insufficient funds.");
            Balance -= amount;
            Version++;
        }

        // EF Core
        private Wallet() { }
    }

    public class Transaction : BaseEntity
    {
        public Guid WalletId { get; private set; }
        public decimal Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public TransactionStatus Status { get; private set; }

        public Transaction(Guid walletId, decimal amount, TransactionType type)
        {
            WalletId = walletId;
            Amount = amount;
            Type = type;
            Status = TransactionStatus.Pending;
        }

        public void Complete() => Status = TransactionStatus.Completed;
        public void Fail() => Status = TransactionStatus.Failed;

        // EF Core
        private Transaction() { }
    }
}
