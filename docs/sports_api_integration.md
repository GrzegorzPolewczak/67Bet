# Specyfikacja Integracji z API Wydarzeń Sportowych

## 1. Opis Funkcjonalności
System 67Bet integruje się z zewnętrznym dostawcą danych sportowych (**The Odds API**) w celu zapewnienia użytkownikom dostępu do aktualnych wydarzeń i kursów bukmacherskich. Integracja odbywa się w ramach mikroserwisu **Odds**, który odpowiada za pobieranie, mapowanie i udostępnianie tych danych pozostałym modułom systemu oraz frontendowi.

## 2. Architektura Integracji
Integracja została zaprojektowana zgodnie z zasadami **Clean Architecture**:
- **Domain:** Definiuje modele `ExternalEvent`, `ExternalMarket` i `ExternalOutcome`, które są niezależne od formatu zewnętrznego dostawcy.
- **Infrastructure:** Zawiera `TheOddsApiClient`, który implementuje specyficzną komunikację HTTP z API oraz repozytoria do składowania danych.
- **Application:** Zarządza logiką synchronizacji danych i udostępnia interfejsy dla warstwy API.
- **Api:** Udostępnia endpointy RESTowe dla frontendu i innych mikroserwisów.

## 3. Wybrany Dostawca: The Odds API
- **URL:** `https://api.the-odds-api.com/v4/sports`
- **Kluczowe dane:**
    - Lista sportów i lig.
    - Wydarzenia (nadchodzące i trwające).
    - Kursy (H2H, Total, Spread) od różnych bukmacherów.

## 4. Przepływ Danych (Sync Workflow)
1. Administrator lub systemowy harmonogram wywołuje endpoint `/api/external/sync`.
2. `OddsIntegrationService` pobiera dane z `TheOddsApiClient`.
3. Dane są mapowane z formatu JSON dostawcy na encje domenowe.
4. Dane są zapisywane/aktualizowane w bazie danych `OddsDbContext`.
5. Frontend pobiera dane poprzez `ExternalOddsController`.

## 5. Mapowanie Danych
| Dane API Zewnętrznego | Model 67Bet |
|-----------------------|-------------|
| `id`                  | `ExternalId` |
| `home_team` vs `away_team` | `Name` |
| `commence_time`       | `StartTime` |
| `price`               | `CurrentPrice` |
| `outcome name`        | `OutcomeName` |

## 6. Bezpieczeństwo i Wydajność
- Klucz API jest przechowywany w `appsettings.json` (lub zmiennych środowiskowych) i nigdy nie jest wystawiany na frontend.
- Dane są buforowane w bazie danych, aby zminimalizować liczbę zapytań do płatnego API.
- Usługa wspiera mechanizm `Optimistic Concurrency` przy aktualizacji kursów.
