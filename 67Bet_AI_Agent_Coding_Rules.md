# 67Bet — AI Agent Coding Rules

Plik przeznaczony do wgrania jako instrukcja dla agenta AI, np. Cline w Visual Studio Code.  
Agent ma stosować te zasady przy każdym zadaniu w projekcie 67Bet.

---

# 1. Rola agenta AI

Agent AI pełni rolę asystenta programisty. Może analizować kod, proponować zmiany, generować implementację, pisać testy i aktualizować dokumentację.

Agent AI nie podejmuje samodzielnie decyzji architektonicznych, finansowych ani biznesowych. Przy większych zmianach najpierw przygotowuje plan, a dopiero po akceptacji tworzy kod.

Najważniejsza zasada:

```text
Najpierw analiza istniejącego kodu.
Potem plan.
Potem implementacja.
Na końcu testy i podsumowanie zmian.
```

---

# 2. Ogólne zasady pracy

Agent musi:

```text
1. Czytać istniejący kod przed zmianami.
2. Sprawdzać strukturę projektu.
3. Nie tworzyć duplikatów istniejących klas, metod i komponentów.
4. Pisać kod czytelny, prosty i testowalny.
5. Stosować nazwy, które jasno opisują przeznaczenie elementu.
6. Dzielić kod na małe klasy, metody i komponenty.
7. Nie mieszać odpowiedzialności.
8. Nie umieszczać logiki biznesowej w kontrolerach.
9. Nie umieszczać sekretów w kodzie.
10. Nie usuwać plików bez wyraźnej zgody programisty.
11. Po zmianach podawać listę zmodyfikowanych plików.
12. Po zmianach uruchomić build i testy, jeśli projekt na to pozwala.
```

---

# 3. Stos technologiczny projektu

Agent ma zakładać następujący stos:

```text
Backend:
- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- SignalR
- ML.NET

Frontend:
- React
- TypeScript
- Tailwind CSS
- Redux Toolkit
- SignalR Client

Baza danych:
-MySQL, zgodnie z aktualną konfiguracją projektu

Cache:
- Redis

Dokumentacja:
- Markdown
- PlantUML
```

Jeżeli w projekcie istnieje już konkretna technologia, agent ma trzymać się istniejącej konfiguracji zamiast wymyślać nową.

---

# 4. Architektura projektu

Projekt ma być tworzony zgodnie z zasadami Clean Architecture.

Typowa struktura mikroserwisu:

```text
ServiceName/
│
├── ServiceName.Api/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Extensions/
│   ├── Filters/
│   ├── Program.cs
│   └── appsettings.json
│
├── ServiceName.Application/
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Validators/
│   ├── Services/
│   └── Mappings/
│
├── ServiceName.Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── ValueObjects/
│   ├── Events/
│   ├── Exceptions/
│   └── Rules/
│
├── ServiceName.Infrastructure/
│   ├── Persistence/
│   ├── Repositories/
│   ├── Configurations/
│   ├── Migrations/
│   ├── Integrations/
│   └── Caching/
│
└── ServiceName.Tests/
    ├── Unit/
    ├── Integration/
    └── TestData/
```

Zasada zależności:

```text
Api może zależeć od Application.
Application może zależeć od Domain.
Infrastructure może implementować interfejsy z Application.
Domain nie może zależeć od Api, Infrastructure ani Application.
```

Zakazane:

```text
- kontroler wywołujący bezpośrednio DbContext,
- encja domenowa zależna od Entity Framework,
- logika biznesowa w Program.cs,
- logika biznesowa w kontrolerze,
- jeden ogromny serwis obsługujący wiele modułów,
- kopiowanie tej samej logiki do wielu miejsc.
```

---

# 5. Nazewnictwo w C#

## 5.1. Klasy

Nazwy klas zawsze zapisuj w PascalCase.

Dobrze:

```csharp
public class BetSlip
public class WalletTransaction
public class CreateBetSlipCommand
public class SettlementService
```

Źle:

```csharp
public class betslip
public class wallet_transaction
public class createbetslip
public class service1
```

Klasa powinna mieć nazwę opisującą konkretną odpowiedzialność.

Dobrze:

```csharp
BetSlipSettlementService
WalletTransactionRepository
CreateCustomBetRequestCommandHandler
```

Źle:

```csharp
Manager
Helper
Service
DataProcessor
Stuff
```

