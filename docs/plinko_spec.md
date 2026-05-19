# Plinko - specyfikacja funkcjonalnosci

Funkcjonalnosc Plinko pozwala zalogowanemu uzytkownikowi postawic kwote z portfela, uruchomic symulacje spadania kulki po planszy oraz otrzymac wyplate zgodna z mnoznikiem pola koncowego.

## Zasady

- Uzytkownik wybiera stawke, poziom ryzyka: `Low`, `Medium` albo `High`, oraz liczbe rzedow od 8 do 16.
- Serwis pobiera stawke z portfela przez endpoint portfela `POST /api/PlinkoWallet/process-stake`.
- Sciezka kulki jest losowana kryptograficznie jako sekwencja ruchow `L` albo `R`.
- Numer pola koncowego wynika z liczby ruchow w prawo.
- Mnoznik jest wyliczany na podstawie liczby rzedow i poziomu ryzyka. Pola skrajne maja najwyzsze mnozniki, pola srodkowe najnizsze.
- Wyplata to `stawka * mnoznik`, zaokraglona do 2 miejsc po przecinku.
- Serwis zleca wyplate do portfela przez endpoint `POST /api/PlinkoWallet/process-payout`.
- Historia rund jest przechowywana w repozytorium pamieciowym dodanym specjalnie dla funkcjonalnosci Plinko.

## Endpointy

- `GET /api/Plinko/board?riskLevel=Medium&rows=12` - zwraca konfiguracje mnoznikow.
- `POST /api/Plinko/play` - uruchamia runde i zwraca wynik.
- `GET /api/Plinko/history?limit=10` - zwraca ostatnie rundy uzytkownika.

Przykladowe body dla `POST /api/Plinko/play`:

```json
{
  "stake": 25,
  "riskLevel": 2,
  "rows": 12
}
```

Wartosc `riskLevel`: `1` = Low, `2` = Medium, `3` = High.

## Ograniczenie integracyjne

Zgodnie z zalozeniem projektu funkcjonalnosc zostala dodana bez edycji istniejacych plikow. Dlatego frontend jest dostepny jako osobna strona `public/plinko.html`, a backend korzysta z samodzielnego kontrolera i repozytorium pamieciowego.
