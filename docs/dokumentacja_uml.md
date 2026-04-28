# Punkt 4 — Dokumentacja projektowa I wersji systemu UML

## Cel dokumentacji

Celem tego punktu jest przygotowanie dokumentacji projektowej pierwszej wersji systemu 67Bet w oparciu o diagramy UML wykonane w narzędziu PlantUML. Wymaganie minimalne obejmuje przygotowanie przynajmniej diagramu klas, dlatego do projektu dodano plik:

```text
docs/class_diagram.puml
```

Diagram klas przedstawia główne klasy domenowe, klasy pomocnicze, interfejsy repozytoriów, implementacje repozytoriów, konteksty bazy danych oraz planowaną warstwę logiki biznesowej wynikającą z założeń projektu.

## Ważne założenie interpretacyjne

Diagram nie jest wyłącznie automatycznym odwzorowaniem aktualnego kodu. Jest to dokumentacja projektowa pierwszej wersji systemu, dlatego zawiera dwa typy elementów:

- `<<implemented>>` — element istnieje już w kodzie projektu,
- `<<planned>>` — element wynika z założeń projektu, ale może być dopiero planowany do implementacji w kolejnych etapach.

Dzięki temu diagram może być używany jako bezpieczna dokumentacja projektowa: pokazuje obecny model oraz kierunek dalszej implementacji, ale nie sugeruje, że wszystkie oznaczone elementy są już fizycznie zaimplementowane.

## Zakres diagramu klas

Diagram obejmuje następujące części systemu:

1. **Shared Kernel** — wspólna klasa bazowa `BaseEntity` oraz interfejs znacznikowy `IAggregateRoot`.
2. **Identity Service** — obsługa użytkowników, ról i repozytorium użytkownika.
3. **Wallet Service** — obsługa portfela użytkownika, transakcji oraz mechanizmu optimistic concurrency.
4. **Betting Service** — obsługa sportów, wydarzeń, rynków, kursów, kuponów i pojedynczych zakładów.
5. **Custom Bet Service** — obsługa zgłoszeń indywidualnych zakładów tworzonych przez użytkowników.
6. **Business Logic Layer / Core** — planowane elementy wynikające z opisu projektu, takie jak `WalletService`, `BettingService`, `OddsService`, `SettlementService` i powiadomienia SignalR.

## Relacje logiczne a relacje fizyczne

Ponieważ system jest projektowany jako mikroserwisowy, część relacji między modułami nie jest bezpośrednią relacją obiektową w kodzie. Przykładowo `Wallet`, `Ticket` i `CustomBetRequest` odnoszą się do użytkownika przez `UserId`, a nie przez pole nawigacyjne typu `User`.

Z tego powodu na diagramie zastosowano następujące oznaczenia:

- linia ciągła — relacja domenowa lub relacja w ramach jednego modułu,
- linia przerywana — powiązanie logiczne przez identyfikator, REST/API albo zależność planowana między mikroserwisami.

Takie rozróżnienie zapobiega nieporozumieniom podczas dalszej implementacji. Osoba korzystająca z diagramu widzi, które powiązania są rzeczywistą częścią modelu, a które są logicznym powiązaniem między oddzielnymi usługami.

## Najważniejsze relacje pokazane na diagramie

- `User` logicznie posiada jeden `Wallet`, ale w implementacji powiązanie odbywa się przez `UserId`.
- `User` może posiadać wiele kuponów `Ticket`, również przez `UserId`.
- `User` może tworzyć wiele zgłoszeń `CustomBetRequest`, również przez `UserId`.
- `Wallet` zapisuje wiele operacji `Transaction`.
- `Sport` zawiera wiele wydarzeń `Event`.
- `Event` posiada wiele rynków `Market`.
- `Market` zawiera wiele rezultatów/kursów `Outcome`.
- `Ticket` składa się z wielu pojedynczych zakładów `Bet`.
- `Bet` odnosi się do konkretnego `Outcome` przez `OutcomeId`.
- Interfejsy repozytoriów są implementowane przez konkretne klasy repozytoriów.
- Repozytoria korzystają z odpowiednich klas `DbContext`.
- Planowane serwisy BLL korzystają z repozytoriów oraz komunikacji między modułami zgodnie z założeniami mikroserwisowymi.

## Jak wygenerować diagram z pliku PlantUML

### Opcja 1 — przeglądarka

1. Wejdź na stronę edytora PlantUML:

```text
https://editor.plantuml.com/
```

2. Otwórz plik:

```text
docs/class_diagram.puml
```

3. Skopiuj całą zawartość pliku od `@startuml` do `@enduml`.
4. Wklej kod do edytora PlantUML w przeglądarce.
5. Po wygenerowaniu podglądu pobierz diagram jako PNG albo SVG.
6. Zapisz plik graficzny w katalogu `docs`, np. jako:

```text
docs/class_diagram.png
```

### Opcja 2 — Visual Studio Code

1. Otwórz projekt w Visual Studio Code.
2. Zainstaluj rozszerzenie **PlantUML**.
3. Zainstaluj Javę, jeżeli nie jest jeszcze zainstalowana.
4. Zainstaluj Graphviz, ponieważ PlantUML używa go do renderowania diagramów.
5. Otwórz plik:

```text
docs/class_diagram.puml
```

6. Kliknij prawym przyciskiem myszy w edytorze i wybierz podgląd diagramu PlantUML.
7. Wyeksportuj diagram do PNG lub SVG.

### Opcja 3 — komenda w terminalu

Po pobraniu pliku `plantuml.jar` można użyć komendy:

```bash
java -jar plantuml.jar docs/class_diagram.puml
```

W folderze `docs` zostanie wygenerowany plik graficzny, na przykład:

```text
class_diagram.png
```

Aby wygenerować SVG:

```bash
java -jar plantuml.jar -tsvg docs/class_diagram.puml
```