Nazwy typu `Manager`, `Helper`, `Processor`, `Handler` są dozwolone tylko wtedy, gdy mają jasny kontekst, np. `CreateBetSlipCommandHandler`.

---

## 5.2. Interfejsy

Interfejsy w C# zaczynają się od litery `I`.

Dobrze:

```csharp
public interface IWalletRepository
public interface IBetSlipService
public interface IOddsProvider
```

Źle:

```csharp
public interface WalletRepository
public interface BetSlipServiceInterface
```

---

## 5.3. Metody

Metody zapisuj w PascalCase. Nazwa metody ma opisywać czynność.

Dobrze:

```csharp
CreateBetSlipAsync()
CalculatePotentialWin()
SettleBetSlipAsync()
GetUserWalletAsync()
```

Źle:

```csharp
Do()
Process()
HandleData()
Run()
```

Metody asynchroniczne muszą mieć suffix `Async`.

Dobrze:

```csharp
Task<Wallet> GetWalletAsync(Guid userId)
Task CreateTransactionAsync(WalletTransaction transaction)
```

Źle:

```csharp
Task<Wallet> GetWallet(Guid userId)
```

Metoda powinna robić jedną rzecz. Jeżeli metoda ma więcej niż około 40-60 linii, agent powinien sprawdzić, czy da się ją podzielić.

---

## 5.4. Zmienne lokalne

Zmienne lokalne zapisuj w camelCase. Nazwa ma mówić, co przechowuje zmienna.

Dobrze:

```csharp
var userWallet = await walletRepository.GetByUserIdAsync(userId);
var totalOdds = betSelections.Aggregate(1m, (current, selection) => current * selection.OddsAtPlacement);
var potentialWin = stake * totalOdds;
```

Źle:

```csharp
var x = await walletRepository.GetByUserIdAsync(userId);
var data = GetData();
var temp = stake * odds;
```

Wyjątki są dopuszczalne tylko dla bardzo krótkiego kontekstu, np. `i` w pętli.

---

## 5.5. Pola prywatne

Prywatne pola klas zapisuj z podkreśleniem i camelCase.

Dobrze:

```csharp
private readonly IWalletRepository _walletRepository;
private readonly ILogger<WalletService> _logger;
```

Źle:

```csharp
private IWalletRepository WalletRepository;
private ILogger logger;
```

---

## 5.6. Stałe

Stałe zapisuj w PascalCase.

Dobrze:

```csharp
private const decimal MinimumStake = 1.00m;
private const int MaxSelectionsPerBetSlip = 20;
```

Źle:

```csharp
private const decimal minimumstake = 1.00m;
private const int MAX_SELECTIONS = 20;
```

---

## 5.7. Enumy

Enumy zapisuj w PascalCase. Wartości enumów również PascalCase.

Dobrze:

```csharp
public enum BetSlipStatus
{
    Pending,
    Won,
    Lost,
    Cancelled,
    Settled
}
```

Źle:

```csharp
public enum bet_status
{
    pending,
    won,
    lost
}
```

---

# 6. Nazewnictwo w TypeScript i React

## 6.1. Komponenty

Komponenty React zapisuj w PascalCase.

Dobrze:

```tsx
BetSlipCard.tsx
WalletBalance.tsx
EventDetailsPage.tsx
AdminUsersTable.tsx
```

Źle:

```tsx
betslipcard.tsx
wallet_balance.tsx
page.tsx
component.tsx
```

---

## 6.2. Hooki React

Własne hooki muszą zaczynać się od `use`.

Dobrze:

```tsx
useAuth()
useWallet()
useBetSlip()
useLiveOdds()
```

Źle:

```tsx
authHook()
wallet()
getBetSlip()
```

---

## 6.3. Zmienne i funkcje

Zmienne oraz funkcje w TypeScript zapisuj w camelCase.

Dobrze:

```tsx
const selectedOutcomes = [];
const totalOdds = calculateTotalOdds(selectedOutcomes);
const handleSubmitBetSlip = async () => {};
```

Źle:

```tsx
const Selected_Outcomes = [];
const total_odds = 0;
const submit = () => {};
```

---

## 6.4. Typy i interfejsy

Typy i interfejsy zapisuj w PascalCase.

Dobrze:

```ts
type BetSlipStatus = "Pending" | "Won" | "Lost";

interface WalletTransactionDto {
  id: string;
  amount: number;
  balanceAfter: number;
}
```

