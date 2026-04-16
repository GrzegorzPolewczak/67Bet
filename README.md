# 67Bet - System Zakładów Sportowych

## 📌 Opis projektu
**67Bet** to zaawansowany system do obsługi zakładów sportowych, realizowany w ramach projektu akademickiego. Celem projektu jest stworzenie skalowalnej aplikacji webowej z wykorzystaniem nowoczesnych wzorców projektowych, architektury wielowarstwowej oraz wsparcia narzędzi sztucznej inteligencji w procesie wytwórczym.

## 🛠 Stos technologiczny
* **Backend:** .NET 10 / ASP.NET Core Web API
* **Baza danych:** Microsoft SQL Server
* **ORM:** Entity Framework Core
* **Frontend:** React
* **Testy:** xUnit / Moq 

## 🏗 Architektura systemu
System został zaprojektowany w oparciu o architekturę warstwową, zapewniającą czysty podział odpowiedzialności:
1.  **Data Access Layer (DAL):** Odpowiada za strukturę danych (Modele) oraz bezpośrednią komunikację z bazą danych poprzez wzorzec Repository.
2.  **Business Logic Layer (BLL):** Zawiera logikę biznesową, serwisy oraz walidacje.
3.  **Web API:** Warstwa komunikacyjna udostępniająca zasoby poprzez kontrolery REST.
4.  **Frontend:** Interfejs użytkownika komunikujący się z API.

### Etap 2: Rozszerzenie, API i Interfejs (Planowane)
- [ ] Rozszerzenie specyfikacji o dodatkowe funkcjonalności.
- [ ] Aktualizacja dokumentacji UML (Diagramy klas i sekwencji dla BLL).
- [ ] Implementacja kontrolerów **Web API (REST)**.
- [ ] Implementacja Frontendu i integracja z API.
- [ ] Deployment systemu na serwer WWW.
