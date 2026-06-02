# Specyfikacja Funkcjonalności: AI Match Insights (Asystent Gemini)

## 1. Cel i Kontekst
Wprowadzenie inteligentnego asystenta opartego na modelu językowym **Google Gemini 1.5 Flash**, który dostarcza użytkownikom platformy 67Bet krótkie, merytoryczne i angażujące wskazówki (insights) dotyczące konkretnych wydarzeń sportowych.
Głównym usprawnieniem jest eliminacja "halucynacji" modelu (zmyślania statystyk) poprzez wstrzykiwanie twardych danych historycznych i kursowych bezpośrednio do kontekstu zapytania (wzorzec Context Injection / RAG).

## 2. Opis Funkcjonalny
1.  **Widok szczegółów meczu:** Użytkownik ma dostęp do przycisku "Wygeneruj analizę AI".
2.  **Agregacja Kontekstu (Backend):** Zanim zapytanie trafi do AI, backend zbiera aktualne dane:
    *   **Wyniki i historia:** Pobranie ostatnich wyników danych drużyn z The Odds API (wykorzystanie endpointu `GET scores` z parametrem `daysFrom`).
    *   **Kursy:** Pobranie aktualnych kursów rynkowych na to spotkanie.
3.  **Zapytanie do modelu (Context Injection):** System przesyła do Gemini 1.5 Flash ścisły prompt zawierający zebrany kontekst (nazwy drużyn, dyscyplina, ostatnie wyniki, bieżące kursy) z kategorycznym nakazem opierania analizy *wyłącznie* na podanych danych.
4.  **Prezentacja:** Wygenerowana podpowiedź (np. "Drużyna A wygrała swój ostatni mecz, a bukmacherzy stawiają ją w roli wyraźnego faworyta z kursem 1.45. Spodziewany jest mecz jednostronny.") wyświetla się w dedykowanym komponencie.

## 3. Mechanizm Bazy Danych (Cache)
Funkcjonalność wykorzystuje relacyjną bazę danych MySQL do przechowywania wygenerowanych analiz:
*   Przy żądaniu system najpierw sprawdza, czy analiza dla danego `EventId` istnieje w bazie i czy jest aktualna.
*   Jeśli tak — serwuje ją z bazy (oszczędność kredytów API i szybkość).
*   Jeśli nie — agreguje kontekst z The Odds API, pobiera podsumowanie z API Gemini, zapisuje w bazie i zwraca użytkownikowi.

## 4. Wymagania Techniczne
*   **Backend:** ASP.NET Core API (C#), Entity Framework Core. Moduł agregacji kontekstu.
*   **Integracja API:** The Odds API (endpointy: kursy oraz `GET scores`).
*   **Frontend:** React (TypeScript).
*   **Model:** Google Gemini 1.5 Flash (API Key).
*   **Baza danych:** MySQL (tabela `AiMatchInsights`).

---
*Status: Zaktualizowano (Usprawnienie mechanizmu kontekstu i ograniczenie halucynacji AI).*
