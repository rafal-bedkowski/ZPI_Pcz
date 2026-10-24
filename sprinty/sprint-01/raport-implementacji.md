# Raport implementacji — Sprint 01

## 1. Zakres i mapowanie kryteriów akceptacji na testy

Zaimplementowano US-01-01 do US-01-15. US-01-01–06 i US-01-08, 09, 11–14 zostały wcześniej zaakceptowane i zweryfikowane; w tej iteracji zmieniono US-01-07 (nowa numeracja kryteriów, pkt 5 obejmuje README i skrypty, nowy pkt 6), US-01-10 (doprecyzowany pkt 9) oraz dodano US-01-15 (krok SQL Server w `setup.ps1`). Kryteria dotyczące SQL Server zweryfikowano na lokalnej instancji SQL Server 2025 Express (SQLEXPRESS) — testy `SqlServerProviderTests`, migracja i uruchomienie aplikacji (pkt 4). Ustalenie Lidera: prowadzący wymaga SQL Server; jego instancja może się różnić od lokalnej, dlatego README opisuje ręczną konfigurację dowolnej instancji.

Wszystkie testy mają w nazwie identyfikator US (`US_01_xx_...`). Skrót „M” oznacza weryfikację ręczną opisaną w pkt 4.

| US | Kryteria | Testy / weryfikacja |
|---|---|---|
| US-01-01 | 1, 2, 5, 7 | `ArchitectureTests`: `US_01_01_Solution_ContainsAllProjects`, `US_01_01_ProjectReferences_MatchLayering`, `US_01_01_CsprojFiles_ReferenceExpectedProjects`, `US_01_01_AllProjects_ShareTheSameTargetFramework`, `US_01_01_GlobalJson_PinsSdkMatchingTargetFramework` |
| | 3, 4 | M: `dotnet build` (0 błędów, 0 ostrzeżeń), `dotnet run` → HTTP 200 |
| | 6 | `ArchitectureTests.US_01_01_WebProject_RunsNpmOnlyDuringPublish`; M: `dotnet build` i `dotnet test` z PATH bez Node.js przechodzą, `dotnet publish` bez Node.js kończy się komunikatem o wymaganym Node.js; bez zbudowanego klienta `/` zwraca 200 z instrukcją i w logu startu nie ma ostrzeżenia o `WebRootPath` |
| | 8 | `ConfigurationTests.US_01_01_AllSprintServices_AreRegisteredAndResolvable`, `ArchitectureTests.US_01_01_WebProject_HasNoApiControllersNorSwagger` |
| US-01-02 | 1–6 | `ModelTests.US_01_02_*` (6 testów), niepodzielność adresów: `DbContextTests.US_01_06_SharingAnAddressBetweenCustomers_IsRejected` |
| US-01-03 | 1–3 | `ModelTests.US_01_03_*` |
| US-01-04 | 1–3 | `ModelTests.US_01_04_*` |
| | 4 | `ModelTests.US_01_04_TotalAmount_IsSumOfFrozenUnitPriceTimesQuantity` |
| | 5 | `DbContextTests.US_01_04_OrderWithFractionalPrices_KeepsExactTotalAfterRoundTrip` (SQLite); `SqlServerProviderTests.US_01_10_SqlServer_TotalAmountIsExactAfterRoundTrip` (SQL Server, pomijany bez zmiennej `WEBSTORE_TEST_SQLSERVER`) |
| | 6 | `OrderServiceTests.US_01_11_ProductPriceChange_DoesNotAffectExistingOrder` |
| | 7 | `InvoiceServiceTests.US_01_12_InvoiceTotalAmount_IsComputedFromOrderAndRefreshedAfterOrderChange` |
| US-01-05 | 1 | `ModelTests.US_01_05_StationaryStore_HasExactlyTheRequiredFields` |
| US-01-06 | 1–5 | `DbContextTests.US_01_06_*` (kontekst, relacje 1:N, dwa adresy klienta, 1:1, klucz złożony, TPH) |
| | 6 | `DbContextTests.US_01_06_ProductWithMissingCategoryOrSupplier_ViolatesForeignKey` (SQLite), `SqlServerProviderTests.US_01_10_SqlServer_MissingForeignKeyIsRejected` (SQL Server, pomijany bez zmiennej) |
| | 7 | `ArchitectureTests.US_01_06_ModelServicesViewModels_DoNotReferenceDatabaseProviders` |
| US-01-07 | 1–3 | `ConfigurationTests.US_01_07_*` (domyślny SQLite, SQL Server z konfiguracji, brak ciągu połączenia, nieobsługiwany provider z listą wartości) |
| | 4 | `DbContextTests.US_01_07_PriceOperations_GiveCorrectResultsOnSqlite`, `SqlServerProviderTests.US_01_10_SqlServer_PriceOperationsMatchSqlite` (pomijany bez zmiennej `WEBSTORE_TEST_SQLSERVER`) |
| | 5 | `ArchitectureTests.US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword` (pliki `.cs`, `.json`, `.csproj`, `.md`, `.ps1`, `.sh` w całym repozytorium, w tym README, raporty i user stories sprintów, `setup.ps1`, `setup.sh`, `appsettings*.json`; pomijane tylko `wymagania`, `.claude`, `.git`, `bin`, `obj`, `node_modules`, `wwwroot` i migracje; wzorce: hasło, login w ciągu połączenia (po separatorze lub cudzysłowie, bez fałszywych alarmów na zwykłym kodzie jak `UserId = 5`) oraz serwer z nazwą instancji w składni `host\instancja`); sam wzorzec testują `US_01_07_ConnectionSecretPattern_DetectsConnectionStrings` i `US_01_07_ConnectionSecretPattern_IgnoresOrdinaryCode`; M: ciąg połączenia i hasło są odczytywane wyłącznie z user-secrets lub zmiennej środowiskowej (`DatabaseServiceCollectionExtensions`, `DesignTimeFactories`) |
| | 6 | `SetupScriptTests.US_01_07_AppSettings_DefaultProviderIsSqlite`; `ConfigurationTests.US_01_07_*` (SQL Server wybierany konfiguracją); kroki konfiguracji: US-01-15 |
| | 7 | `MigrationTests.US_01_07_SqliteMigration_CreatesDatabaseFileWithAllTablesAndForeignKeys`; M: `dotnet ef database update` tworzy `WebStore.Web/webstore.db` |
| | 8 | `ArchitectureTests.US_01_07_GitIgnore_ExcludesSqliteDatabaseFile` |
| | 9 | `MigrationTests.US_01_07_SqliteMigrations_AreUpToDateWithModel`; M: `dotnet ef migrations has-pending-model-changes --context SqliteDbContext` — brak zmian |
| | 10 | M (sprawdzone wcześniej): katalog `WebStore.Web` tylko do odczytu → `SQLite Error 14: unable to open database file`, plik i historia migracji nie powstają |
| | 11 | M: `dotnet ef database update --context SqlServerDbContext` (z user-secrets, bez zmiennych środowiskowych) na SQLEXPRESS stosuje migrację `InitialSqlServer` bez błędów (pkt 4) |
| | 12 | `MigrationTests.US_01_07_SqlServerMigrations_ExistAndAreUpToDateWithModel`; M: `has-pending-model-changes --context SqlServerDbContext` — brak zmian |
| | 13 | M: ciąg wskazujący nieistniejącą instancję (`NOSUCHINSTANCE`) → `dotnet ef database update` kończy się błędem połączenia, baza nie powstaje |
| US-01-08 | 1–7 | `ConventionTests.US_01_08_*` |
| US-01-09 | 1–11 | `ProductServiceTests.US_01_09_*` (pkt 11 na SQL Server: `SqlServerProviderTests.US_01_09_SqlServer_ProductService_PriceFiltersReturnOnlyMatchingProducts`) |
| | 12 | `ProductServiceTests.US_01_09_AddOrUpdateProduct_NullImage_KeepsExistingImage`, `US_01_09_AddOrUpdateProduct_NewImage_ReplacesExistingImage` |
| US-01-10 | 1, 2 | `ArchitectureTests.US_01_10_TestProject_UsesXunitV3`, `Startup.cs` |
| | 3, 6, 7, 8 | `SeedDataTests.US_01_10_*` (dane startowe, wymuszanie kluczy obcych, izolacja per test), asercje liczby w `ProductServiceTests`, `OrderServiceTests` |
| | 4, 5 | M: `dotnet test` (kod wyjścia 0), `dotnet test --filter-class` |
| | 9 | `SqlServerProviderTests` (uruchamiane po ustawieniu `WEBSTORE_TEST_SQLSERVER`: 5 testów, żaden nie jest pomijany; wynik w pkt 4) |
| US-01-11 | 1–5, 9–11 | `OrderServiceTests.US_01_11_*` (pkt 10 także `US_01_11_TotalAmount_FractionalPrices_IsExactAfterSaveAndLoad` oraz na SQL Server `SqlServerProviderTests.US_01_11_SqlServer_OrderService_TotalAmountWithFractionalPricesIsExactAfterSaveAndLoad`) |
| | 6 | `US_01_11_NonPositiveQuantity_IsRejectedWithValidationErrorAndNothingSaved` |
| | 7 | `US_01_11_InvalidOrUnknownProduct_IsRejectedWithValidationErrorAndNothingSaved` |
| | 8 | `US_01_11_EmptyItemList_IsRejectedWithValidationErrorAndNothingSaved` |
| US-01-12 | 1–9 | `InvoiceServiceTests.US_01_12_*` (pkt 8: `US_01_12_InvoiceTotalAmount_IsComputedFromOrderAndRefreshedAfterOrderChange`) |
| US-01-13 | 1–6 | `StoreServiceTests.US_01_13_*` |
| US-01-14 | 1–7 | `AddressServiceTests.US_01_14_*` |
| US-01-15 | 1, 2, 3, 4 | M: scenariusze `setup.ps1` w kopii z podmienioną funkcją `Confirm-Action` (interaktywny terminal nie jest dostępny): bez terminala odpowiedź = nie (pkt 4: user-secrets bez zmian, komunikat „SQL Server nie został skonfigurowany”); zgoda przy pustych user-secrets ustawia `Database:Provider` = `SqlServer` i `ConnectionStrings:SqlServer` (pkt 1, 2: ciąg złożony ze zmiennej nazwy instancji: serwer lokalny z nazwą wykrytej instancji, baza `WebStore`, uwierzytelnianie Windows, zaufanie certyfikatowi serwera, bez loginu i hasła); przy istniejących wartościach dodatkowe pytanie, odmowa zostawia je bez zmian z komunikatem (pkt 3), zgoda nadpisuje |
| | 5 | M: kopia skryptu z nazwą usługi, która nie istnieje: brak pytania, komunikat z instrukcją instalacji (`winget install --id Microsoft.SQLServer.2025.Express -e`, instalator Basic), user-secrets bez zmian, kod wyjścia 0, pozostałe kroki wykonane |
| | 6 | `SetupScriptTests.US_01_15_CheckOnly_DoesNotAskAndDoesNotChangeUserSecrets` (Windows: uruchamia `setup.ps1 -CheckOnly` bez wejścia, sprawdza brak pytania `[t/N]` i niezmieniony plik user-secrets); M: `setup.ps1 -CheckOnly` — `[OK] SQL Server Express: wykryto działającą instancję`, user-secrets puste |
| | 7 | `ArchitectureTests.US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword` (patrz US-01-07 pkt 5); M: `git status` po konfiguracji pokazuje tylko zmiany w kodzie tej iteracji — ciąg połączenia istnieje tylko w user-secrets |
| | 8 | M: po konfiguracji i migracji `dotnet run --project WebStore.Web` (bez zmiennych środowiskowych, profil Development ładuje user-secrets) → HTTP 200, w logu brak błędów |
| | 9 | M: `dotnet ef database update --context SqlServerDbContext --project WebStore.DAL --startup-project WebStore.Web` na pustej instancji → migracja `InitialSqlServer` zastosowana (fabryka kontekstu czyta user-secrets i zmienną `ConnectionStrings__SqlServer`) |
| | 10 | M: `dotnet test` z `WEBSTORE_TEST_SQLSERVER` (ustawioną tylko na czas polecenia) przy ustawionych user-secrets: 131 testów, 131 zaliczonych, 0 pominiętych (w tym 5 `SqlServerProviderTests`; przebieg w Release) |
| | 11 | `ConfigurationTests.US_01_07_*` (domyślnie SQLite), `SetupScriptTests.US_01_07_AppSettings_DefaultProviderIsSqlite`; M: `dotnet test` przy ustawionych user-secrets bez zmiennej `WEBSTORE_TEST_SQLSERVER` — testy używają SQLite in-memory, 5 testów SQL Server pominiętych |
| | 12, 13 | `SetupScriptTests.US_01_15_Readme_DescribesWindowsSqlServerSetupAndManualConfiguration` (obecność kluczy, poleceń i zmiennej testowej) oraz przegląd README: sekcje „Windows: SQL Server Express przez setup.ps1”, „Windows: ręczna konfiguracja innej instancji SQL Server” (słowami i wzorem polecenia `dotnet user-secrets set <klucz> <wartość>`, w tym logowanie hasłem), „Testy wariantu SQL Server”; sekcja macOS opisuje wyłącznie SQLite |

