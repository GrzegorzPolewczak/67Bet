# Roulette - specyfikacja funkcjonalnosci

Funkcjonalnosc Roulette pozwala zalogowanemu uzytkownikowi postawic dowolna liczbe zakladow na jednym obrocie europejskiej ruletki, zaplacic laczna stawke z portfela, poznac wylosowany numer i otrzymac wyplate za wygrywajace zaklady.

## Zasady

- Uzytkownik wybiera stawke oraz rodzaj zakladu na stole do gry. Moze dodac do 10 zakladow na jeden obrot.
- Dostepne typy zakladow:
  - **StraightUp** - konkretny numer 0–36, kurs 35:1 (mnoznik 36).
  - **Red / Black** - kolor czerwony lub czarny, kurs 1:1 (mnoznik 2).
  - **Even / Odd** - parzyste lub nieparzyste (0 przegrywa), kurs 1:1.
  - **Low / High** - 1–18 lub 19–36, kurs 1:1.
  - **DozenFirst / DozenSecond / DozenThird** - dziesiatki 1–12, 13–24, 25–36, kurs 2:1 (mnoznik 3).
  - **ColumnFirst / ColumnSecond / ColumnThird** - kolumny (numery mod 3), kurs 2:1.
- Serwis pobiera laczna stawke z portfela przez endpoint `POST /api/RouletteWallet/process-stake`.
- Wynik obrotu (liczba 0–36) jest losowany kryptograficznie (`RandomNumberGenerator.GetInt32(0, 37)`).
- Dla kazdego zakladu sprawdzany jest warunek wygranej, a wyplata liczy sie jako `stawka * mnoznik`.
- Po zatwierdzeniu wyplaty serwis przekazuje laczna wyplate do portfela przez `POST /api/RouletteWallet/process-payout`.
- Historia rund przechowywana jest w repozytorium pamieci podrejenczej (InMemory).

## Endpointy

- `POST /api/Roulette/play` - wykonuje obrot i zwraca wynik.
- `POST /api/Roulette/{roundId}/settle` - przetwarza wyplate do portfela i oznacza runde jako rozliczona.
- `GET /api/Roulette/history?limit=10` - zwraca ostatnie rundy uzytkownika.

Przykladowe body dla `POST /api/Roulette/play`:

```json
{
  "bets": [
    { "betType": 1, "chosenNumber": null, "stake": 10 },
    { "betType": 0, "chosenNumber": 17, "stake": 5 }
  ]
}
```

Wartosci `betType`: 0=StraightUp, 1=Red, 2=Black, 3=Even, 4=Odd, 5=Low, 6=High,
7=DozenFirst, 8=DozenSecond, 9=DozenThird, 10=ColumnFirst, 11=ColumnSecond, 12=ColumnThird.

## Integracja z portfelem

Endpoint `POST /api/RouletteWallet/process-stake` pobiera laczna stawke ze wszystkich zakladow jednorazowo. Endpoint `POST /api/RouletteWallet/process-payout` uznaje sume wyplat ze wszystkich wygrywajacych zakladow. Obydwa endpointy wymagaja autoryzacji JWT i deleguja do IWalletService.

## Limity odpowiedzialnej gry

Przed pobraniem stawki serwis wywoluje `IResponsibleGamblingService.ValidateStakeAsync`, a po pobraniu `RecordActivityAsync`, tak samo jak pozostale gry kasynowe (Plinko).
