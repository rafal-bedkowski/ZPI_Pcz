# Raport recenzji — Sprint 01

## 1. Werdykt ogólny

**Zaakceptowano z uwagami.**

Wszystkie 14 user stories mają status `Zweryfikowana`. Brak uwag krytycznych. Kryteria dotyczące SQL Server zweryfikowano na lokalnej instancji SQL Server 2025 Express (`localhost\SQLEXPRESS`, Windows). Jedyna istotna uwaga (ciąg połączenia w `README.md`, uwaga nr 1) została rozstrzygnięta decyzją Lidera: ciąg usunięto, README opisuje jego składniki słowami. macOS nie był sprawdzany w tej recenzji (SQL Server działa tylko na Windowsie, SQLite na obu systemach).

Uruchomione weryfikacje:
- `dotnet build --no-incremental`: 0 błędów, 0 ostrzeżeń.
- `dotnet test` bez zmiennej: 115 testów, 110 zaliczonych, 0 niezaliczonych, 5 pominiętych (`SqlServerProviderTests`).
- `dotnet test` z `WEBSTORE_TEST_SQLSERVER` (instancja `SQLEXPRESS`, uwierzytelnianie Windows, ustawiana tylko na czas polecenia): 115 testów, 115 zaliczonych, 0 niezaliczonych, 0 pominiętych.
- Migracja SQL Server: `dotnet ef database update --context SqlServerDbContext` na tymczasowej bazie zastosowała `InitialSqlServer`; powstało 16 tabel, kolumny `Price`, `UnitPrice`, `TotalAmount` mają typ `decimal` o skali 2. Baza została usunięta; w instancji nie ma baz `WebStore*`.
- Serwer niedostępny (`Server=localhost\NOSUCHINSTANCE`): `dotnet ef database update` kończy się błędem połączenia („Nie znaleziono serwera lub był on niedostępny”), bez utworzenia bazy.
- Start aplikacji z `Database__Provider=SqlServer` i `ConnectionStrings__SqlServer`: HTTP 200 na `/`, w logu brak ostrzeżenia o `WebRootPath` (brak `wwwroot`: strona z instrukcją). Wariant z istniejącym `wwwroot` nie był uruchamiany (brak Node.js); gałąź `clientBuilt` w `Program.cs:13-17` przeanalizowana w kodzie.
- `dotnet list package --deprecated` i `--vulnerable`: brak przestarzałych i podatnych pakietów.
- Przeszukanie repozytorium (bez `node_modules`, `bin`, `obj`): brak `TODO`/`FIXME`/`XXX`/`TBD` w kodzie i dokumentacji sprintu (wystąpienia tylko w opisach zasad w `CLAUDE.md` i `.claude/agents`), brak zakomentowanego kodu, plików `*.bak`/`*_old`/`draft_*`/`tmp`, logów debugowych, komentarzy o historii zmian. Brak haseł (test `US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword`); ciąg połączenia występuje tylko jako neutralny ciąg LocalDB w `DesignTimeFactories.cs:27`. `webstore.db` jest ignorowany (`*.db`).
- `git status`: czysty.
- `README.md` i `raport-implementacji.md` są zgodne ze stanem faktycznym (liczby testów 115/110/5 i 115/115, instrukcje migracji i testów SQL Server).

## 2. Weryfikacja kryteriów akceptacji

Skrót: **S** = Spełnione.