## 2. Zmienione / dodane pliki

- `WebStore.sln`, `global.json`, `.gitignore` — solucja, przypięty SDK 10.0.402 z runnerem Microsoft.Testing.Platform, wykluczenia (`*.db`, `node_modules`, `wwwroot`).
- `CLAUDE.md` — dopisana struktura katalogów kodu i zaktualizowany opis klienta.
- `WebStore.Model/DataModels/*.cs` — encje: User, Customer, Supplier, StationaryStoreEmployee, Address, Category, Product, ProductStock, StationaryStore, Order, OrderProduct, Invoice.
- `WebStore.DAL/EF/ApplicationDbContext.cs` — kontekst z relacjami, TPH, kluczem złożonym i obsługą decimal zależną od providera.
- `WebStore.DAL/EF/SqliteDbContext.cs`, `SqlServerDbContext.cs`, `DesignTimeFactories.cs` — konteksty pod osobne zestawy migracji i fabryki dla `dotnet ef`.
- `WebStore.DAL/Configuration/DatabaseServiceCollectionExtensions.cs` — wybór providera z konfiguracji (domyślnie SQLite), czytelny błąd dla nieobsługiwanej wartości.
- `WebStore.ViewModels/VM/*.cs` — 12 view modeli (`Vm`) dla produktów, adresów, sklepów, zamówień i faktur.
- `WebStore.Services/Interfaces/*.cs` — 5 interfejsów usług.
- `WebStore.Services/ConcreteServices/*.cs` — `BaseService` i 5 usług z logowaniem błędów i wyjątkiem „nie znaleziono”.
- `WebStore.Services/Configuration/Profiles/MainProfile.cs`, `ServiceCollectionExtensions.cs` — mapowania i rejestracja usług w DI.
- `WebStore.Web/Program.cs`, `appsettings.json`, `WebStore.Web.csproj` — hosting, rejestracja DB i usług, serwowanie klienta (lub komunikat, gdy nie jest zbudowany), budowanie klienta przy `dotnet publish`.
- `WebStore.DAL/Migrations/Sqlite/*`, `Migrations/SqlServer/*` — migracje początkowe i snapshoty dla obu providerów.
- `dotnet-tools.json` — lokalny manifest narzędzi z `dotnet-ef` 10.0.13.
- `WebStore.Tests/MigrationTests.cs` — testy migracji.
- `WebStore.Web/ClientApp/*` — minimalny klient React (Vite) z `package.json` i `package-lock.json`.
- `WebStore.Tests/*` — projekt xunit.v3: `Startup.cs`, `Infrastructure/*` (dane startowe, logger, ścieżki repo) i 11 klas testowych; filtry ścieżek w `ArchitectureTests` normalizują separator `\` na Windowsie.
- `README.md` — opis projektu i uruchomienia; sekcja „Baza danych”: macOS (SQLite), Windows (SQL Server przez `setup.ps1`, ręczna konfiguracja user-secrets dla innych instancji, testy SQL Server).
- `setup.ps1` — skrypt środowiska dla Windowsa z opcjonalnym krokiem „SQL Server Express”: wykrywa usługę `MSSQL$SQLEXPRESS`, po zgodzie `[t/N]` ustawia user-secrets (`Database:Provider`, `ConnectionStrings:SqlServer`, ciąg składany w czasie działania, dodatkowe pytanie przed nadpisaniem), w `-CheckOnly` tylko informuje; kroki build i test w konfiguracji Release. `setup.sh` (macOS) bez zmian.
- `WebStore.DAL/EF/DesignTimeFactories.cs`, `WebStore.DAL/WebStore.DAL.csproj` — fabryka kontekstu SQL Server dla `dotnet ef` czyta ciąg połączenia z user-secrets lub zmiennej `ConnectionStrings__SqlServer`; zapasowy ciąg bez nazwy instancji; `UserSecretsId` taki sam jak w `WebStore.Web`.
- `WebStore.Tests/SetupScriptTests.cs` — testy: domyślny provider w `appsettings.json`, `setup.ps1 -CheckOnly` (bez pytań i bez zmian user-secrets), zawartość README.
- `WebStore.Tests/ArchitectureTests.cs` — test braku ciągu połączenia/hasła obejmuje `.ps1`, `.sh`, katalog `sprinty` i wzorzec serwera z nazwą instancji; wzorzec jest wydzielony i ma własne testy (wykrywa ciągi połączenia, nie oznacza zwykłego kodu).
- `WebStore.Tests/Infrastructure/RepositoryPaths.cs` — `RepositoryFiles()` (dawniej nazwa sugerująca pliki z gita) zwraca pliki katalogu repozytorium z pominięciem katalogów generowanych, `wymagania` i `.claude`.
- `sprinty/sprint-01/user-stories.md` — zmiana pola `Status`.

## 3. Uruchomienie

PATH musi zawierać `dotnet` (macOS: `/usr/local/share/dotnet` lub `/opt/homebrew/bin`) oraz — do pracy nad klientem — Node.js. Środowisko przygotowuje `setup.ps1` (Windows) lub `setup.sh` (macOS).

- Build: `dotnet build` w katalogu głównym; nie wymaga Node.js.
- Klient: `npm install` i `npm run build` w `WebStore.Web/ClientApp` (wynik w `WebStore.Web/wwwroot`), `npm run dev` do pracy nad nim; `dotnet publish WebStore.Web -c Release` buduje klienta automatycznie (wymaga Node.js).
- Serwer: `dotnet run --project WebStore.Web`; bez zbudowanego klienta strona główna (HTTP 200) wyświetla instrukcję jego zbudowania.
- Migracje: `dotnet tool restore`, potem `dotnet ef database update --context SqliteDbContext --project WebStore.DAL --startup-project WebStore.Web` (plik `webstore.db` powstaje w `WebStore.Web`). Dla SQL Server: `--context SqlServerDbContext` (ciąg połączenia z user-secrets lub zmiennej `ConnectionStrings__SqlServer`; sprawdzone na SQLEXPRESS). Nowa migracja: `dotnet ef migrations add Nazwa --context <kontekst> --project WebStore.DAL --startup-project WebStore.Web --output-dir Migrations/Sqlite` (lub `/SqlServer`).
- Testy: `dotnet test` (na Windowsie z blokadą Smart App Control w Debug: `dotnet test -c Release`; filtr: `dotnet test --filter-class WebStore.Tests.ProductServiceTests` lub `--filter-method`).
- SQL Server na Windowsie: `setup.ps1` (po zgodzie, gdy działa `MSSQL$SQLEXPRESS`) ustawia user-secrets projektu Web; potem `dotnet ef database update --context SqlServerDbContext --project WebStore.DAL --startup-project WebStore.Web` i `dotnet run --project WebStore.Web`. Inna instancja: `dotnet user-secrets set Database:Provider SqlServer --project WebStore.Web` i `dotnet user-secrets set ConnectionStrings:SqlServer <ciąg> --project WebStore.Web`. Powrót do SQLite: `dotnet user-secrets clear --project WebStore.Web`. Testy SQL Server: zmienna `WEBSTORE_TEST_SQLSERVER` z ciągiem połączenia (nazwa bazy zbędna) ustawiona tylko na czas polecenia `dotnet test`.

## 4. Wynik testów i weryfikacji

- `dotnet test -c Release` bez zmiennej `WEBSTORE_TEST_SQLSERVER` (także przy ustawionych user-secrets SQL Server): 131 testów, 126 zaliczonych, 0 niezaliczonych, 5 pominiętych (`SqlServerProviderTests`). Kod wyjścia 0.
- `dotnet test -c Release` ze zmienną `WEBSTORE_TEST_SQLSERVER` (SQL Server 2025 Express, instancja `SQLEXPRESS`, uwierzytelnianie Windows, zmienna tylko na czas polecenia): 131 testów, 131 zaliczonych, 0 niezaliczonych, 0 pominiętych. Po przebiegu w instancji nie zostają bazy `WebStoreTests_*` ani `WebStore`.
- Uwaga środowiskowa: na tym komputerze Smart App Control blokuje ładowanie świeżo zbudowanych bibliotek (np. `WebStore.ViewModels.dll`) z konfiguracji Debug (zdarzenie Code Integrity 3077; nie dotyczy kodu; `dotnet build --no-incremental` i usunięcie `bin` nie pomagają), więc `dotnet test` w Debug kończy się tu niepowodzeniem części testów (`FileLoadException`). Konfiguracja Release przechodzi, dlatego wyniki powyżej pochodzą z Release, a krok testów w `setup.ps1` buduje i testuje w Release. Ustawień bezpieczeństwa nie zmieniano.
- `dotnet build --no-incremental`: 0 błędów, 0 ostrzeżeń.
- `dotnet list package --deprecated` i `--vulnerable`: brak przestarzałych i podatnych pakietów.
- `setup.ps1 -CheckOnly`: kod wyjścia 0, `[OK]` wykrycia instancji SQLEXPRESS, bez pytań, user-secrets bez zmian. Scenariusze kroku SQL Server (brak terminala, zgoda, nadpisanie: odmowa i zgoda, brak usługi) — patrz mapowanie US-01-15; w kopiach skryptu skrócono kroki restore/build/test, aby sprawdzić tylko logikę kroku. Kroki build i test skryptu używają konfiguracji Release (`dotnet build -c Release --no-restore`, `dotnet test -c Release --no-build`); pełnego `setup.ps1` bez `-CheckOnly` nie uruchamiano.
- `dotnet run --project WebStore.Web` (user-secrets ustawione krokiem z `setup.ps1`, bez zmiennych środowiskowych): HTTP 200, w logu brak błędów; bez konfiguracji SQL Server — SQLite, 200 (strona główna nie odpytuje bazy). Bez zbudowanego klienta — 200 z instrukcją jego zbudowania.
- SQL Server: `dotnet ef database update --context SqlServerDbContext` na pustej instancji (ciąg z user-secrets) stosuje `InitialSqlServer` bez błędów; model zgodny ze snapshotami obu providerów; nieistniejąca instancja daje błąd połączenia. Baza `WebStore` została usunięta poleceniem `dotnet ef database drop`; w instancji nie ma baz `WebStore*`.
- `dotnet build` i `dotnet test` z PATH bez Node.js (bez `node_modules`): przechodzą, 0 ostrzeżeń.
- `dotnet publish -c Release`: buduje klienta (`wwwroot/index.html` i `assets` w katalogu publikacji); bez Node.js w PATH kończy się błędem „Do opublikowania klienta React wymagany jest Node.js…”.
- Linter: w projekcie nie skonfigurowano osobnego lintera; analizatory kompilatora i xunit nie zgłaszają ostrzeżeń.

## 5. Nowe zależności i decyzje techniczne

Pakiety NuGet (wersje wg `dotnet package search`): EF Core Sqlite / SqlServer / Design 10.0.13, Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.13, Microsoft.Extensions.Identity.Stores 10.0.13, AutoMapper 16.2.0, Microsoft.Extensions.Logging(.Abstractions) i DependencyInjection.Abstractions 10.0.13, xunit.v3 4.0.2, Xunit.DependencyInjection 12.0.1. Dodatkowo `System.Security.Cryptography.Pkcs` 10.0.13 w DAL i Tests. W DAL dodano Microsoft.Extensions.Configuration.UserSecrets i .EnvironmentVariables 10.0.13 (fabryka kontekstu SQL Server dla `dotnet ef` czyta user-secrets) — podbija tranzytywną wersję 9.0.11 z lukami bezpieczeństwa (NU1902/NU1903), co dawało ostrzeżenia w buildzie.
Narzędzie lokalne: dotnet-ef 10.0.13 (`dotnet-tools.json`); w projekcie Web pakiet Microsoft.EntityFrameworkCore.Design 10.0.13 (PrivateAssets=all) jako projekt startowy dla `dotnet ef`.
Pakiety npm: react i react-dom 19.3.1, vite 8.3.4, @vitejs/plugin-react 6.1.2.

Decyzje:
1. **Klient React.** W .NET 10 nie ma szablonu `dotnet new react`. Użyto `dotnet new web` + klient Vite w `ClientApp`. Wariant uzgodniony z Liderem: `dotnet build` i `dotnet test` nie wymagają Node.js ani nie uruchamiają npm; klient jest budowany przez `dotnet publish` (`npm ci` + `npm run build`, wynik dołączany do publikacji) albo ręcznie. `wwwroot` i `node_modules` są w `.gitignore`.
2. **Decimal na SQLite.** SQLite nie ma typu decimal, a EF zapisuje go jako tekst, co psuje sortowanie, porównania i sumy. Na SQLite kwoty są zapisywane jako grosze (kolumna całkowita) przez konwerter wartości; na SQL Server `decimal(18,2)`. Skutek: dokładność do 2 miejsc po przecinku (zgodnie z kryteriami).
3. **Dwa konteksty pod migracje.** `SqliteDbContext` i `SqlServerDbContext` dziedziczą po `ApplicationDbContext`, aby migracje obu providerów nie mieszały się w jednym zestawie. Usługi i testy używają `ApplicationDbContext`.
4. **Adresy klienta.** Unikalne indeksy na `BillingAddressId` i `ShippingAddressId` zapewniają, że rekord adresu nie jest współdzielony.
5. **Pozycje zamówienia.** Przy aktualizacji zamówienia istniejąca pozycja zachowuje zamrożoną cenę, nowa dostaje aktualną cenę produktu.
6. **Faktura i walidacja zamówień.** `AddOrUpdateInvoiceVm` nie ma `TotalAmount`; usługa zawsze ustawia kwotę faktury na `TotalAmount` zamówienia (także przy ponownym zapisie). Decyzja Lidera: kwota faktury jest ustalana przy jej zapisie i nie zmienia się sama po zmianie zamówienia — aktualizuje ją dopiero ponowny zapis faktury. Drugi zapis faktury dla zamówienia kończy się `InvalidOperationException`; brak zamówienia/klienta/adresu daje `KeyNotFoundException`. `OrderService` odrzuca `ValidationException` (przed jakimkolwiek zapisem): pustą listę pozycji, `Quantity <= 0`, `ProductId <= 0` oraz nieistniejące produkty. Aktualizacja produktu z `ImageBytes = null` zachowuje istniejący obraz (`ImageBytes` w `MainProfile` mapowane warunkowo).
7. **Dodatki ponad listę pól.** `Product.ProductStocks`, `Category.Products` (nawigacje konieczne dla relacji), `ProductVm.Id`, `OrderVm.InvoiceId`, `StoreVm.Address`.
8. **Runner testów.** .NET 10 wymaga w `global.json` ustawienia `test.runner = Microsoft.Testing.Platform`, dlatego pakiety `Microsoft.NET.Test.Sdk` i `xunit.runner.visualstudio` nie są używane.
9. **AutoMapper.** Decyzja Lidera: AutoMapper 16.2.0 w wariancie Community, bez klucza licencyjnego. Biblioteka działa poprawnie, a ostrzeżenie o braku licencji w logach jest znane i akceptowane. Linia 14.x nie wchodzi w grę, bo ma znaną lukę wysokiej wagi (GHSA-rvv3-g6hj-g44x).
10. **SQLite w aplikacji.** Plik `webstore.db` jest umieszczany w katalogu projektu `WebStore.Web` (`ContentRootPath`), a `dotnet ef` tworzy go w tym samym miejscu. Aplikacja nie stosuje migracji przy starcie.
11. **Krok SQL Server w `setup.ps1`.** Skrypt nie zawiera ciągu połączenia: składa go w czasie działania przez `DbConnectionStringBuilder` z nazwy instancji wykrytej w nazwie usługi (serwer lokalny z nazwą instancji, baza `WebStore`, uwierzytelnianie Windows, zaufanie certyfikatowi serwera) i zapisuje wyłącznie w user-secrets. Krok działa po restore, build i testach; wartości user-secrets nie są wypisywane. Aplikacja czyta user-secrets w profilu Development (`dotnet run`), a `dotnet ef` — przez fabrykę kontekstu w DAL (stąd ten sam `UserSecretsId` w `WebStore.DAL.csproj`). Skrypt nie tworzy schematu bazy; podaje polecenie migracji.

## 6. Stan SQL Server i user-secrets

Wariant SQL Server przechodzi wszystkie testy, migrację i uruchomienie aplikacji na lokalnej instancji (pkt 4). Dane startowe testów nie zawierają jawnych `Id` (nadaje je baza), więc działają tak samo na SQLite i SQL Server.

Stan końcowy po weryfikacji: user-secrets projektu `WebStore.Web` są puste, tak jak przed pracą (ustawiałem je wyłącznie na czas weryfikacji i wyczyściłem poleceniem `dotnet user-secrets clear`; plik `secrets.json` usunięty). W instancji SQLEXPRESS nie ma baz `WebStore*`. Aby włączyć SQL Server na tym komputerze, wystarczy uruchomić `setup.ps1` i odpowiedzieć `t` na pytanie, a następnie wykonać migrację z pkt 3. Aplikacja bez konfiguracji używa SQLite.

Niezweryfikowane (stan otwarty): działanie na instancji SQL Server prowadzącego (inna wersja lub typ, logowanie hasłem) oraz na macOS. Ustalenie Lidera: prowadzący wymaga SQL Server, więc README opisuje ręczną konfigurację dowolnej instancji; na instancji prowadzącego konfiguracja nie była sprawdzana.

Ustalenie dotyczące środowiska: krok build i test w `setup.ps1` używa konfiguracji Release, bo Smart App Control na części komputerów z Windows blokuje świeżo zbudowane biblioteki w Debug (pkt 4); README opisuje to krótko.
