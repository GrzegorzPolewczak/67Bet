# REFERRAL & PROMO CODE SYSTEM SPECIFICATION

## 1. Cel systemu
System poleceĹ„ i kodĂłw promocyjnych ma na celu zwiÄ™kszenie zaangaĹĽowania uĹĽytkownikĂłw poprzez nagradzanie ich za zapraszanie nowych graczy (Kody TwĂłrcĂłw) oraz umoĹĽliwienie korzystania z ogĂłlnodostÄ™pnych promocji (Kody Deweloperskie).

## 2. Kluczowe pojÄ™cia
- **Freebet**: Wirtualne Ĺ›rodki na koncie uĹĽytkownika, ktĂłre mogÄ… byÄ‡ wykorzystane do zawierania zakĹ‚adĂłw, ale nie mogÄ… byÄ‡ bezpoĹ›rednio wypĹ‚acone.
- **Kod TwĂłrcy (Creator Code)**: Unikalny kod (max 10 znakĂłw) generowany przez uĹĽytkownika, sĹ‚uĹĽÄ…cy do polecania platformy znajomym.
- **Kod Deweloperski (Promo Code)**: Kod generowany przez administratora (np. "WORLDCUP26"), dostÄ™pny dla wielu uĹĽytkownikĂłw.
- **Kamienie Milowe (Milestones)**: Progi liczby poleconych osĂłb, po osiÄ…gniÄ™ciu ktĂłrych TwĂłrca otrzymuje nagrodÄ™ w postaci Freebetu.

## 3. Mechanika Freebetu
- **Saldo Freebet**: Oddzielne od salda gĹ‚Ăłwnego (Real Balance).
- **Zasada 70% wygranej**: JeĹĽeli kupon zostanie postawiony za Ĺ›rodki z Freebetu, finalna wygrana dopisana do salda gĹ‚Ăłwnego wynosi **70%** wyliczonej wygranej potencjalnej.
- **UĹĽycie**: System automatycznie priorytetyzuje Ĺ›rodki Freebet przy zawieraniu zakĹ‚adu, jeĹĽeli sÄ… one dostÄ™pne (uproszczona logika dla etapu MVP).

## 4. System PoleceĹ„ (Creator Codes)
- **Tworzenie**: UĹĽytkownik moĹĽe stworzyÄ‡ jeden wĹ‚asny kod w zakĹ‚adce Settings.
- **Walidacja**: Max 10 znakĂłw, brak znakĂłw specjalnych, unikalnoĹ›Ä‡ w skali systemu.
- **UĹĽycie kodu znajomego**:
    - UĹĽytkownik moĹĽe wprowadziÄ‡ kod znajomego tylko **raz** w historii konta.
    - Po wprowadzeniu, uĹĽytkownik otrzymuje bonus Freebet (kwota konfigurowalna).
- **Nagrody dla TwĂłrcy (Kamienie Milowe)**:
    - Licznik poleceĹ„: UsageCount.
    - Progi nagrĂłd: 5, 15, 25, 50, 100, 250.
    - Po osiÄ…gniÄ™ciu progu, TwĂłrca otrzymuje Freebet na konto.

## 5. System KodĂłw Promocyjnych (Promo Codes)
- **Tworzenie**: Tylko przez Administratora.
- **Charakterystyka**: Kod moĹĽe byÄ‡ uĹĽyty przez wielu uĹĽytkownikĂłw, ale kaĹĽdy uĹĽytkownik moĹĽe go uĹĽyÄ‡ tylko **raz**.
- **ZarzÄ…dzanie**: Administrator moĹĽe deaktywowaÄ‡ kod w dowolnym momencie.

## 6. Architektura i Baza Danych
### Nowe Encje (Wallet Service):
- ReferralCode: Id, UserId, Code, UsageCount, CreatedAt
- PromoCode: Id, Code, RewardAmount, IsActive, CreatedAt
- UserCodeUsage: Id, UserId, CodeId (lub Code), UsedAt (Ĺ›ledzenie wykorzystania kodĂłw promo i poleceĹ„)

### Zmiany w istniejÄ…cych encjach:
- Wallet: Dodanie pola FreebetBalance.
- Ticket: Dodanie pola IsFreebet, aby silnik rozliczajÄ…cy wiedziaĹ‚ o koniecznoĹ›ci zastosowania mnoĹĽnika 0.7.

## 7. Panel Administratora
- Sekcja "ZarzÄ…dzanie kodami".
- Dodawanie nowych kodĂłw promo (Nazwa, Kwota).
- Lista kodĂłw z moĹĽliwoĹ›ciÄ… deaktywacji.
