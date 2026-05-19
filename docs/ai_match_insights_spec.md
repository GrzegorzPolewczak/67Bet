# Specyfikacja Funkcjonalności: AI Match Insights (Asystent Gemini)

## 1. Cel i Kontekst
Wprowadzenie inteligentnego asystenta opartego na modelu językowym **Google Gemini 1.5 Flash**, który dostarcza użytkownikom platformy 67Bet krótkie, merytoryczne i angażujące wskazówki (insights) dotyczące konkretnych wydarzeń sportowych.

## 2. Opis Funkcjonalny
1.  **Widok szczegółów meczu:** Użytkownik ma dostęp do przycisku "Wygeneruj analizę AI".
2.  **Zapytanie do modelu:** System przesyła do Gemini 1.5 Flash kontekst meczu (np. nazwy drużyn, dyscyplina).
3.  **Prezentacja:** Wygenerowana podpowiedź (np. "Drużyna A ma silną defensywę, spodziewaj się niskiego wyniku") wyświetla się w dedykowanym komponencie.

## 3. Mechanizm Bazy Danych (Cache)
Funkcjonalność wykorzystuje relacyjną bazę danych MySQL do przechowywania wygenerowanych analiz:
*   Przy żądaniu system najpierw sprawdza, czy analiza dla danego `EventId` istnieje w bazie.
*   Jeśli tak — serwuje ją z bazy (oszczędność kredytów API i szybkość).
*   Jeśli nie — pobiera z API Gemini, zapisuje w bazie i zwraca użytkownikowi.

## 4. Wymagania Techniczne
*   **Backend:** ASP.NET Core API (C#), Entity Framework Core.
*   **Frontend:** React (TypeScript).
*   **Model:** Google Gemini 1.5 Flash (API Key).
*   **Baza danych:** MySQL (nowa tabela `AiMatchInsights`).

---
*Status: Krok 1 zakończony (Specyfikacja).*
