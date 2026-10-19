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
WebStore.sln                 solucja aplikacji (kod w katalogu głównym repozytorium)
global.json                  przypięty .NET SDK i runner testów (Microsoft.Testing.Platform)
WebStore.Model/              encje (DataModels), bez zależności od providera bazy
WebStore.DAL/                ApplicationDbContext (EF), konfiguracja providera, migracje
WebStore.ViewModels/         view modele (VM, nazwy z sufiksem Vm)
WebStore.Services/           interfejsy (Interfaces), usługi (ConcreteServices), MainProfile AutoMappera
WebStore.Web/                aplikacja ASP.NET Core serwująca klienta React (ClientApp, Vite)
WebStore.Tests/              testy jednostkowe xunit.v3
```

Zależności projektów: DAL→Model; ViewModels→Model; Services→Model, DAL, ViewModels; Web→Model, DAL, ViewModels, Services; Tests→Services, Model, DAL.

## Stos technologiczny

Uzgodniony z Liderem na podstawie wymagań sprintu 1:

- Serwer: .NET 10 (LTS), C#, jedna wersja platformy dla całej solucji, przypięta w `global.json`.
- ORM: Entity Framework Core 10.x.
- Baza danych: SQLite domyślnie; SQL Server jako wariant alternatywny wybierany konfiguracją. Praca odbywa się na macOS i Windowsie.
- Mapowanie obiektów: AutoMapper 16.x (wariant Community, użytek edukacyjny; bez klucza licencyjnego w logach pojawia się ostrzeżenie o braku licencji, co jest znane i akceptowane).
- Testy: xunit.v3, baza SQLite in-memory (osobna dla każdego testu).
- Klient: React 19 (Vite), źródła w `WebStore.Web/ClientApp` (szablon `dotnet new react` nie istnieje w .NET 10). Zwykły `dotnet build` i `dotnet test` nie wymagają Node.js; klient budowany jest przez `dotnet publish` (wymaga Node.js) lub ręcznie `npm run build` w `ClientApp` (wynik w `WebStore.Web/wwwroot`), a praca nad nim przez `npm run dev`.
- Migracje EF: narzędzie `dotnet-ef` jako lokalne (`dotnet-tools.json`, `dotnet tool restore`); osobne migracje w `WebStore.DAL/Migrations/Sqlite` i `/SqlServer`.
- Wersje pakietów ustala programista poleceniem `dotnet package search` i wybiera aktualne, nieprzestarzałe wydania stabilne.
- Hasła i ciągi połączenia nie trafiają do repozytorium (user-secrets lub zmienne środowiskowe).
- Instalacje narzędzi wykonywane są wyłącznie po akceptacji Lidera.

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
