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
- **Baza danych:** SQL Server (docelowa; Windows: Express, instancja `SQLEXPRESS`) lub SQLite (wariant, m.in. dla macOS); provider wybiera się jawnie w konfiguracji, patrz „Baza danych”.
- **Klient:** React 19 + Vite (`WebStore.Web/ClientApp`).
- **API:** ASP.NET Core Web API, uwierzytelnianie tokenem JWT (Identity), dokumentacja Swagger (Swashbuckle) w środowisku Development.
- **Testy:** xunit.v3, SQLite in-memory, testy integracyjne API przez `WebApplicationFactory`.

## Struktura repozytorium

```
wymagania/            dokumenty z wymaganiami na kolejne sprinty
sprinty/sprint-NN/    user stories, raporty implementacji i recenzji
.claude/agents/       definicje agentów: analityk, programista, recenzent
WebStore.Model/       encje
WebStore.DAL/         kontekst EF Core, konfiguracja providera, migracje
WebStore.ViewModels/  view modele
WebStore.Services/    usługi i mapowanie AutoMappera
WebStore.Web/         aplikacja ASP.NET Core: kontrolery Web API i klient React (ClientApp)
WebStore.Tests/       testy jednostkowe i integracyjne
CLAUDE.md             zasady pracy w projekcie
```

## Uruchomienie

Wymagane: Git i .NET SDK 10. Node.js (LTS) jest potrzebny tylko do pracy nad klientem React; do budowania klienta wymagana jest wersja co najmniej 22.22.0 (pole `engines` w `WebStore.Web/ClientApp/package.json`).

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

`dotnet build` i `dotnet test` nie wymagają konfiguracji bazy (testy używają SQLite w pamięci). Uruchomienie aplikacji (`dotnet run`) wymaga wyboru bazy w konfiguracji (patrz „Baza danych”); bez ustawienia `Database:Provider` start kończy się czytelnym błędem.

`setup.ps1` buduje i testuje w konfiguracji Release (`dotnet build -c Release`, `dotnet test -c Release`). Na części komputerów z Windows funkcja Smart App Control blokuje ładowanie świeżo zbudowanych bibliotek z konfiguracji Debug (zdarzenie Code Integrity 3077, `FileLoadException`), przez co `dotnet test` w Debug kończy się niepowodzeniem; Release nie jest blokowany. Jeżeli ręczne `dotnet test` kończy się takim błędem, użyj `dotnet test -c Release`. Ustawień zabezpieczeń Windows nie trzeba zmieniać.

Klient React: `npm install` i `npm run dev` w `WebStore.Web/ClientApp` (praca nad klientem) lub `npm run build` (wynik w `WebStore.Web/wwwroot`).

### Testy

Testy integracyjne API (folder `WebStore.Tests/Api`) uruchamiają aplikację w pamięci przez `WebApplicationFactory`; każdy test ma własną bazę SQLite w pamięci i własną konfigurację JWT (losowy klucz i hasło kont testowych), więc nie wymagają user-secrets ani Node.js. Testy usług z folderu `WebStore.Tests/UnitTests` (klasy `ProductServiceUnitTests`, `OrderServiceUnitTests`, `InvoiceServiceUnitTests`, `StoreServiceUnitTests`, `AddressServiceUnitTests` dziedziczące po `BaseUnitTests`) korzystają z danych startowych z `Extensions.SeedData` (źródło: `Infrastructure/TestData`); każdy test ma własną bazę SQLite w pamięci. Poniższe polecenia wykonuje się w katalogu głównym repozytorium; na komputerach, na których Smart App Control blokuje Debug (patrz wyżej), dodaj `-c Release` (poniżej w tej postaci).

- wszystkie testy: `dotnet test -c Release`,
- pojedyncza metoda: `dotnet test -c Release --filter-method WebStore.Tests.UnitTests.ProductServiceUnitTests.UpdateProductTest`,
- cała klasa: `dotnet test -c Release --filter-class WebStore.Tests.UnitTests.ProductServiceUnitTests`.