Źle:

```ts
interface wallettransactiondto {}
type bet_status = string;
```

---

# 7. Formatowanie kodu

Agent musi trzymać spójny styl formatowania.

C#:

```text
- używaj standardowego formatowania dotnet format,
- stosuj nullable reference types, jeżeli projekt ich używa,
- preferuj async/await dla operacji IO,
- używaj decimal dla pieniędzy i kursów,
- nie używaj double ani float do wartości finansowych,
- unikaj magic numbers,
- stosuj guard clauses dla walidacji.
```

TypeScript:

```text
- używaj ścisłego typowania,
- unikaj any,
- dziel komponenty na mniejsze,
- logikę API przenoś do osobnych plików,
- nie trzymaj całej logiki w komponencie widoku,
- nazwy propsów mają być opisowe.
```

---

# 8. Komentarze i dokumentacja w kodzie

Komentarze mają wyjaśniać powód, a nie przepisywać kod.

Dobrze:

```csharp
// Kurs musi zostać zapisany w momencie obstawienia,
// aby późniejsze zmiany kursów nie wpływały na aktywny kupon.
selection.OddsAtPlacement = outcome.CurrentPrice;
```

Źle:

```csharp
// Ustawia kurs
selection.OddsAtPlacement = outcome.CurrentPrice;
```

Komentarze wymagane są przy:

```text
- nietypowej logice biznesowej,
- operacjach finansowych,
- obsłudze współbieżności,
- integracji z ML.NET,
- ręcznych obejściach ograniczeń technicznych,
- kodzie trudnym do zrozumienia bez kontekstu domenowego.
```

Nie komentuj oczywistych rzeczy.

---

# 9. DTO, modele i mapowanie

Agent musi odróżniać encje domenowe od DTO.

Encja domenowa:

```text
- reprezentuje logikę systemu,
- może posiadać metody biznesowe,
- nie powinna być bezpośrednio zwracana z API.
```

DTO:

```text
- służy do komunikacji przez API,
- nie zawiera logiki biznesowej,
- może mieć pola dopasowane do widoku lub endpointu.
```

Dobrze:

```csharp
public class BetSlipDto
{
    public Guid Id { get; set; }
    public decimal Stake { get; set; }
    public decimal TotalOdds { get; set; }
    public decimal PotentialWin { get; set; }
    public string Status { get; set; } = string.Empty;
}
```

Zakazane:

```text
- zwracanie encji EF Core bezpośrednio z kontrolera,
- przyjmowanie encji domenowej jako body requestu,
- dodawanie logiki biznesowej do DTO.
```

---

# 10. Kontrolery API

Kontroler powinien być cienki. Ma przyjąć request, wywołać warstwę Application i zwrócić response.

Dobrze:

```csharp
[HttpPost]
public async Task<ActionResult<BetSlipDto>> CreateBetSlip(CreateBetSlipRequest request)
{
    var result = await _betSlipService.CreateBetSlipAsync(request);
    return CreatedAtAction(nameof(GetBetSlip), new { id = result.Id }, result);
}
```

Źle:

```csharp
[HttpPost]
public async Task<IActionResult> CreateBetSlip(CreateBetSlipRequest request)
{
    var wallet = await _dbContext.Wallets.FirstAsync(...);
    wallet.Balance -= request.Stake;
    await _dbContext.SaveChangesAsync();
    return Ok();
}
```

Kontroler nie może:

```text
- wykonywać obliczeń finansowych,
- rozliczać kuponów,
- bezpośrednio używać DbContext,
- zawierać dużych bloków logiki,
- tworzyć skomplikowanych zapytań LINQ,
- decydować o regułach domenowych.
```

---

# 11. Walidacja danych

Każdy request wejściowy musi być walidowany.

Sprawdzaj:

```text
- wymagane pola,
- długości tekstów,
- zakresy liczb,
- format email,
- minimalną i maksymalną stawkę,
- poprawność identyfikatorów,
- status rynku,
- status konta użytkownika,
- uprawnienia administratora.
```

Przykładowe zasady:

```text
- Stake musi być większe od 0.
- Stake nie może przekraczać salda użytkownika.
- Kurs musi być większy od 1.00.
- Nie można obstawić zamkniętego rynku.
- Nie można rozliczyć kuponu drugi raz.
- Zablokowany użytkownik nie może obstawiać.
```

