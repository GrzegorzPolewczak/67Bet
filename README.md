# 🎲 67Bet – AI-Driven Sports Betting Platform

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-TypeScript-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![ML.NET](https://img.shields.io/badge/ML.NET-Machine_Learning-5C2D91?style=for-the-badge&logo=microsoft&logoColor=white)

Zaawansowany system do obsługi zakładów sportowych, symulujący rzeczywiste środowisko platformy bukmacherskiej. System wykorzystuje architekturę hybrydową, integrując przetwarzanie w czasie rzeczywistym (Real-time), spójność transakcyjną oraz modele sztucznej inteligencji oparte na ML.NET do dynamicznego wyznaczania kursów. Całość backendu napisana jest w 100% w C#.

---

## 📖 Opis, Cel i Idea Projektu

### 💡 Idea Systemu
Główną ideą stojącą za platformą **67Bet** jest zdefiniowanie na nowo sposobu, w jaki użytkownicy wchodzą w interakcję z systemami bukmacherskimi. Zamiast tworzyć kolejny, statyczny klon istniejących rozwiązań, aplikacja wprowadza inteligentne, wysoce responsywne środowisko napędzane algorytmami uczenia maszynowego (ML.NET). System zaciera granicę między tradycyjnym hazardem a rozwiązaniami społecznościowymi, dając graczom nie tylko możliwość obstawiania gotowych zdarzeń, ale również swobodę kreowania własnych, unikalnych rynków (Custom Bets). Całość została zaprojektowana tak, aby symulować w 100% profesjonalne, komercyjne środowisko o wysokiej dostępności.

### 🎯 Cel Projektu
Głównym celem inżynierskim jest zaprojektowanie, zaimplementowanie i przetestowanie wysoce skalowalnej architektury webowej w ekosystemie **.NET 10**, która sprosta wyzwaniom narzucanym przez systemy czasu rzeczywistego (Real-time). Aplikacja musi gwarantować absolutną spójność danych finansowych (ACID), bezopóźnieniową dystrybucję zmieniających się kursów (SignalR) oraz odporność na problemy współbieżności (np. jednoczesne postawienie tysięcy zakładów na to samo zdarzenie).

---

## ✨ Kluczowe Funkcjonalności

- 🧠 **Native C# AI Oddsmaker:** Automatyczne szacowanie prawdopodobieństwa i generowanie kursów przy użyciu modeli uczenia maszynowego wytrenowanych i zaimplementowanych natywnie w **ML.NET**.
- ⚡ **Live Betting (Real-Time):** Natychmiastowe odświeżanie kursów u klientów przy użyciu WebSockets (SignalR) – bez konieczności przeładowywania strony.
- 🛠️ **Multisport & Custom Bets:** Obsługa klasycznych sportów (piłka nożna, MMA) dzięki polimorficznej bazie danych (JSONB) oraz unikalny moduł pozwalający użytkownikom zgłaszać własne propozycje zakładów do wyceny przez model predykcyjny.
- 🔐 **Transakcyjny Portfel (Wallet):** Zabezpieczenie przed wyścigami (Race Conditions) dzięki `Optimistic Concurrency` – pełna spójność operacji wpłat, wypłat i zamrażania środków.
- 🛡️ **Role-Based Access Control (RBAC):** Trzypoziomowy system uprawnień (Admin, Moderator, User) wspierany przez nowoczesne API ASP.NET Core Identity.

---

## 🏗️ Architektura i Stos Technologiczny

Projekt został zrealizowany w oparciu o zasady **Clean Architecture**. Zastosowanie jednolitego stosu technologicznego dla całego backendu eliminuje wąskie gardła komunikacyjne między mikroserwisami.

- **Frontend:** React (TypeScript), Tailwind CSS, Redux Toolkit, SignalR Client.
- **Backend (Core & API):** .NET 10 (ASP.NET Core Web API), Entity Framework Core.
- **AI & Data Ingestion:** **ML.NET** zintegrowane w ramach .NET Worker Services. Własne modele ładujące dane historyczne i korygujące kursy na żywo, działające jako usługi w tle.
- **Baza Danych & Cache:** PostgreSQL 16+ (jako główne źródło prawdy z natywnym wsparciem JSONB), Redis (do buforowania aktywnych rynków i sesji).

---

## 🗄️ Model Danych (High-Level)

Baza danych PostgreSQL została zaprojektowana z myślą o elastyczności. Zamiast dedykowanych tabel dla każdego sportu, wykorzystano kolumnę `metadata` typu `JSONB` w tabeli `Events`. Rozdzielono również warstwę analityczną od rynkowej: tabela `Outcomes` przechowuje zarówno czyste prawdopodobieństwo z modelu ML.NET (`probability`), jak i finalny kurs dla gracza (`current_price`). Umożliwia to łatwe audytowanie skuteczności sztucznej inteligencji.

---

## 🚀 Uruchomienie lokalne (Development)

### Wymagania:
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker & Docker Compose](https://www.docker.com/) (dla bazy danych i cache'u)

### Kroki instalacji:

1. **Sklonuj repozytorium:**
   ```bash
   git clone [https://github.com/TwojUsername/67Bet.git](https://github.com/TwojUsername/67Bet.git)
   cd 67Bet