Runner Microsoft.Testing.Platform (xunit.v3) wymaga w `--filter-method` i `--filter-class` pełnej nazwy z przestrzenią nazw (dla metody także z nazwą klasy); sama krótka nazwa (np. `UpdateProductTest`) nie wybiera żadnego testu, a polecenie kończy się kodem wyjścia 8. W tej konfiguracji działa też krótki filtr z instrukcji laboratorium, np. `dotnet test -c Release --filter UpdateProductTest` lub `--filter ProductServiceUnitTests`.

## Web API

Kontrolery (`WebStore.Web/Controllers`, trasy `api/<Zasób>Api`): `AccountApi`, `ProductApi`, `OrderApi`, `InvoiceApi`, `StoreApi`, `AddressApi`. Wszystkie endpointy poza `POST /api/AccountApi/Login` wymagają nagłówka `Authorization: Bearer <token>`. Logika biznesowa pozostaje w usługach (`WebStore.Services`).

### Konfiguracja tokenu JWT

Wydawca (`Issuer`), odbiorca (`Audience`) i czas ważności (`TokenExpirationMinutes`, 20 minut) są w sekcji `JwtOptions` pliku `appsettings.json`. Klucz podpisujący (`JwtOptions:SecretKey`, co najmniej 32 bajty) nie może trafić do repozytorium:

- w środowisku Development, gdy klucz nie jest ustawiony, aplikacja losuje go przy każdym starcie (tokeny wydane przed restartem przestają działać),
- w pozostałych środowiskach brak klucza przerywa start z komunikatem; ustaw go w user-secrets (`dotnet user-secrets set "JwtOptions:SecretKey" "<klucz>" --project WebStore.Web`) lub zmienną środowiskową `JwtOptions__SecretKey`.

### Konta testowe

Tylko w środowisku Development, po zastosowaniu migracji, aplikacja tworzy role `Admin` i `Customer` oraz konta `admin@webstore.local` i `customer@webstore.local`. Wspólne hasło tych kont ustawiasz w user-secrets (`dotnet user-secrets set "Seed:TestAccountPassword" "<hasło>" --project WebStore.Web`) lub zmienną środowiskową `Seed__TestAccountPassword` (co najmniej 6 znaków). Bez tego ustawienia konta nie powstają, a w logu pojawia się ostrzeżenie. Hasło nie jest przechowywane w repozytorium.

### Swagger i logowanie

Po `dotnet run --project WebStore.Web` (Development) dokumentacja jest pod adresem `/swagger/index.html` (np. `https://localhost:7192/swagger/index.html`; profil `https` w `launchSettings.json`). Token uzyskujesz z `POST /api/AccountApi/Login` (JSON lub formularz z polami `Login` i `Password`); odpowiedź zawiera `access_token` i `expires_in`. Token podajesz w oknie „Authorize” Swaggera (sam token, bez słowa `Bearer`) albo w nagłówku `Authorization` w Postmanie. `DELETE /api/ProductApi/{id}` ukrywa produkt (flaga w bazie), nie usuwa rekordu.

Po aktualizacji repozytorium zastosuj nową migrację (flaga ukrycia produktu) dla wybranego providera (polecenia w sekcji „Baza danych”).

### Klient React a API

Serwer deweloperski klienta (`npm run dev`) przekazuje żądania `/api` i `/swagger` do `WebStore.Web` (`server.proxy` w `vite.config.js`), więc nie jest potrzebny CORS. Domyślny adres serwera API to `https://localhost:7192`; inny adres ustawia zmienna środowiskowa `API_TARGET` (np. `API_TARGET=http://localhost:5299 npm run dev`).

## Baza danych

Docelową bazą jest SQL Server, a SQLite jest wariantem (m.in. dla macOS). Aplikacja nie ma domyślnego providera: bez ustawienia `Database:Provider` start kończy się błędem z opisem, co ustawić. Provider i ciąg połączenia ustawia się wyłącznie poza repozytorium (user-secrets lub zmienne środowiskowe); ciąg połączenia SQL Server ani hasło nie mogą trafić do żadnego pliku repozytorium (ani do `appsettings*.json`, README, skryptów, ani do kodu). Dla `Database:Provider` dopuszczalne wartości to `SqlServer` i `Sqlite`.

### macOS (SQLite)

