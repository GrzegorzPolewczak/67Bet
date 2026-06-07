# Specyfikacja - Weryfikacja KYC (Know Your Customer)

## 1. Opis Funkcjonalności
Moduł weryfikacji KYC to uproszczony proces potwierdzania tożsamości użytkownika. Rozwiązanie ma na celu integrację widoku na urządzeniu stacjonarnym (Desktop) z urządzeniem mobilnym (Mobile) w czasie rzeczywistym.

## 2. Architektura i Technologie
*   **Backend:** C# .NET Core Web API (Identity Service)
*   **Komunikacja w czasie rzeczywistym:** SignalR (WebSockets)
*   **Frontend:** React / TypeScript
*   **Wykorzystywane biblioteki:** `@microsoft/signalr`, `qrcode.react`

## 3. Przepływ procesu (User Flow)
1.  Użytkownik otwiera na urządzeniu stacjonarnym komponent `DesktopVerification`.
2.  Aplikacja generuje unikalną sesję (`sessionId`) z serwera poprzez wywołanie `GET /api/session`.
3.  Aplikacja na Desktopie nawiązuje połączenie z `VerificationHub` w SignalR i dołącza do grupy o nazwie odpowiadającej `sessionId`.
4.  Na ekranie wyświetlany jest kod QR, który zawiera adres URL prowadzący do widoku na urządzeniu mobilnym: `/mobile/{sessionId}`.
5.  Użytkownik skanuje kod QR telefonem, otwierając komponent `MobileVerification`.
6.  Na telefonie użytkownik widzi dwa pola do przesyłania plików:
    *   Skan dowodu tożsamości (z atrybutem `capture="environment"` otwierającym tylny aparat).
    *   Selfie (z atrybutem `capture="user"` otwierającym przedni aparat).
7.  Użytkownik klika "Verify", co powoduje wysłanie plików żądaniem `POST` typu `multipart/form-data` pod endpoint `/api/verify/{sessionId}`.
8.  Serwer przyjmuje żądanie, zapisuje w bazie status sesji i symuluje proces weryfikacji AI poprzez 3-sekundowe opóźnienie (`Task.Delay`).
9.  Po zakończeniu weryfikacji, serwer za pomocą interfejsu `IHubContext` wysyła zdarzenie `VerificationCompleted` do klientów w grupie `{sessionId}`.
10. Aplikacja na Desktopie odbiera zdarzenie, ukrywa kod QR i wyświetla komunikat o udanej weryfikacji (zielony tekst). Na telefonie pojawia się informacja o pomyślnym wysłaniu.

## 4. Ograniczenia / Założenia Uproszczone
*   Pliki obrazów przesłane przez użytkownika nie są w tej wersji persystowane długoterminowo do storage'u (skupienie na strumieniu i interakcji SignalR).
*   Proces weryfikacji przez AI jest wyłącznie symulowany w kodzie.
*   Zapis do bazy danych sprowadza się do przechowania podstawowej informacji o wygenerowanej sesji i aktualizacji jej statusu.