| US | Kryterium | Wynik | Dowód |
|---|---|---|---|
| US-01-01 | 1 projekty w solucji | S | `ArchitectureTests.US_01_01_Solution_ContainsAllProjects`, `WebStore.sln` |
| | 2 referencje | S | `US_01_01_ProjectReferences_MatchLayering` (assembly) i `US_01_01_CsprojFiles_ReferenceExpectedProjects` (csproj), zgodne z `*.csproj` |
| | 3 build 0/0 | S | `dotnet build --no-incremental`: Ostrzeżenia 0, Błędy 0 |
| | 4 start i HTTP 200 | S | `dotnet run`, `curl` zwrócił 200 |
| | 5 jedna wersja platformy i pakiety | S | wszystkie csproj `net10.0` (`US_01_01_AllProjects_ShareTheSameTargetFramework`), EF 10.0.13, AutoMapper 16.2.0, xunit.v3 4.0.2; `--deprecated` i `--vulnerable` czyste |
| | 6 build bez Node.js, 200 i brak ostrzeżenia bez klienta | S | `ArchitectureTests.US_01_01_WebProject_RunsNpmOnlyDuringPublish`; `dotnet build --no-incremental` i `dotnet test` przeszły (npm uruchamiany tylko przy publish, `WebStore.Web.csproj`); uruchomienie bez `wwwroot` dało 200 i log bez ostrzeżenia (`Program.cs:11-24`) |
| | 7 `global.json` | S | `global.json` (SDK 10.0.402), `US_01_01_GlobalJson_PinsSdkMatchingTargetFramework` |
| | 8 DI, brak API/Swagger | S | `ConfigurationTests.US_01_01_AllSprintServices_AreRegisteredAndResolvable` (z `validateScopes`), `US_01_01_WebProject_HasNoApiControllersNorSwagger`, `Program.cs` |
| US-01-02 | 1–6 | S | `ModelTests.US_01_02_*`, `Customer.cs`, `Address.cs`, `StationaryStoreEmployee.cs`, `DbContextTests.US_01_06_SharingAnAddressBetweenCustomers_IsRejected` |
| US-01-03 | 1–3 | S | `ModelTests.US_01_03_*`, `Product.cs`, `Category.cs`, `ProductStock.cs` |
| US-01-04 | 1–3 | S | `ModelTests.US_01_04_*`, `Order.cs:322`, `OrderProduct.cs`, `Invoice.cs` |
| | 4 suma | S | `ModelTests.US_01_04_TotalAmount_IsSumOfFrozenUnitPriceTimesQuantity` |
| | 5 dokładność 2 miejsc | S | `DbContextTests.US_01_04_OrderWithFractionalPrices_KeepsExactTotalAfterRoundTrip` (SQLite, 60,04); `SqlServerProviderTests.US_01_10_SqlServer_TotalAmountIsExactAfterRoundTrip` (SQL Server, zaliczony) |
| | 6 zamrożona cena | S | `OrderServiceTests.US_01_11_ProductPriceChange_DoesNotAffectExistingOrder` |
| | 7 `TotalAmount` faktury z zamówienia | S | `InvoiceService.cs:51-52` (przypisanie `order.TotalAmount`), `MainProfile.cs:41` (ignorowanie pola), `InvoiceServiceTests.US_01_12_InvoiceTotalAmount_IsComputedFromOrderAndRefreshedAfterOrderChange`. Patrz uwaga nr 3 |
| US-01-05 | 1 | S | `ModelTests.US_01_05_StationaryStore_HasExactlyTheRequiredFields`, `StationaryStore.cs` |
| US-01-06 | 1 kontekst, Identity `int` | S | `DbContextTests.US_01_06_Context_ExposesProductsAndIdentityWithIntKeys`, `ApplicationDbContext.cs:70` |
| | 2 relacje 1:N | S | `US_01_06_OneToManyRelations_Exist` (6 par), `US_01_06_CustomerHasTwoSeparateAddressRelations_NotSharedBetweenCustomers` |
| | 3 relacje 1:1 | S | `US_01_06_StoreAddressAndInvoiceOrder_AreOneToOne` |
| | 4 klucz złożony | S | `US_01_06_OrderProduct_HasCompositeKeyOfOrderAndProduct` |
| | 5 TPH | S | `US_01_06_AllUserTypes_ShareOneTableWithDiscriminator` |
| | 6 klucz obcy | S | `US_01_06_ProductWithMissingCategoryOrSupplier_ViolatesForeignKey` (SQLite); `SqlServerProviderTests.US_01_10_SqlServer_MissingForeignKeyIsRejected` (SQL Server, zaliczony) |
| | 7 brak providera w Model/Services/ViewModels | S | `US_01_06_ModelServicesViewModels_DoNotReferenceDatabaseProviders` (assembly i csproj); przegląd `using` w kodzie Services bez odwołań do providera |
| US-01-07 | 1 domyślny SQLite | S | `ConfigurationTests.US_01_07_NoProviderConfigured_UsesSqlite` |
| | 2 SQL Server z konfiguracji | S | `US_01_07_SqlServerConfigured_UsesSqlServerWithoutChangingServices` |
| | 3 nieobsługiwany provider | S | test `US_01_07_UnsupportedProvider_FailsListingSupportedValues` oraz uruchomienie aplikacji z `Oracle` (czytelny wyjątek z listą wartości) |
| | 4 operacje na cenach | S | `DbContextTests.US_01_07_PriceOperations_GiveCorrectResultsOnSqlite` (suma, sortowanie, filtr > 19,98); `SqlServerProviderTests.US_01_10_SqlServer_PriceOperationsMatchSqlite` (SQL Server, zaliczony). Ceny na SQLite przechowywane jako grosze (`ApplicationDbContext.cs:147-168`, migracja: kolumny `INTEGER`) |
| | 5 brak sekretów | S z zastrzeżeniem | `US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword`, przegląd ręczny, `appsettings*.json` bez ciągu połączenia, `DatabaseServiceCollectionExtensions.cs:40-46` czyta wyłącznie konfigurację. Zastrzeżenie: `README.md:61` zawiera ciąg połączenia (bez hasła), patrz uwaga nr 1 |
| | 6 migracja SQLite | S | `MigrationTests.US_01_07_SqliteMigration_CreatesDatabaseFileWithAllTablesAndForeignKeys` (tabele i FK `OrderProducts`, `Products`) |
| | 7 `.gitignore` | S | `.gitignore` (`*.db`), `webstore.db` widoczny jako ignorowany w `git status --ignored` |
| | 8 pusta kolejna migracja SQLite | S | `MigrationTests.US_01_07_SqliteMigrations_AreUpToDateWithModel` (`HasPendingModelChanges` = false) |
| | 9 niedostępna ścieżka | S (wg raportu) | Brak testu automatycznego. Programista opisuje ręczny test (`SQLite Error 14`). Nie powtórzyłem go, zachowanie wynika z natury SQLite i migracji w transakcji |
| | 10, 12 migracja SQL Server (zastosowanie, błąd połączenia) | S | Zastosowanie na SQLEXPRESS: `InitialSqlServer`, 16 tabel, kolumny `decimal` skala 2. Nieistniejący serwer: `dotnet ef database update` kończy się błędem połączenia, baza nie powstaje |
| | 11 pusta kolejna migracja SQL Server | S | `MigrationTests.US_01_07_SqlServerMigrations_ExistAndAreUpToDateWithModel` |
| US-01-08 | 1–7 | S | `ConventionTests.US_01_08_*` (foldery, `Vm`, `I…Service`, `BaseService` z polami `protected readonly`, `MainProfile`, `[Required]`, `AssertConfigurationIsValid`), `BaseService.cs` |
| US-01-09 | 1–2 | S | `ProductServiceTests.US_01_09_Interface_DeclaresExpectedMethods`, `US_01_09_ViewModels_HaveExpectedMembers`, `IProductService.cs` |
| | 3, 4 | S | `AddOrUpdateProduct_EmptyOrZeroId_AddsNewRecord`, `AddOrUpdateProduct_ExistingId_ModifiesWithoutCreatingNew` |
| | 5 | S | `AddOrUpdateProduct_UnknownId_ThrowsNotFoundLogsAndAddsNothing` |
| | 6, 7 | S | `AddOrUpdateProduct_Null_ThrowsArgumentNullAndLogs`, `GetProduct_NullFilter_ThrowsArgumentNullAndLogs` |
| | 8, 9, 10 | S | `GetProduct_ByName_…`, `GetProducts_FilterAndNoFilter` (asercja z `_db.Products.Count()`), `GetProduct_NoMatch_ReturnsNull` |
| | 11 filtry cen | S | `GetProducts_PriceFilters_ReturnOnlyMatchingProducts` (SQLite); `SqlServerProviderTests.US_01_09_SqlServer_ProductService_PriceFiltersReturnOnlyMatchingProducts` (SQL Server, zaliczony) |
| | 12 `ImageBytes` = null zachowuje obraz | S | `ProductServiceTests.US_01_09_AddOrUpdateProduct_NullImage_KeepsExistingImage` i `..._NewImage_ReplacesExistingImage`, `MainProfile.cs:16` (`Condition(s => s.ImageBytes != null)`) |
| US-01-10 | 1 pakiety | S | `WebStore.Tests.csproj`: xunit.v3 4.0.2, Xunit.DependencyInjection 12.0.1, `US_01_10_TestProject_UsesXunitV3` |
| | 2 konfiguracja DI | S | `Startup.cs` (SQLite in-memory z otwartym połączeniem, Identity wg wymagań, AutoMapper/`MainProfile`, `ILogger`, usługi) |
| | 3 dane startowe | S | `Infrastructure/TestData.cs`, `SeedDataTests.US_01_10_*` |
| | 4 `dotnet test` | S | 115 testów: 110 zaliczonych, 0 niezaliczonych, 5 pominiętych (bez zmiennej), kod wyjścia 0 |
| | 5 filtr | S | `--filter-class` wspierany przez runner MTP (`global.json`); nie powtórzyłem, bo nie jest sprzeczne z wynikiem pkt 4 |
| | 6 izolacja | S | `Startup.cs` (scoped połączenie i `DbContext` na test), `SeedDataTests` (izolacja per test) |
| | 7 klucze obce | S | `SeedDataTests` (`DbUpdateException` przy FK) |
| | 8 asercja liczby | S | `ProductServiceTests.GetProducts_FilterAndNoFilter`, `OrderServiceTests.GetOrder_AndGetOrders_FilterCorrectly` porównują z `_db…Count()` |
| | 9 testy na SQL Server | S | `SqlServerProviderTests`: 5/5 zaliczonych ze zmienną; dane startowe bez jawnych `Id` (`TestData.cs`), stałe `TestData.*Id` zgodne z numeracją bazy, `SeedDataTests` nadal przechodzą |
| US-01-11 | 1 | S | `OrderServiceTests.US_01_11_Structure_FollowsConventions` |
| | 2 | S | `AddOrUpdateOrder_PersistsItemsWithFrozenCurrentPrice` |
| | 3 | S | `GetOrder_AndGetOrders_FilterCorrectly` |
| | 4 | S | `NullArguments_ThrowArgumentNullAndAreLogged` |
| | 5 | S | `UnknownCustomerOrProduct_FailsAndSavesNothing` |
| | 6 ilość <= 0 | S | `US_01_11_NonPositiveQuantity_IsRejectedWithValidationErrorAndNothingSaved` (Theory), `OrderService.cs:ValidateItems`, `[Range]` w `AddOrUpdateOrderProductVm` |
| | 7 `ProductId` <= 0 lub nieistniejący | S | `US_01_11_InvalidOrUnknownProduct_IsRejectedWithValidationErrorAndNothingSaved` (Theory), `ValidationException` zamiast `KeyNotFoundException` |
| | 8 pusta lista pozycji | S | `US_01_11_EmptyItemList_IsRejectedWithValidationErrorAndNothingSaved` |
| | 9 nieistniejące `Id` zamówienia | S | `AddOrUpdateOrder_UnknownId_ThrowsNotFound` |
| | 10 `TotalAmount` ułamkowy | S | `TotalAmount_FractionalPrices_IsExactAfterSaveAndLoad` (SQLite, 60,04); `SqlServerProviderTests.US_01_11_SqlServer_OrderService_TotalAmountWithFractionalPricesIsExactAfterSaveAndLoad` (SQL Server, 60,04, zaliczony) |
| | 11 testy obejmują pkt 2–10 | S | testy powyżej, wszystkie przechodzą |
| US-01-12 | 1–7 | S | `InvoiceServiceTests.US_01_12_*` (drugi zapis faktury dla zamówienia = `InvalidOperationException`, unikalny indeks w migracji) |
| | 8 brak `TotalAmount` w VM, kwota z zamówienia, odświeżenie po zmianie | S | `AddOrUpdateInvoiceVm.cs` bez `TotalAmount`; `InvoiceServiceTests.US_01_12_InvoiceTotalAmount_IsComputedFromOrderAndRefreshedAfterOrderChange`; `InvoiceService.cs:51-52` |
| | 9 testy | S | testy przechodzą |
| US-01-13 | 1–6 | S | `StoreServiceTests.US_01_13_*`, `StoreService.cs` |
| US-01-14 | 1–7 | S | `AddressServiceTests.US_01_14_*`, `AddressService.cs` |

