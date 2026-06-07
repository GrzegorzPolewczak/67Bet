# ADR 0003: Ruletka z repozytorium in-memory i dwuetapowym rozliczeniem

## Status
Zaakceptowany

## Data
2026-06-07

## Kontekst
Projekt wymaga dodania gry kasynowej Roulette (europejska, 37 pól: 0–36). Gra musi obsługiwać wiele zakładów w jednej rundzie, losowanie kryptograficzne i integrację z serwisem portfela. Należało wybrać podejście do przechowywania danych oraz do przepływu płatności.

## Decyzja

### 1. Repozytorium in-memory (ConcurrentDictionary)
Zdecydowaliśmy się na repozytorium pamięciowe, identyczne z podejściem zastosowanym w grze Plinko. Ronda ruletki nie wymagają trwałości między restartami serwisu — są rozgrywkami jednorazowymi, a historia musi być dostępna tylko w trakcie sesji serwera.

### 2. Dwuetapowy przepływ: Play → Settle
Logika podzielona jest na dwa wywołania:
- `POST /api/Roulette/play` — pobiera całą stawkę z portfela, losuje wynik (0–36), rozlicza zakłady, zwraca wynik do frontendu.
- `POST /api/Roulette/{id}/settle` — kredytuje sumę wygranych do portfela i oznacza rundę jako rozliczoną (idempotentne: drugie wywołanie jest ignorowane).

Podział pozwala frontendowi wyświetlić animację koła między wywołaniami bez ryzyka niespójności salda.

### 3. Kryptograficzne losowanie
Wynik obrotu generowany jest przez `RandomNumberGenerator.GetInt32(0, 37)` z `System.Security.Cryptography`, a nie przez `System.Random`. Zapobiega to przewidywalności wyników.

### 4. Integracja z odpowiedzialną grą
`RouletteService` honoruje `IResponsibleGamblingService` (opcjonalna zależność), wywołując `ValidateStakeAsync` przed pobraniem stawki i `RecordActivityAsync` dla obu przepływów (Stake i Payout). Wzorzec identyczny z Plinko.

## Konsekwencje

**Pozytywne:**
- Brak zależności od bazy MySQL — gra działa natychmiast bez migracji.
- Prosta architektura spójna z istniejącą grą Plinko.
- Idempotentność rozliczenia eliminuje ryzyko podwójnej wypłaty przy ponownym żądaniu.

**Negatywne:**
- Historia rund jest tracona po restarcie serwera.
- Brak możliwości raportowania GGR z ruletki w panelu admina (dane nie trafiają do bazy).