---

# 12. Obsługa błędów

Agent ma stosować czytelną obsługę błędów.

Zalecane typy błędów domenowych:

```text
- NotFoundException
- ValidationException
- ForbiddenException
- ConflictException
- InsufficientFundsException
- MarketClosedException
- BetSlipAlreadySettledException
```

Nie wolno ukrywać błędów pustym catch.

Źle:

```csharp
try
{
    await service.DoSomethingAsync();
}
catch
{
}
```

Dobrze:

```csharp
try
{
    await service.SettleBetSlipAsync(betSlipId);
}
catch (BetSlipAlreadySettledException ex)
{
    _logger.LogWarning(ex, "Bet slip {BetSlipId} was already settled", betSlipId);
    throw;
}
```

---

# 13. Logowanie

Agent powinien dodawać logi dla ważnych operacji.

Logować należy:

```text
- logowanie użytkownika,
- błędne próby logowania,
- blokadę konta,
- utworzenie kuponu,
- odjęcie stawki z portfela,
- rozliczenie kuponu,
- zmianę kursu,
- decyzję administratora,
- błąd integracji z Redis, SignalR lub ML.NET.
```

Nie logować:

```text
- haseł,
- tokenów,
- API Key,
- danych wrażliwych,
- pełnych danych płatniczych,
- prywatnych sekretów z konfiguracji.
```

---

# 14. Bezpieczeństwo

Agent musi traktować bezpieczeństwo jako obowiązkowe.

Zasady:

```text
1. Hasła obsługuje ASP.NET Core Identity.
2. Hasła nigdy nie są zapisywane jawnie.
3. Tokeny i API Key nie mogą znajdować się w repozytorium.
4. Endpointy admina muszą wymagać roli Administrator.
5. Backend zawsze waliduje uprawnienia, nawet jeśli frontend ukrywa przyciski.
6. Użytkownik nie może samodzielnie zmienić swojego salda.
7. Użytkownik nie może zmienić kursu.
8. Użytkownik nie może rozliczyć kuponu.
9. Zablokowany użytkownik nie może obstawiać.
10. Każda operacja finansowa musi być audytowalna.
```

Przykład endpointu admina:

```csharp
[Authorize(Roles = "Admin")]
[HttpPatch("admin/users/{id}/block")]
public async Task<IActionResult> BlockUser(Guid id)
{
    await _adminUserService.BlockUserAsync(id);
    return NoContent();
}
```

---

# 15. Operacje finansowe

W systemie 67Bet wszystkie środki są wirtualne, ale kod musi być pisany tak, jakby operował na realnych pieniądzach.

Zasady obowiązkowe:

```text
- używaj decimal,
- każda zmiana salda w transakcji,
- każda zmiana salda tworzy WalletTransaction,
- zapisuj BalanceAfter,
- saldo nie może spaść poniżej zera,
- obsłuż optimistic concurrency,
- nie rozliczaj kuponu dwa razy,
- nie naliczaj wygranej bez zapisu transakcji,
- kurs użyty na kuponie zapisuj jako OddsAtPlacement.
```

Przykład nazw pól:

```text
Wallet.Balance
Wallet.Version
WalletTransaction.Amount
WalletTransaction.BalanceAfter
BetSelection.OddsAtPlacement
BetSlip.PotentialWin
```

Zakazane:

```text
- double dla pieniędzy,
- float dla pieniędzy,
- modyfikacja salda bez historii,
- modyfikacja salda po stronie frontendu,
- zaufanie do kursu przesłanego przez frontend,
- rozliczenie kuponu bez transakcji.
```

---

# 16. Kursy i zakłady

Zasady dla kursów:

```text
1. Frontend może wyświetlać kurs, ale backend decyduje o kursie użytym na kuponie.
2. Kurs z momentu obstawienia musi być zapisany w BetSelection.OddsAtPlacement.
3. Zmiana kursu po obstawieniu nie zmienia aktywnego kuponu.
4. Każda zmiana kursu powinna być możliwa do audytu.
5. Zamkniętego rynku nie można obstawiać.
6. Zawieszony rynek nie powinien przyjmować nowych zakładów.
```

Zasady dla kuponów:

