# 🧩 67Bet Domain Model

Diagram przedstawia strukturę obiektową systemu, z uwzględnieniem podziału na mikroserwisy i kluczowe agregaty.

```mermaid
classDiagram
    direction LR

    %% Identity Aggregate
    class User {
        +Guid Id
        +string Username
        +string Email
        +Role UserRole
        +DateTime CreatedAt
    }

    class Role {
        <<enumeration>>
        Admin
        User
    }

    %% Wallet Aggregate
    class Wallet {
        +Guid Id
        +Guid UserId
        +decimal Balance
        +int Version
        +Deposit(amount)
        +Withdraw(amount)
        +Freeze(amount)
    }

    class Transaction {
        +Guid Id
        +TransactionType Type
        +decimal Amount
        +DateTime Timestamp
    }

    %% Betting Aggregate
    class Ticket {
        +Guid Id
        +Guid UserId
        +decimal TotalOdds
        +decimal Stake
        +TicketStatus Status
        +List~Bet~ Bets
        +CalculatePotentialWinning()
    }

    class Bet {
        +Guid Id
        +Guid OutcomeId
        +decimal FixedPrice
        +BetStatus Status
    }

    class Event {
        +Guid Id
        +string Name
        +DateTime StartTime
        +EventStatus Status
        +Dictionary~string, object~ Metadata
    }

    class Market {
        +Guid Id
        +string Name
        +bool IsActive
        +List~Outcome~ Outcomes
    }

    class Outcome {
        +Guid Id
        +string Name
        +decimal Probability
        +decimal CurrentPrice
        +bool? IsWinner
    }

    %% Custom Bet Aggregate
    class CustomBetRequest {
        +Guid Id
        +Guid UserId
        +string Description
        +decimal AiSuggestedOdds
        +decimal? AdminFinalOdds
        +RequestStatus Status
        +Accept(finalOdds)
        +Reject(reason)
    }

    %% Relationships
    User "1" -- "1" Wallet : owns
    Wallet "1" -- "*" Transaction : records
    User "1" -- "*" Ticket : places
    Ticket "1" -- "*" Bet : contains
    Bet "*" -- "1" Outcome : refers to
    Event "1" -- "*" Market : hosts
    Market "1" -- "*" Outcome : provides
    User "1" -- "*" CustomBetRequest : submits
```

### Kluczowe założenia modelu:

1.  **Wallet (Agregat Finansowy):** 
    *   Wykorzystuje `Version` do **Optimistic Concurrency**, co jest krytyczne przy szybkich zmianach salda.
    *   Metody `Freeze` i `Withdraw` są odseparowane, aby obsługiwać proces "zamrażania" środków pod aktywny kupon.

2.  **Ticket & Bet (Logika AKO):**
    *   `Ticket` to agregat nadrzędny. Sumuje kursy (`TotalOdds`) i zarządza statusem całego kuponu.
    *   `Bet` przechowuje `FixedPrice` (kurs z momentu postawienia), co chroni gracza przed zmianami kursów "na żywo" już po zawarciu zakładu.

3.  **Event (Polimorfizm przez Metadata):**
    *   Zamiast dziedziczenia klas dla każdego sportu, używamy `Dictionary/JSONB`, co pozwala na elastyczne dodawanie nowych dyscyplin (np. statystyki zawodników w MMA vs rzuty rożne w piłce).

4.  **CustomBetRequest:**
    *   Proces workflow: `Pending` (zgłoszony) -> `Reviewing` (AI wycenia) -> `Accepted/Rejected` (decyzja Admina).
