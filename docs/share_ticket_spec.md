# Specyfikacja funkcjonalności: udostępnianie kuponów

## 1. Cel funkcjonalności

Funkcjonalność udostępniania kuponów pozwala użytkownikowi wygenerować publiczny link do postawionego kuponu i przekazać go innej osobie. Osoba otwierająca link może zobaczyć szczegóły kuponu bez logowania: status kuponu, listę selekcji, obstawione typy, kursy, stawkę, potencjalną wygraną oraz wynik zakończonego wydarzenia.

Funkcja ma też umożliwić skopiowanie aktywnego kuponu do własnego betslipa, jeżeli wszystkie zdarzenia z kuponu są nadal dostępne do obstawienia.

## 2. Zakres funkcjonalny

### 2.1. Udostępnienie kuponu z historii

Użytkownik wchodzi do widoku historii zakładów i przy wybranym kuponie klika przycisk udostępniania. Frontend tworzy link w formacie:

```text
/share-ticket/{ticketId}
```

Następnie link jest kopiowany do schowka przeglądarki.

### 2.2. Publiczny podgląd kuponu

Po wejściu w link publiczny frontend pobiera dane kuponu z Betting API przez endpoint:

```http
GET /api/tickets/share/{id}
```

Endpoint jest oznaczony jako anonimowy, więc nie wymaga tokena JWT.

### 2.3. Dane pokazywane w udostępnionym kuponie

Widok udostępnionego kuponu pokazuje:

- identyfikator kuponu,
- status kuponu: `Pending`, `Won`, `Lost`, `Cancelled`,
- status opisowy w UI: `W TRAKCIE`, `WIN`, `LOST`, `CANCELLED`,
- stawkę,
- kurs łączny,
- potencjalną wygraną,
- listę selekcji,
- nazwę wydarzenia,
- rynek, np. `Winner`,
- obstawiony wybór, np. `Team A`,
- kurs z chwili obstawienia,
- czas rozpoczęcia wydarzenia,
- status pojedynczej selekcji,
- nazwę zwycięskiego wyniku, jeżeli wydarzenie zostało rozliczone.

### 2.4. Kopiowanie udostępnionego kuponu do betslipa

Użytkownik może skopiować selekcje z udostępnionego kuponu do swojego betslipa tylko wtedy, gdy:

- kupon ma status `Pending`,
- wszystkie zdarzenia z kuponu mają `StartTime` większy niż aktualny czas.

Po kliknięciu przycisku `Copy to my Betslip` frontend dodaje każdą selekcję do lokalnego stanu betslipa.

## 3. Aktorzy

- **Właściciel kuponu** — zalogowany użytkownik, który postawił kupon i generuje link z historii.
- **Odbiorca linku** — dowolny użytkownik, także niezalogowany, który otwiera publiczny link.
- **Betting API** — mikroserwis odpowiedzialny za odczyt kuponu i zwrócenie danych w formie DTO.
- **Frontend** — aplikacja React odpowiedzialna za kopiowanie linku, wyświetlenie strony udostępnionego kuponu i dodanie selekcji do betslipa.

## 4. Wymagania funkcjonalne

| ID | Wymaganie |
|---|---|
| SH-01 | System umożliwia skopiowanie linku do kuponu z poziomu historii betów. |
| SH-02 | Link publiczny zawiera identyfikator kuponu. |
| SH-03 | Otworzenie linku publicznego nie wymaga zalogowania. |
| SH-04 | System zwraca dane kuponu razem z listą selekcji. |
| SH-05 | System pokazuje, na co użytkownik postawił w każdej selekcji. |
| SH-06 | System pokazuje status kuponu i status każdej selekcji. |
| SH-07 | Jeżeli selekcja jest rozliczona, system pokazuje zwycięski wynik wydarzenia. |
| SH-08 | Jeżeli kupon nie istnieje, API zwraca `404 NotFound`, a frontend pokazuje komunikat błędu. |
| SH-09 | Aktywny kupon można skopiować do własnego betslipa, jeżeli wydarzenia jeszcze nie wystartowały. |

## 5. Wymagania niefunkcjonalne

- Publiczny endpoint nie zwraca danych wrażliwych właściciela kuponu, takich jak email, login, saldo ani dane profilu.
- Link powinien być stabilny i działać po odświeżeniu strony.
- Dane w publicznym widoku powinny pochodzić z backendu, a nie z lokalnego stanu użytkownika.
- Dane selekcji powinny być utrwalane na kuponie w momencie obstawienia, aby późniejsza zmiana kursu nie zmieniała historii kuponu.