```text
- kupon pojedynczy: jedna selekcja,
- kupon AKO: wiele selekcji,
- TotalOdds = iloczyn kursów selekcji,
- PotentialWin = Stake * TotalOdds,
- Pending oznacza kupon oczekujący,
- Won oznacza kupon wygrany,
- Lost oznacza kupon przegrany,
- Cancelled oznacza kupon anulowany,
- Settled oznacza kupon końcowo rozliczony.
```

---

# 17. AI i ML.NET

AI w systemie może wspierać wyznaczanie kursów, ale nie może samodzielnie podejmować decyzji finansowych.

Zasady:

```text
1. ML.NET może generować prawdopodobieństwo.
2. ML.NET może proponować kurs.
3. Administrator może zaakceptować albo zmienić kurs.
4. Wynik AI musi być zapisany.
5. Wersja modelu musi być zapisana.
6. AI nie może bezpośrednio zmieniać salda użytkownika.
7. AI nie może automatycznie rozliczać kuponów bez reguł systemowych.
8. AI nie może publikować Custom Bet bez akceptacji administratora.
```

Pola zalecane:

```text
AiPrediction.Id
AiPrediction.OutcomeId
AiPrediction.Probability
AiPrediction.SuggestedOdds
AiPrediction.ModelVersion
AiPrediction.InputDataHash
AiPrediction.CreatedAt
AiPrediction.ApprovedByAdminId
```

---

# 18. Entity Framework Core

Zasady dla EF Core:

```text
- konfiguracje encji trzymaj w osobnych klasach,
- używaj migracji,
- definiuj relacje jawnie,
- dodawaj indeksy dla często wyszukiwanych pól,
- używaj decimal precision dla pieniędzy i kursów,
- nie rób ciężkich zapytań w kontrolerach,
- unikaj N+1 query problem,
- stosuj AsNoTracking dla odczytów bez edycji.
```

Przykład konfiguracji decimal:

```csharp
builder.Property(x => x.Balance)
    .HasPrecision(18, 2);

builder.Property(x => x.CurrentPrice)
    .HasPrecision(10, 2);
```

Przykładowe indeksy:

```text
User.Email
Wallet.UserId
BetSlip.UserId
BetSlip.Status
SportEvent.StartTime
Market.EventId
Outcome.MarketId
WalletTransaction.WalletId
```

---

# 19. Redis i cache

Redis może być używany do:

```text
- cache kursów live,
- cache listy wydarzeń,
- przechowywania krótkotrwałych danych real-time,
- optymalizacji odczytów.
```

Zasady:

```text
- Redis nie jest źródłem prawdy dla salda użytkownika,
- Redis nie jest źródłem prawdy dla rozliczeń,
- dane finansowe muszą być zapisane w bazie relacyjnej,
- cache musi mieć TTL,
- po zmianie kursu cache musi zostać unieważniony albo zaktualizowany.
```

---

# 20. SignalR

SignalR służy do aktualizacji w czasie rzeczywistym.

Może wysyłać:

```text
- zmianę kursu,
- zamknięcie rynku,
- zmianę statusu wydarzenia,
- rozliczenie kuponu,
- powiadomienie użytkownika.
```

Nie wolno:

```text
- ufać danym z klienta SignalR bez walidacji backendu,
- rozliczać zakładów po stronie klienta,
- zmieniać salda na podstawie wiadomości z frontendu,
- traktować SignalR jako źródła prawdy.
```

---

# 21. Frontend — struktura

Zalecana struktura frontendu:

```text
frontend/67bet-client/
│
├── src/
│   ├── app/
│   ├── pages/
│   ├── components/
│   │   ├── common/
│   │   ├── betting/
│   │   ├── wallet/
│   │   ├── events/
│   │   └── admin/
│   │
│   ├── features/
│   │   ├── auth/
│   │   ├── betting/
│   │   ├── wallet/
│   │   ├── odds/
│   │   └── admin/
│   │
│   ├── hooks/
│   ├── services/
│   ├── types/
│   ├── utils/
│   └── styles/
```

Zasady:

```text
- komponenty widoku nie powinny zawierać ciężkiej logiki biznesowej,
- komunikacja z API w services/,
- typy w types/,
- własne hooki w hooks/,
- Redux Toolkit w features/,
- komponenty wielokrotnego użycia w components/common/.
```

---

# 22. Frontend — dobre praktyki

Agent musi:

```text
1. Używać TypeScript bez any, chyba że jest to uzasadnione.
2. Dzielić duże komponenty na mniejsze.
3. Używać opisowych nazw propsów.
4. Obsługiwać loading, error i empty state.
5. Nie ufać danym użytkownika tylko po stronie frontendu.
6. Nie przechowywać sekretów w kodzie frontendu.
7. Nie trzymać tokenów w miejscach niezgodnych z aktualną strategią bezpieczeństwa projektu.
8. Tworzyć formularze z walidacją.
9. Wyświetlać komunikaty błędów w czytelny sposób.
10. Nie duplikować logiki przeliczania kuponu bez synchronizacji z backendem.
```

Frontend może pokazywać potencjalną wygraną, ale backend musi obliczyć ją ponownie przy zatwierdzeniu kuponu.

---

# 23. Testy

Agent ma pisać testy dla istotnej logiki.

Testy obowiązkowe dla:

```text
- tworzenia konta,
- logowania,
- blokady użytkownika,
- wpłaty i wypłaty z portfela,
- braku środków,
- tworzenia kuponu,
- kursu zapisanego w momencie obstawienia,
- kuponu AKO,
- rozliczania kuponu wygranego,
- rozliczania kuponu przegranego,
- ochrony przed podwójnym rozliczeniem,
- endpointów administracyjnych,
- Custom Bet Request.
```

Nazwy testów powinny opisywać scenariusz.

Dobrze:

```csharp
[Fact]
public async Task CreateBetSlip_ShouldThrowException_WhenUserHasInsufficientFunds()
```

Źle:

```csharp
[Fact]
public async Task Test1()
```

Struktura testu:

```text
Arrange
Act
Assert
```

---

# 24. Git i jakość zmian

Agent nie powinien wykonywać chaotycznych zmian.

Każda zmiana powinna być mała i logiczna.

Po zakończeniu zadania agent powinien podać:

```text
- co zostało zmienione,
- jakie pliki zostały zmodyfikowane,
- jakie pliki zostały dodane,
- jakie testy zostały dodane,
- czy build przeszedł,
- czy testy przeszły,
- czy są znane ograniczenia.
```

Agent nie powinien:

```text
- formatować całego repozytorium bez potrzeby,
- zmieniać wielu niezwiązanych plików,
- mieszać refaktoryzacji z nową funkcją,
- zmieniać nazw publicznych endpointów bez powodu,
- usuwać historii migracji bez zgody.
```

---

# 25. Workflow dla każdego zadania

Agent ma stosować ten workflow:

```text
1. Zrozum zadanie.
2. Przeczytaj powiązane pliki.
3. Sprawdź, czy podobna funkcja już istnieje.
4. Określ moduł i warstwę projektu.
5. Przygotuj krótki plan.
6. Wypisz pliki do zmiany.
7. Wprowadź zmiany.
8. Dodaj lub zaktualizuj testy.
9. Uruchom build.
10. Uruchom testy.
11. Zaktualizuj dokumentację, jeśli trzeba.
12. Podsumuj wynik.
```

Przy dużych zmianach agent musi zatrzymać się po kroku 6 i czekać na akceptację programisty.

---

# 26. Hooks dla agenta

## 26.1. Pre-task hook

Przed rozpoczęciem pracy:

```text
- przeczytaj README.md,
- sprawdź strukturę katalogów,
- znajdź pliki związane z zadaniem,
- sprawdź istniejące testy,
- sprawdź aktualne konwencje nazewnictwa,
- nie generuj kodu od razu.
```

## 26.2. Pre-code hook

Przed edycją kodu:

```text
- napisz plan zmian,
- wskaż pliki do utworzenia,
- wskaż pliki do edycji,
- sprawdź zależności,
- sprawdź ryzyka,
- przy większej zmianie poczekaj na akceptację.
```

## 26.3. Post-code hook

Po edycji kodu:

```text
- uruchom formatowanie,
- uruchom dotnet build,
- uruchom dotnet test,
- uruchom testy frontendu, jeśli zmiana dotyczy frontendu,
- sprawdź błędy,
- popraw błędy,
- podsumuj zmiany.
```

## 26.4. Security hook

Dla zmian dotyczących auth, admina, portfela, kursów i kuponów:

```text
- sprawdź role,
- sprawdź autoryzację,
- sprawdź walidację,
- sprawdź transakcje,
- sprawdź historię operacji,
- sprawdź ochronę przed ujemnym saldem,
- sprawdź ochronę przed podwójnym rozliczeniem.
```

