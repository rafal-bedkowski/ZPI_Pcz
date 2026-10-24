# ZPI

Projekt zespołowy realizowany w ramach przedmiotu ZPI — aplikacja klient–serwer WebStore rozwijana iteracyjnie (sprinty) z wykorzystaniem środowiska agentowego Claude Code.

## Workflow

Każdy sprint przebiega sekwencyjnie, a przejście do kolejnego etapu wymaga akceptacji Lidera:

1. **Lider** dostarcza dokument PDF z wymaganiami do katalogu `wymagania/`.
2. **Analityk** rozpisuje wymagania na user stories z kryteriami akceptacji.
3. **Programista** (full stack) implementuje zaakceptowane user stories.
4. **Recenzent** weryfikuje zaakceptowaną implementację.

Szczegółowe zasady projektu znajdują się w [CLAUDE.md](CLAUDE.md), a opisy ról agentów w katalogu [.claude/agents](.claude/agents).

## Stos technologiczny

- **Serwer:** .NET 10 (LTS), C#, ASP.NET Core; wersja SDK przypięta w `global.json`.
- **Dostęp do danych:** Entity Framework Core 10, AutoMapper 16.
- **Baza danych:** SQLite (domyślnie, macOS) lub SQL Server (Windows: Express, instancja `SQLEXPRESS`; wybór w konfiguracji).
- **Klient:** React 19 + Vite (`WebStore.Web/ClientApp`).
- **Testy:** xunit.v3, SQLite in-memory.

## Struktura repozytorium

```
wymagania/            dokumenty z wymaganiami na kolejne sprinty
sprinty/sprint-NN/    user stories, raporty implementacji i recenzji
.claude/agents/       definicje agentów: analityk, programista, recenzent
WebStore.Model/       encje
WebStore.DAL/         kontekst EF Core, konfiguracja providera, migracje
WebStore.ViewModels/  view modele
WebStore.Services/    usługi i mapowanie AutoMappera
WebStore.Web/         aplikacja ASP.NET Core z klientem React (ClientApp)
WebStore.Tests/       testy jednostkowe
CLAUDE.md             zasady pracy w projekcie
```

## Uruchomienie

Wymagane: Git i .NET SDK 10. Node.js (LTS) jest potrzebny tylko do pracy nad klientem React.

Po sklonowaniu repozytorium skrypt sprawdza narzędzia (instalacja brakujących tylko po potwierdzeniu), a następnie wykonuje restore, build i testy:

- Windows (PowerShell): `powershell -ExecutionPolicy Bypass -File .\setup.ps1`
- macOS: `./setup.sh`

Ręcznie:

```
dotnet tool restore
dotnet build
dotnet test
dotnet run --project WebStore.Web
```

Klient React: `npm install` i `npm run dev` w `WebStore.Web/ClientApp` (praca nad klientem) lub `npm run build` (wynik w `WebStore.Web/wwwroot`).

Testy wariantu SQL Server są pomijane, dopóki zmienna środowiskowa `WEBSTORE_TEST_SQLSERVER` nie zawiera ciągu połączenia do instancji SQL Server.

Baza danych według systemu: na Windowsie używamy SQL Server (Express, instancja `SQLEXPRESS`), na macOS SQLite (domyślny). SQL Server włącza się zmiennymi `Database__Provider=SqlServer` i `ConnectionStrings__SqlServer` (zmienna środowiskowa lub `dotnet user-secrets`, nigdy w repozytorium). Testy SQL Server na Windowsie: `$env:WEBSTORE_TEST_SQLSERVER = 'Server=localhost\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True'`, następnie `dotnet test --filter-class WebStore.Tests.SqlServerProviderTests`. Do ustalenia z prowadzącym pozostaje, czy wymaga SQL Server w miejscu oceny projektu.