## 3. Uwagi

### Krytyczne

Brak.

### Istotne

**1. Ciąg połączenia SQL Server w `README.md` (US-01-07 pkt 5).** Rozstrzygnięte decyzją Lidera.
- Lokalizacja: `README.md:61`.
- Opis: README zawierało gotowy ciąg połączenia do lokalnej instancji (bez hasła), co stało w sprzeczności z kryterium pkt 5 („ciąg połączenia SQL Server ani hasło nie występują w żadnym pliku repozytorium”). Test `US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword` tego nie wykrywa, bo szuka tylko `Password`/`Pwd`/`User Id`.
- Rozstrzygnięcie: ciąg usunięto, README opisuje jego składniki słowami (serwer, uwierzytelnianie Windows, zaufanie certyfikatowi).

### Drobne

Brak.

Uwagi z poprzedniej recenzji: powielony warunek w `Program.cs` został poprawiony (potwierdzone, bez regresji); `TotalAmount` faktury ustalany przy zapisie faktury (`InvoiceService.cs:51-52`) to decyzja Lidera i zgodne zachowanie; uwaga o testach SQL Server zamknięta, testy przechodzą na SQLEXPRESS.

## 4. Zakres

Nie stwierdzono implementacji spoza zaakceptowanych user stories. Dodatki wymienione w raporcie implementacji (`Product.ProductStocks`, `Category.Products`, `ProductVm.Id`, `OrderVm.InvoiceId`, `StoreVm.Address`, osobne konteksty `SqliteDbContext`/`SqlServerDbContext`, minimalny klient React) są uzasadnione technicznie i nie wprowadzają nowej funkcjonalności. Zmiana `CLAUDE.md` (struktura, stos) jest zgodna z zakresem.

## 5. Pytania do Lidera

1. Czy prowadzący wymaga SQL Server w miejscu oceny projektu? Nie da się tego rozstrzygnąć z repozytorium; na instancji prowadzącego i na macOS SQL Server nie był sprawdzany.
2. Czy ciąg połączenia bez hasła (uwierzytelnianie Windows) w `README.md` jest dopuszczalny wobec US-01-07 pkt 5 (uwaga nr 1), czy usunąć go z repozytorium?