## 26.5. Database hook

Dla zmian w bazie:

```text
- dodaj konfigurację encji,
- dodaj migrację,
- sprawdź relacje,
- sprawdź indeksy,
- ustaw precision dla decimal,
- zaktualizuj diagram bazy danych.
```

---

# 27. Skills agenta

Agent może działać w trybach:

```text
Backend Developer:
- API, serwisy, komendy, zapytania, logika aplikacyjna.

Frontend Developer:
- React, TypeScript, komponenty, formularze, integracja z API.

Database Designer:
- tabele, relacje, migracje, indeksy, konfiguracje EF Core.

QA Engineer:
- testy jednostkowe, integracyjne, przypadki brzegowe.

Security Reviewer:
- role, autoryzacja, walidacja, operacje finansowe.

Software Architect:
- Clean Architecture, mikroserwisy, zależności, separacja odpowiedzialności.

Documentation Assistant:
- README, UML, API docs, workflow, opis modeli.
```

Agent powinien sam wybrać właściwy skill na podstawie zadania.

---

# 28. Definition of Done

Zadanie jest ukończone tylko wtedy, gdy:

```text
1. Kod jest zgodny z architekturą projektu.
2. Nazwy klas, metod i zmiennych są opisowe.
3. Nie ma duplikacji logiki.
4. Dane wejściowe są walidowane.
5. Endpointy są zabezpieczone rolami, jeśli trzeba.
6. Operacje finansowe są transakcyjne.
7. Dodano lub zaktualizowano testy.
8. Build przechodzi.
9. Testy przechodzą.
10. Dokumentacja została zaktualizowana, jeśli zmieniła się architektura, API albo model danych.
11. Agent podał listę zmienionych plików.
```

---

# 29. Antywzorce zakazane

Agent ma unikać następujących antywzorców:

```text
God Class:
- jedna klasa robi wszystko.

Fat Controller:
- kontroler zawiera logikę biznesową.

Anemic Service:
- serwis tylko przekazuje dane bez sensownej odpowiedzialności.

Magic Numbers:
- wartości liczbowe bez nazwy i kontekstu.

Copy-Paste Logic:
- ta sama logika skopiowana w wielu miejscach.

Primitive Obsession:
- używanie stringów wszędzie tam, gdzie lepszy jest enum lub value object.

Hidden Side Effects:
- metoda robi więcej, niż sugeruje jej nazwa.

Silent Catch:
- łapanie wyjątków bez logowania i reakcji.

Hardcoded Secrets:
- tokeny, hasła i API Key wpisane w kod.

Frontend Trust:
- zaufanie do danych wyliczonych po stronie klienta.
```

---

# 30. Checklista przed zakończeniem odpowiedzi agenta

Przed zakończeniem zadania agent musi sprawdzić:

```text
- Czy przeczytałem istniejący kod?
- Czy nie utworzyłem duplikatu?
- Czy nazwy są jasne?
- Czy kod jest w dobrej warstwie?
- Czy kontroler jest cienki?
- Czy walidacja jest po stronie backendu?
- Czy operacje finansowe są bezpieczne?
- Czy role admina są sprawdzone?
- Czy testy zostały dodane?
- Czy build został uruchomiony?
- Czy dokumentacja wymaga aktualizacji?
- Czy podałem listę zmian?
```

---

# 31. Krótka instrukcja dla agenta

```text
Jesteś agentem AI pracującym nad projektem 67Bet.
Twórz kod zgodnie z Clean Architecture.
Stosuj dobre praktyki C#, .NET, React i TypeScript.
Nazwy klas zapisuj PascalCase.
Nazwy zmiennych lokalnych zapisuj camelCase.
Nazwy metod zapisuj PascalCase.
Interfejsy C# zaczynaj od I.
Metody async kończ suffixem Async.
Nie mieszaj logiki biznesowej z kontrolerami.
Nie zapisuj sekretów w kodzie.
Nie ufaj danym z frontendu.
Dla pieniędzy używaj decimal.
Każda zmiana salda musi mieć transakcję i historię.
Nie rozliczaj kuponu dwa razy.
Zawsze czytaj istniejące pliki przed zmianami.
Przy większych zmianach najpierw pokaż plan.
Po zmianach uruchom build i testy.
Na końcu podsumuj zmienione pliki.
```
