# Projekt ZPI

Celem projektu jest dostarczenie aplikacji klient–serwer. Projekt realizowany jest iteracyjnie (sprinty).

## Zasady ogólne

1. **Język projektu: polski.** Dokumentacja, user stories, raporty, komunikaty i rozmowa z Liderem prowadzone są po polsku. Nazwy w kodzie (zmienne, funkcje, klasy) mogą być po angielsku, jeżeli Lider nie zdecyduje inaczej.
2. **Niejednoznaczności — zawsze dopytaj.** Jeżeli wymaganie, decyzja techniczna lub polecenie jest niejednoznaczne, niepełne albo sprzeczne, należy zatrzymać pracę i zadać Liderowi konkretne pytanie. Nie zgadujemy i nie przyjmujemy założeń „po cichu”.
3. **Nie komitujemy kodu.** Commity (oraz push, merge, tagi, tworzenie PR) wykonuje wyłącznie Lider. Agenci mogą tylko odczytywać stan repozytorium (`git status`, `git diff`, `git log`).
4. **Brak artefaktów redakcyjnych między iteracjami.** Po zakończeniu każdego kroku w repozytorium nie mogą pozostać ślady procesu redakcji, m.in.:
   - notatki robocze, szkice, pliki tymczasowe i kopie zapasowe (`*.bak`, `*_old`, `draft_*`, `tmp/` itp.),
   - zakomentowany kod, martwy kod, „tymczasowe” obejścia,
   - znaczniki typu `TODO`, `FIXME`, `XXX`, `[DO UZUPEŁNIENIA]`, `TBD` (chyba że Lider wprost na nie zezwolił),
   - komentarze opisujące historię zmian zamiast działania kodu (np. „zmienione w sprincie 2”, „poprawka wg recenzji”, „NOWE”),
   - nieaktualne wersje dokumentów i user stories — dokument opisuje stan bieżący, historię przechowuje git.
5. **Lider jest bramką akceptacji.** Żaden etap workflow nie rusza bez wyraźnej akceptacji Lidera dla etapu poprzedniego.

## Struktura katalogów

```
wymagania/                   PDF z wymaganiami dostarczany przez Lidera na początku sprintu (tylko do odczytu)
sprinty/sprint-NN/
  user-stories.md            wynik pracy analityka
  raport-implementacji.md    wynik pracy programisty
  raport-recenzji.md         wynik pracy recenzenta
.claude/agents/              definicje agentów (analityk, programista, recenzent)
```

Kod aplikacji (klient i serwer) — struktura zostanie ustalona z Liderem przy pierwszej implementacji i dopisana w tym pliku.

## Stos technologiczny

Nie został jeszcze ustalony. Przed pierwszą implementacją programista musi uzgodnić z Liderem technologie klienta, serwera, bazy danych i testów. Uzgodnione decyzje zapisujemy w tej sekcji.

## Agenci

| Agent | Plik | Rola |
|---|---|---|
| analityk | `.claude/agents/analityk.md` | Analizuje PDF z wymaganiami i rozpisuje je na user stories z kryteriami akceptacji |
| programista | `.claude/agents/programista.md` | Programista full stack — implementuje zaakceptowane user stories (klient + serwer + testy) |
| recenzent | `.claude/agents/recenzent.md` | Weryfikuje zaakceptowaną implementację względem user stories i jakości kodu |

## Workflow sprintu (sekwencyjny)

1. **Lider** umieszcza plik PDF z wymaganiami w katalogu `wymagania/`.
2. **Analityk** podejmuje plik, analizuje go i rozpisuje na user stories (opis + kryteria akceptacji) w `sprinty/sprint-NN/user-stories.md`. Niejasności zgłasza jako pytania do Lidera.
3. **Bramka 1 — Lider akceptuje user stories** (lub zgłasza poprawki → powrót do kroku 2).
4. **Programista** implementuje zaakceptowane user stories i przygotowuje `sprinty/sprint-NN/raport-implementacji.md`.
5. **Bramka 2 — Lider akceptuje kod** (lub zgłasza poprawki → powrót do kroku 4). Lider decyduje o commicie.
6. **Recenzent** weryfikuje zaimplementowane funkcjonalności względem kryteriów akceptacji i zapisuje `sprinty/sprint-NN/raport-recenzji.md`.
7. **Lider** decyduje o zamknięciu sprintu lub skierowaniu uwag recenzenta do programisty (powrót do kroku 4).

Zasady orkiestracji:
- Agenci uruchamiani są **pojedynczo i po kolei** — nigdy równolegle i nigdy z pominięciem bramki akceptacji.
- Po zakończeniu pracy agenta główna sesja przedstawia Liderowi podsumowanie oraz pytania otwarte i **czeka na decyzję**.
- Status każdej user story (`Do akceptacji`, `Zaakceptowana`, `Zaimplementowana`, `Kod zaakceptowany`, `Zweryfikowana`, `Odrzucona w recenzji`) prowadzony jest w `user-stories.md`.