Wybór SQLite: `dotnet user-secrets set "Database:Provider" "Sqlite" --project WebStore.Web` (albo zmienna środowiskowa `Database__Provider=Sqlite`, albo profil `https-sqlite` w `launchSettings.json`, który ustawia ją za Ciebie). Plik bazy `webstore.db` powstaje w katalogu `WebStore.Web` po zastosowaniu migracji: `dotnet ef database update --context SqliteDbContext --project WebStore.DAL --startup-project WebStore.Web`. Skrypt `setup.sh` przypomina o tym kroku po sprawdzeniu narzędzi.

### Windows: SQL Server Express przez setup.ps1

Jeżeli działa usługa `MSSQL$SQLEXPRESS` (SQL Server Express, instancja `SQLEXPRESS`), `setup.ps1` wykrywa ją i po wykonaniu restore, build i testów pyta `[t/N]` (domyślnie „nie”), czy skonfigurować aplikację do pracy z nią. Po odpowiedzi `t` skrypt ustawia w user-secrets projektu `WebStore.Web` wartości `Database:Provider` = `SqlServer` oraz `ConnectionStrings:SqlServer`. Ciąg połączenia skrypt składa w trakcie działania z nazwy wykrytej instancji (uwierzytelnianie Windows bez loginu i hasła, baza `WebStore`, zaufanie certyfikatowi serwera) i zapisuje go tylko w user-secrets, poza repozytorium. Jeżeli user-secrets zawierają już te wartości, skrypt dodatkowo pyta o ich nadpisanie; odpowiedź inna niż `t` niczego nie zmienia (aplikacja nie uruchomi się wtedy bez ręcznego ustawienia providera). Z parametrem `-CheckOnly` skrypt tylko informuje o wykryciu instancji. Gdy instancji nie ma, wypisuje, jak ją zainstalować (np. `winget install --id Microsoft.SQLServer.2025.Express -e`, instalator Basic), oraz jak wybrać SQLite.

Po konfiguracji:

- schemat bazy: `dotnet ef database update --context SqlServerDbContext --project WebStore.DAL --startup-project WebStore.Web` (polecenie czyta te same user-secrets),
- uruchomienie na SQL Server: `dotnet run --project WebStore.Web` (bez dodatkowych zmiennych środowiskowych),
- przejście na SQLite: `dotnet user-secrets set "Database:Provider" "Sqlite" --project WebStore.Web`.

### Windows: ręczna konfiguracja innej instancji SQL Server

Dla dowolnej innej instancji (inna wersja lub typ, instancja zdalna, w tym z logowaniem loginem i hasłem) ustaw ręcznie dwa user-secrets projektu `WebStore.Web` poleceniami `dotnet user-secrets set` z kluczami `Database:Provider` (wartość `SqlServer`) oraz `ConnectionStrings:SqlServer` (ciąg połączenia do tej instancji z nazwą bazy w parametrze `Initial Catalog`; w uwierzytelnianiu Windows bez loginu i hasła, przy logowaniu hasłem z loginem i hasłem). Polecenie ma postać `dotnet user-secrets set <klucz> <wartość> --project WebStore.Web`. Alternatywnie te same wartości można podać zmiennymi środowiskowymi `Database__Provider` i `ConnectionStrings__SqlServer`. Ciąg połączenia i hasło wpisujemy wyłącznie w user-secrets lub zmiennych środowiskowych; nie mogą trafić do żadnego pliku repozytorium.

### Testy wariantu SQL Server

Testy wariantu SQL Server (`WebStore.Tests.SqlServerProviderTests`) są pomijane, dopóki zmienna środowiskowa `WEBSTORE_TEST_SQLSERVER` nie zawiera ciągu połączenia do instancji SQL Server. Ustawiamy ją tylko w bieżącej sesji terminala, nie w repozytorium. Dla lokalnej instancji `SQLEXPRESS` ciąg składa się z serwera (`localhost`, backslash i nazwa instancji), uwierzytelniania Windows (zintegrowanego, bez loginu i hasła) oraz zaufania certyfikatowi serwera (lokalna instancja ma certyfikat samopodpisany); nazwa bazy nie jest potrzebna, bo każdy test tworzy i usuwa własną bazę. Następnie `dotnet test` (lub `dotnet test --filter-class WebStore.Tests.SqlServerProviderTests`). Zwykłe `dotnet test` korzysta z SQLite w pamięci niezależnie od user-secrets i od wyboru providera aplikacji.
