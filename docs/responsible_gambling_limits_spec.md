# Responsible Gambling Limits - specyfikacja funkcjonalności

## Cel

Funkcjonalność Responsible Gambling Center pozwala użytkownikowi kontrolować ryzyko gry przez limity stawek, depozytów, strat oraz czasową blokadę gry. System przechowuje limity, waliduje operacje oraz pokazuje aktualne wykorzystanie limitów.

## Zakres

Użytkownik może:

- ustawić limit pojedynczej stawki,
- ustawić dzienny limit stawek,
- ustawić tygodniowy limit straty netto,
- ustawić dzienny limit depozytów,
- uruchomić cooling-off / self-exclusion na minimum 24 godziny,
- sprawdzić, czy dana stawka lub depozyt są dozwolone,
- zobaczyć dashboard z aktywnymi limitami, wykorzystaniem i historią blokad.

## Reguły biznesowe

1. Kwoty limitów i aktywności muszą być większe od zera.
2. Obniżenie limitu działa natychmiast.
3. Podwyższenie limitu nie zmienia aktywnej kwoty od razu. System zapisuje zmianę jako oczekującą i aktywuje ją po 24 godzinach.
4. Aktywna self-exclusion blokuje walidację stawki i depozytu.
5. Self-exclusion musi trwać minimum 24 godziny i maksymalnie 365 dni.
6. Limit pojedynczej stawki blokuje stawkę większą niż aktywna kwota limitu.
7. Dzienny limit stawek blokuje stawkę, jeśli suma dzisiejszych stawek i nowej stawki przekroczy limit.
8. Dzienny limit depozytów blokuje depozyt, jeśli suma dzisiejszych depozytów i nowego depozytu przekroczy limit.
9. Tygodniowy limit straty porównuje limit z wartością `stawki z 7 dni - wypłaty z 7 dni`, nie mniej niż zero.

## Endpointy

- `GET /api/responsible-gambling/me` - dashboard użytkownika.
- `POST /api/responsible-gambling/me/limits` - ustawienie limitu.
- `POST /api/responsible-gambling/me/self-exclusion` - uruchomienie blokady.
- `POST /api/responsible-gambling/me/validate-stake` - walidacja stawki.
- `POST /api/responsible-gambling/me/validate-deposit` - walidacja depozytu.
- `POST /api/responsible-gambling/me/activity` - zapis aktywności wykorzystywanej do obliczania limitów.

## Warstwy

- Domena: `ResponsibleGamblingLimit`, `SelfExclusion`, `ResponsibleGamblingActivity`.
- Repozytoria: `IResponsibleGamblingLimitRepository`, `ISelfExclusionRepository`, `IResponsibleGamblingActivityRepository`.
- Logika biznesowa: `ResponsibleGamblingService`.
- API: `ResponsibleGamblingController`.
- Frontend: strona Responsible Gambling Center z dashboardem, formularzami limitów i walidatorem.
