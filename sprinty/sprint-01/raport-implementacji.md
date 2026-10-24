# Raport implementacji — Sprint 01

## 1. Zakres i mapowanie kryteriów akceptacji na testy

Zaimplementowano US-01-01 do US-01-14. Kod został zaakceptowany przez Lidera, a wszystkie stories mają status „Zweryfikowana” na providerze SQLite. Kryteria dotyczące SQL Server pozostają niezweryfikowane — testy i migracja na tym providerze czekają na instancję udostępnioną przez prowadzącego (pkt 6).

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
| | 5 | `DbContextTests.US_01_04_OrderWithFractionalPrices_KeepsExactTotalAfterRoundTrip` (SQLite); `SqlServerProviderTests.US_01_10_SqlServer_TotalAmountIsExactAfterRoundTrip` (SQL Server, pomijany bez instancji) |
| | 6 | `OrderServiceTests.US_01_11_ProductPriceChange_DoesNotAffectExistingOrder` |
| | 7 | `InvoiceServiceTests.US_01_12_InvoiceTotalAmount_IsComputedFromOrderAndRefreshedAfterOrderChange` |
| US-01-05 | 1 | `ModelTests.US_01_05_StationaryStore_HasExactlyTheRequiredFields` |
| US-01-06 | 1–5 | `DbContextTests.US_01_06_*` (kontekst, relacje 1:N, dwa adresy klienta, 1:1, klucz złożony, TPH) |
| | 6 | `DbContextTests.US_01_06_ProductWithMissingCategoryOrSupplier_ViolatesForeignKey` (SQLite), `SqlServerProviderTests.US_01_10_SqlServer_MissingForeignKeyIsRejected` (SQL Server, pomijany) |
| | 7 | `ArchitectureTests.US_01_06_ModelServicesViewModels_DoNotReferenceDatabaseProviders` |
| US-01-07 | 1–3 | `ConfigurationTests.US_01_07_*` (domyślny SQLite, SQL Server z konfiguracji, brak ciągu połączenia, nieobsługiwany provider z listą wartości) |
| | 4 | `DbContextTests.US_01_07_PriceOperations_GiveCorrectResultsOnSqlite`, `SqlServerProviderTests.US_01_10_SqlServer_PriceOperationsMatchSqlite` (pomijany bez instancji) |
| | 5, 7 | `ArchitectureTests.US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword`, `US_01_07_GitIgnore_ExcludesSqliteDatabaseFile` |
| | 6 | `MigrationTests.US_01_07_SqliteMigration_CreatesDatabaseFileWithAllTablesAndForeignKeys`; M: `dotnet ef database update` tworzy `WebStore.Web/webstore.db` |
| | 8 | `MigrationTests.US_01_07_SqliteMigrations_AreUpToDateWithModel`; M: kolejna migracja jest pusta |
| | 9 | M: katalog `WebStore.Web` tylko do odczytu → `SQLite Error 14: unable to open database file`, plik i historia migracji nie powstają |
| | 10, 12 | Nie sprawdzone: brak instancji SQL Server (migracja SQL Server wygenerowana, nie zastosowana) |
| | 11 | `MigrationTests.US_01_07_SqlServerMigrations_ExistAndAreUpToDateWithModel`; M: kolejna migracja jest pusta |
| US-01-08 | 1–7 | `ConventionTests.US_01_08_*` |
| US-01-09 | 1–11 | `ProductServiceTests.US_01_09_*` |
| | 12 | `ProductServiceTests.US_01_09_AddOrUpdateProduct_NullImage_KeepsExistingImage`, `US_01_09_AddOrUpdateProduct_NewImage_ReplacesExistingImage` |
| US-01-10 | 1, 2 | `ArchitectureTests.US_01_10_TestProject_UsesXunitV3`, `Startup.cs` |
| | 3, 6, 7, 8 | `SeedDataTests.US_01_10_*` (dane startowe, wymuszanie kluczy obcych, izolacja per test), asercje liczby w `ProductServiceTests`, `OrderServiceTests` |
| | 4, 5 | M: `dotnet test` (kod wyjścia 0), `dotnet test --filter-class` |
| | 9 | `SqlServerProviderTests` (uruchamiane po ustawieniu `WEBSTORE_TEST_SQLSERVER`; niezweryfikowane w tym środowisku) |
| US-01-11 | 1–5, 9–11 | `OrderServiceTests.US_01_11_*` (pkt 10 także `US_01_11_TotalAmount_FractionalPrices_IsExactAfterSaveAndLoad`) |
| | 6 | `US_01_11_NonPositiveQuantity_IsRejectedWithValidationErrorAndNothingSaved` |
| | 7 | `US_01_11_InvalidOrUnknownProduct_IsRejectedWithValidationErrorAndNothingSaved` |
| | 8 | `US_01_11_EmptyItemList_IsRejectedWithValidationErrorAndNothingSaved` |
| US-01-12 | 1–9 | `InvoiceServiceTests.US_01_12_*` (pkt 8: `US_01_12_InvoiceTotalAmount_IsComputedFromOrderAndRefreshedAfterOrderChange`) |
| US-01-13 | 1–6 | `StoreServiceTests.US_01_13_*` |
| US-01-14 | 1–7 | `AddressServiceTests.US_01_14_*` |

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
- `README.md`, `setup.ps1`, `setup.sh` — opis projektu i uruchomienia oraz skrypty przygotowania środowiska dla Windowsa i macOS.
- `sprinty/sprint-01/user-stories.md` — zmiana pola `Status`.