## 6. Warstwa domenowa

Funkcjonalność opiera się na istniejącym agregacie `Ticket` oraz encji `Bet`.

### Ticket

`Ticket` reprezentuje cały kupon. Przechowuje:

- `UserId`,
- `Stake`,
- `TotalOdds`,
- `PotentialWinning`,
- `Status`,
- listę `Bets`.

### Bet

`Bet` reprezentuje pojedynczą selekcję kuponu. Dla udostępniania ważne są pola:

- `OutcomeId`,
- `OutcomeName`,
- `MarketName`,
- `EventName`,
- `StartTime`,
- `FixedPrice`,
- `Status`,
- `WinningOutcomeName`.

Dzięki tym polom udostępniony kupon może być poprawnie wyświetlony nawet wtedy, gdy kurs lub nazwa wydarzenia zmieniły się później w źródle danych.

## 7. Warstwa dostępu do danych

Do odczytu udostępnionego kuponu używany jest interfejs:

```csharp
ITicketRepository.GetByIdAsync(Guid id)
```

Implementacja `TicketRepository.GetByIdAsync` powinna ładować kupon razem z kolekcją `Bets` przez `Include(t => t.Bets)`. Bez tego publiczny widok kuponu nie otrzymałby selekcji.

Do historii użytkownika używany jest:

```csharp
ITicketRepository.GetByUserIdAsync(Guid userId)
```

Ta metoda również powinna ładować kolekcję `Bets`.

## 8. Warstwa logiki biznesowej

Za logikę aplikacyjną odpowiada `IBettingService` i `BettingService`.

Metoda wykorzystywana przez publiczny link:

```csharp
Task<Ticket?> GetTicketByIdAsync(Guid ticketId)
```

Metoda pobiera kupon z repozytorium i zwraca go do kontrolera. W obecnym zakresie funkcjonalności nie jest wymagana dodatkowa autoryzacja, bo link publiczny ma pokazywać ograniczony zestaw danych bez informacji o użytkowniku.

## 9. Kontroler API

Endpoint publiczny znajduje się w `TicketsController`:

```http
GET /api/tickets/share/{id}
```

Cechy endpointu:

- posiada `[AllowAnonymous]`,
- wywołuje `IBettingService.GetTicketByIdAsync(id)`,
- zwraca `404 NotFound`, gdy kupon nie istnieje,
- zwraca `TicketDto`, gdy kupon istnieje.

## 10. Frontend

Frontend obsługuje funkcję w dwóch miejscach:

### BetHistoryPage

- wyświetla historię kuponów,
- posiada przycisk udostępniania,
- generuje URL `/share-ticket/{ticketId}`,
- kopiuje link do schowka.

### SharedTicketPage

- odczytuje `ticketId` z adresu,
- pobiera dane z `/tickets/share/{id}`,
- pokazuje status kuponu i selekcji,
- pokazuje obstawiony typ i zwycięski wynik wydarzenia,
- pozwala skopiować aktywny kupon do betslipa.

## 11. Obsługa błędów

| Sytuacja | Zachowanie |
|---|---|
| Kupon istnieje | API zwraca `200 OK` i `TicketDto`. |
| Kupon nie istnieje | API zwraca `404 NotFound`. |
| Błąd sieci/API | Frontend pokazuje komunikat, że nie udało się załadować kuponu. |
| Kupon zakończony | Frontend nie pozwala skopiować go do betslipa. |
| Wydarzenie już wystartowało | Frontend blokuje kopiowanie kuponu do betslipa. |

## 12. Kryteria akceptacji

- Użytkownik może skopiować link do kuponu z historii.
- Link otwiera publiczny widok kuponu.
- Publiczny widok działa po odświeżeniu strony.
- Niezalogowany użytkownik może zobaczyć udostępniony kupon.
- Publiczny widok pokazuje status `WIN`, `LOST` albo `W TRAKCIE`.
- Publiczny widok pokazuje, na co postawiono.
- Po rozliczeniu wydarzenia publiczny widok pokazuje zwycięzcę.
- Publiczny endpoint nie zwraca danych właściciela kuponu poza samymi danymi kuponu.
