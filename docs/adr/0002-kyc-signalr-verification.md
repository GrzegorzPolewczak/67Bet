# ADR 0002: Weryfikacja KYC przy użyciu SignalR

## Status
Zaakceptowany

## Data
2026-06-07

## Kontekst
Proces KYC wymaga od użytkownika przesłania dokumentów tożsamości. Często wygodniej jest wykonać zdjęcie dokumentu telefonem niż przesyłać gotowy plik z komputera. Potrzebujemy mechanizmu, który połączy sesję na komputerze (Desktop) z sesją na telefonie (Mobile) i powiadomi komputer o zakończeniu procesu na telefonie w czasie rzeczywistym.

## Decyzja
Zdecydowaliśmy się na użycie biblioteki SignalR (WebSockets) do synchronizacji stanów między urządzeniami. 
*   Serwer generuje unikalny `sessionId`.
*   Desktop dołącza do grupy SignalR o nazwie `sessionId`.
*   Mobile wysyła żądanie weryfikacji do API, które po przetworzeniu (symulacja AI) wysyła sygnał przez SignalR do odpowiedniej grupy.

## Konsekwencje
*   **Pozytywne:** Natychmiastowa reakcja interfejsu desktopowego po zakończeniu weryfikacji na telefonie, lepsze User Experience.
*   **Negatywne:** Konieczność utrzymywania aktywnego połączenia WebSocket, dodatkowa logika obsługi grup na serwerze.