## 3. Uruchomienie

PATH musi zawierać `dotnet` (macOS: `/usr/local/share/dotnet` lub `/opt/homebrew/bin`) oraz — do pracy nad klientem — Node.js. Środowisko przygotowuje `setup.ps1` (Windows) lub `setup.sh` (macOS).

- Build: `dotnet build` w katalogu głównym; nie wymaga Node.js.
- Klient: `npm install` i `npm run build` w `WebStore.Web/ClientApp` (wynik w `WebStore.Web/wwwroot`), `npm run dev` do pracy nad nim; `dotnet publish WebStore.Web -c Release` buduje klienta automatycznie (wymaga Node.js).
- Serwer: `dotnet run --project WebStore.Web`; bez zbudowanego klienta strona główna (HTTP 200) wyświetla instrukcję jego zbudowania.
- Migracje: `dotnet tool restore`, potem `dotnet ef database update --context SqliteDbContext --project WebStore.DAL --startup-project WebStore.Web` (plik `webstore.db` powstaje w `WebStore.Web`). Dla SQL Server: `--context SqlServerDbContext` ze zmienną `ConnectionStrings__SqlServer` (niezastosowane, brak instancji). Nowa migracja: `dotnet ef migrations add Nazwa --context <kontekst> --project WebStore.DAL --startup-project WebStore.Web --output-dir Migrations/Sqlite` (lub `/SqlServer`).
- Testy: `dotnet test` (filtr: `dotnet test --filter-class WebStore.Tests.ProductServiceTests` lub `--filter-method`).
- SQL Server (opcjonalnie): `Database__Provider=SqlServer` oraz `ConnectionStrings__SqlServer` ze zmiennej środowiskowej lub `dotnet user-secrets` (projekt Web ma `UserSecretsId`); testy SQL Server: zmienna `WEBSTORE_TEST_SQLSERVER` z ciągiem połączenia bez nazwy bazy.

## 4. Wynik testów i weryfikacji

- `dotnet test`: 113 testów, 110 zaliczonych, 0 niezaliczonych, 3 pominięte (`SqlServerProviderTests` — brak instancji SQL Server). Kod wyjścia 0 na macOS i na Windowsie.
- `dotnet build --no-incremental`: 0 błędów, 0 ostrzeżeń.
- `dotnet list package --deprecated --vulnerable`: brak przestarzałych i podatnych pakietów.
- `dotnet run --project WebStore.Web`: strona główna zwraca HTTP 200; bez zbudowanego klienta — 200 z instrukcją jego zbudowania.
- SQL Server: niezweryfikowane (brak instancji) — wynik nieznany.
- `dotnet build` i `dotnet test` z PATH bez Node.js (bez `node_modules`): przechodzą, 0 ostrzeżeń.
- `dotnet publish -c Release`: buduje klienta (`wwwroot/index.html` i `assets` w katalogu publikacji); bez Node.js w PATH kończy się błędem „Do opublikowania klienta React wymagany jest Node.js…”.
- Linter: w projekcie nie skonfigurowano osobnego lintera; analizatory kompilatora i xunit nie zgłaszają ostrzeżeń.

## 5. Nowe zależności i decyzje techniczne

Pakiety NuGet (wersje wg `dotnet package search`): EF Core Sqlite / SqlServer / Design 10.0.13, Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.13, Microsoft.Extensions.Identity.Stores 10.0.13, AutoMapper 16.2.0, Microsoft.Extensions.Logging(.Abstractions) i DependencyInjection.Abstractions 10.0.13, xunit.v3 4.0.2, Xunit.DependencyInjection 12.0.1. Dodatkowo `System.Security.Cryptography.Pkcs` 10.0.13 w DAL i Tests — podbija tranzytywną wersję 9.0.11 z lukami bezpieczeństwa (NU1902/NU1903), co dawało ostrzeżenia w buildzie.
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

## 6. Stan otwarty: SQL Server

Status wariantu SQL Server jest nieznany — nie był uruchamiany na żadnej instancji. Decyzja Lidera: weryfikacja czeka na instancję udostępnioną przez prowadzącego; do tego czasu sprint 1 pozostaje otwarty.

- Niezweryfikowane: migracja SQL Server (US-01-07 pkt 10 i 12) oraz testy `SqlServerProviderTests` (US-01-04 pkt 5, US-01-06 pkt 6, US-01-07 pkt 4, US-01-10 pkt 9).
- Znane ryzyko: `TestData.Seed` wstawia jawne `Id` do kolumn IDENTITY, co na SQL Server prawdopodobnie wymaga poprawki przy pierwszym uruchomieniu.
- Brakuje testów usług na SQL Server (US-01-09 pkt 11, US-01-11 pkt 10) — do dodania po uzyskaniu instancji.
