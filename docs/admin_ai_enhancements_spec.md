# Specyfikacja Funkcjonalności: Admin Panel AI Enhancements

## 1. Cel i Kontekst
Rozbudowa panelu administratora o zaawansowane narzędzia do zarządzania i nadzoru nad modułami sztucznej inteligencji (Gemini) w systemie 67Bet. Celem jest zapewnienie pełnej kontroli nad generowanymi treściami oraz monitorowanie stabilności połączenia z API AI.

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
*   **Tabela `AiMatchInsights`:** Wykorzystanie istniejącej kolumny `Content` i `GeneratedAt` (typ EventId zmieniony na string dla kompatybilności).
*   **Nowa Tabela `AiGenerationLogs`:**
    *   `Id` (Guid)
    *   `EventId` (string)
    *   `Timestamp` (DateTime)
    *   `Status` (Success/Error)
    *   `ErrorMessage` (string, nullable)

---

## 3. Wymagania Techniczne
*   **Backend:** Rozbudowa kontrolera `AiAssistantController` o punkty końcowe dla admina (wymagana rola `Admin`).
*   **Frontend:** Nowe komponenty w folderze `features/admin` (Dashboard z zakładkami).

---
*Status: Zaktualizowano (Usunięto moduł Custom Bet AI, zachowano AI Insights).*
