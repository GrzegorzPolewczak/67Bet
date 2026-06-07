# Specyfikacja Funkcjonalności: Admin Panel AI Enhancements

## 1. Cel i Kontekst
Rozbudowa panelu administratora o zaawansowane narzędzia do zarządzania i nadzoru nad modułami sztucznej inteligencji (Gemini) w systemie 67Bet. Celem jest zapewnienie pełnej kontroli nad generowanymi treściami oraz automatyzacja procesu wyceny zakładów niestandardowych.

---

## 2. Funkcjonalność: AI Insights Analytics (Zarządzanie Podpowiedziami)
Moduł ten pozwala administratorowi monitorować "jakość" pracy AI oraz zarządzać pamięcią podręczną analiz meczowych.

### 2.1. Opis Funkcjonalny
1.  **Dashboard Analiz:** Widok listy wszystkich wygenerowanych dotychczas podpowiedzi AI, posortowanych od najnowszych.
2.  **Podgląd Kontekstu (Debug):** Możliwość sprawdzenia, jakie dokładnie dane (JSON z wynikami i kursami) zostały wysłane do modelu Gemini w celu wygenerowania danej analizy.
3.  **Wymuszenie Regeneracji (Force Regenerate):** Przycisk pozwalający na natychmiastowe wysłanie nowego żądania do Gemini dla wybranego meczu (np. po aktualizacji kursów), co nadpisuje aktualną treść w bazie.
4.  **Usuwanie Analizy (Delete):** Opcja trwałego usunięcia podpowiedzi z bazy danych (czyszczenie cache), co skutkuje tym, że przy następnym wejściu użytkownika w ten mecz, analiza nie zostanie wyświetlona (lub zostanie wygenerowana na nowo).
5.  **Logowanie Generacji:** Rejestrowanie każdego zapytania do API Gemini w celu monitorowania zużycia tokenów i wykrywania błędów.

### 2.2. Zmiany w Bazie Danych
*   **Tabela `AiMatchInsights`:** Bez zmian (wykorzystanie istniejącej kolumny `Content` i `GeneratedAt`).
*   **Nowa Tabela `AiGenerationLogs`:**
    *   `Id` (Guid)
    *   `EventId` (string)
    *   `Timestamp` (DateTime)
    *   `Status` (Success/Error)
    *   `ErrorMessage` (string, nullable)

---

## 3. Funkcjonalność: Custom Bet AI Assistant (Moderacja Zakładów)
Usprawnienie procesu weryfikacji i wyceny propozycji graczy (`CustomBetRequest`) przy wsparciu modelu Gemini.

### 3.1. Opis Funkcjonalny
1.  **AI Pre-evaluation:** Każdy nowy wniosek o Custom Bet jest automatycznie analizowany przez AI w momencie wejścia administratora w szczegóły wniosku.
2.  **Rekomendacja AI:** System wyświetla administratorowi sugestię wygenerowaną przez model:
    *   **Sugerowany Kurs (Odds):** Propozycja wartości na bazie opisu gracza.
    *   **Ocena Ryzyka (Risk Assessment):** Skala High/Medium/Low.
    *   **Uzasadnienie (Reasoning):** Krótka notatka, dlaczego AI zaproponowało taką wycenę.
    *   **Tagowanie Kategorii:** Automatyczne przypisanie do sportu (np. "Football - Player Props").
3.  **One-Click Approve:** Możliwość zatwierdzenia wniosku z wartościami zaproponowanymi przez AI jednym kliknięciem.

### 3.2. Zmiany w Bazie Danych
*   **Tabela `CustomBetRequests`:** Dodanie kolumn:
    *   `AiSuggestedOdds` (decimal)
    *   `AiAnalysisNote` (text)
    *   `AiRiskLevel` (string)
    *   `AiCategory` (string)

---

## 4. Wymagania Techniczne
*   **Backend:** Rozbudowa kontrolerów `AiAssistantController` oraz `CustomBetController` o punkty końcowe dla admina (wymagana rola `Admin`).
*   **Logika:** Implementacja serwisu `AiModerationService` do obsługi promptów administracyjnych.
*   **Frontend:** Nowe komponenty w folderze `features/admin` (np. `AiInsightsManager.tsx`).

---
*Status: Krok 1 zakończony (Specyfikacja dla Admin Panel AI).*
