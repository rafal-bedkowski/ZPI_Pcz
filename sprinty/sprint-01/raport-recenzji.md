# Raport recenzji — Sprint 01

## 1. Werdykt ogólny

**Zaakceptowano z uwagami.**

Wszystkie 14 user stories mają status `Zweryfikowana`. Ponowna recenzja objęła US-01-01, 04, 09, 11 i 12 (zaktualizowane kryteria po poprawkach) oraz regresje. Pozostałe stories nie były zmieniane i zachowują wynik z poprzedniej recenzji. Brak uwag krytycznych. Części kryteriów dotyczących SQL Server (migracja, testy cen i kluczy obcych na tym providerze) nie da się zweryfikować bez instancji. Lider zna ten stan i zaakceptował go (uwaga nr 1 wstrzymana do zajęć).

Uruchomione weryfikacje:
- `dotnet build --no-incremental`: 0 błędów, 0 ostrzeżeń.
- `dotnet test`: 113 testów, 110 zaliczonych, 0 niezaliczonych, 3 pominięte (`SqlServerProviderTests`, brak `WEBSTORE_TEST_SQLSERVER`).
- `dotnet list package --deprecated` i `--vulnerable`: brak przestarzałych i podatnych pakietów.
- `dotnet run --project WebStore.Web --urls http://127.0.0.1:5988` (bez `wwwroot`): HTTP 200 na `/`, w logu startowym brak ostrzeżenia o `WebRootPath`.
- `Database__Provider=Oracle`: start kończy się wyjątkiem „Nieobsługiwany provider bazy danych 'Oracle'… Obsługiwane wartości: Sqlite (SQLite), SqlServer (SQL Server)”.
- Przeszukanie repozytorium (bez `node_modules`, `bin`, `obj`): brak `TODO`/`FIXME`/`XXX`/`TBD`, zakomentowanego kodu, plików `*.bak`/`*_old`/`draft_*`/`tmp`, logów debugowych (`Console.Write`), komentarzy o historii zmian. Brak haseł i ciągów połączenia (test `US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword` oraz przegląd ręczny; w `DesignTimeFactories.cs` jest tylko neutralny ciąg LocalDB z `Trusted_Connection`). `webstore.db` jest ignorowany przez git (`.gitignore: *.db`).
- `git status`: wyłącznie zmodyfikowane pliki śledzone (kod, testy, dokumenty sprintu), brak nowych plików nieśledzonych. Pozostałe wpisy to ignorowane `.DS_Store` i `webstore.db`.

## 2. Weryfikacja kryteriów akceptacji

Skróty: **S** = Spełnione, **SQL** = część dotycząca SQL Server nie do zweryfikowania (brak instancji, stan zaakceptowany przez Lidera), **Odst.** = odstępstwo od treści zatwierdzone decyzją Lidera.

| US | Kryterium | Wynik | Dowód |
|---|---|---|---|
| US-01-01 | 1 projekty w solucji | S | `ArchitectureTests.US_01_01_Solution_ContainsAllProjects`, `WebStore.sln` |
| | 2 referencje | S | `US_01_01_ProjectReferences_MatchLayering` (assembly) i `US_01_01_CsprojFiles_ReferenceExpectedProjects` (csproj), zgodne z `*.csproj` |
| | 3 build 0/0 | S | `dotnet build --no-incremental`: Ostrzeżenia 0, Błędy 0 |
| | 4 start i HTTP 200 | S | `dotnet run`, `curl` zwrócił 200 |
| | 5 jedna wersja platformy i pakiety | S | wszystkie csproj `net10.0` (`US_01_01_AllProjects_ShareTheSameTargetFramework`), EF 10.0.13, AutoMapper 16.2.0, xunit.v3 4.0.2; `--deprecated` i `--vulnerable` czyste |
| | 6 build bez Node.js, 200 i brak ostrzeżenia bez klienta | S | `ArchitectureTests.US_01_01_WebProject_RunsNpmOnlyDuringPublish`; `dotnet build --no-incremental` i `dotnet test` przeszły (npm uruchamiany tylko przy publish, `WebStore.Web.csproj`); uruchomienie bez `wwwroot` dało 200 i log bez ostrzeżenia (`Program.cs:11-20`) |
| | 7 `global.json` | S | `global.json` (SDK 10.0.402), `US_01_01_GlobalJson_PinsSdkMatchingTargetFramework` |
| | 8 DI, brak API/Swagger | S | `ConfigurationTests.US_01_01_AllSprintServices_AreRegisteredAndResolvable` (z `validateScopes`), `US_01_01_WebProject_HasNoApiControllersNorSwagger`, `Program.cs` |
| US-01-02 | 1–6 | S | `ModelTests.US_01_02_*`, `Customer.cs`, `Address.cs`, `StationaryStoreEmployee.cs`, `DbContextTests.US_01_06_SharingAnAddressBetweenCustomers_IsRejected` |
| US-01-03 | 1–3 | S | `ModelTests.US_01_03_*`, `Product.cs`, `Category.cs`, `ProductStock.cs` |
| US-01-04 | 1–3 | S | `ModelTests.US_01_04_*`, `Order.cs:322`, `OrderProduct.cs`, `Invoice.cs` |
| | 4 suma | S | `ModelTests.US_01_04_TotalAmount_IsSumOfFrozenUnitPriceTimesQuantity` |
| | 5 dokładność 2 miejsc | S (SQLite) / SQL | `DbContextTests.US_01_04_OrderWithFractionalPrices_KeepsExactTotalAfterRoundTrip` (60,04); SQL Server: `SqlServerProviderTests` pominięty. Patrz uwaga nr 1 |
| | 6 zamrożona cena | S | `OrderServiceTests.US_01_11_ProductPriceChange_DoesNotAffectExistingOrder` |
| | 7 `TotalAmount` faktury z zamówienia | S | `InvoiceService.cs:51-52` (przypisanie `order.TotalAmount`), `MainProfile.cs:41` (ignorowanie pola), `InvoiceServiceTests.US_01_12_InvoiceTotalAmount_IsComputedFromOrderAndRefreshedAfterOrderChange`. Patrz uwaga nr 3 |
| US-01-05 | 1 | S | `ModelTests.US_01_05_StationaryStore_HasExactlyTheRequiredFields`, `StationaryStore.cs` |
| US-01-06 | 1 kontekst, Identity `int` | S | `DbContextTests.US_01_06_Context_ExposesProductsAndIdentityWithIntKeys`, `ApplicationDbContext.cs:70` |
| | 2 relacje 1:N | S | `US_01_06_OneToManyRelations_Exist` (6 par), `US_01_06_CustomerHasTwoSeparateAddressRelations_NotSharedBetweenCustomers` |
| | 3 relacje 1:1 | S | `US_01_06_StoreAddressAndInvoiceOrder_AreOneToOne` |
| | 4 klucz złożony | S | `US_01_06_OrderProduct_HasCompositeKeyOfOrderAndProduct` |
| | 5 TPH | S | `US_01_06_AllUserTypes_ShareOneTableWithDiscriminator` |
| | 6 klucz obcy | S (SQLite) / SQL | `US_01_06_ProductWithMissingCategoryOrSupplier_ViolatesForeignKey` (oba klucze); SQL Server pominięty |
| | 7 brak providera w Model/Services/ViewModels | S | `US_01_06_ModelServicesViewModels_DoNotReferenceDatabaseProviders` (assembly i csproj); przegląd `using` w kodzie Services bez odwołań do providera |
| US-01-07 | 1 domyślny SQLite | S | `ConfigurationTests.US_01_07_NoProviderConfigured_UsesSqlite` |
| | 2 SQL Server z konfiguracji | S | `US_01_07_SqlServerConfigured_UsesSqlServerWithoutChangingServices` |
| | 3 nieobsługiwany provider | S | test `US_01_07_UnsupportedProvider_FailsListingSupportedValues` oraz uruchomienie aplikacji z `Oracle` (czytelny wyjątek z listą wartości) |
| | 4 operacje na cenach | S (SQLite) / SQL | `DbContextTests.US_01_07_PriceOperations_GiveCorrectResultsOnSqlite` (suma, sortowanie rosnące i malejące, filtr > 19,98); SQL Server pominięty. Ceny na SQLite przechowywane jako grosze (`ApplicationDbContext.cs:147-168`, migracja: kolumny `INTEGER`) |
| | 5 brak sekretów | S | `US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword`, przegląd ręczny, `appsettings*.json` bez ciągu połączenia, `DatabaseServiceCollectionExtensions.cs:40-46` czyta wyłącznie konfigurację |
| | 6 migracja SQLite | S | `MigrationTests.US_01_07_SqliteMigration_CreatesDatabaseFileWithAllTablesAndForeignKeys` (tabele i FK `OrderProducts`, `Products`) |
| | 7 `.gitignore` | S | `.gitignore` (`*.db`), `webstore.db` widoczny jako ignorowany w `git status --ignored` |
| | 8 pusta kolejna migracja SQLite | S | `MigrationTests.US_01_07_SqliteMigrations_AreUpToDateWithModel` (`HasPendingModelChanges` = false) |
| | 9 niedostępna ścieżka | S (wg raportu) | Brak testu automatycznego. Programista opisuje ręczny test (`SQLite Error 14`). Nie powtórzyłem go, zachowanie wynika z natury SQLite i migracji w transakcji |
| | 10, 12 migracja SQL Server (zastosowanie, błąd połączenia) | SQL | Brak instancji. Migracja jest wygenerowana (`Migrations/SqlServer`), kolumny `decimal(18,2)` |
| | 11 pusta kolejna migracja SQL Server | S | `MigrationTests.US_01_07_SqlServerMigrations_ExistAndAreUpToDateWithModel` |
| US-01-08 | 1–7 | S | `ConventionTests.US_01_08_*` (foldery, `Vm`, `I…Service`, `BaseService` z polami `protected readonly`, `MainProfile`, `[Required]`, `AssertConfigurationIsValid`), `BaseService.cs` |
| US-01-09 | 1–2 | S | `ProductServiceTests.US_01_09_Interface_DeclaresExpectedMethods`, `US_01_09_ViewModels_HaveExpectedMembers`, `IProductService.cs` |
| | 3, 4 | S | `AddOrUpdateProduct_EmptyOrZeroId_AddsNewRecord`, `AddOrUpdateProduct_ExistingId_ModifiesWithoutCreatingNew` |
| | 5 | S | `AddOrUpdateProduct_UnknownId_ThrowsNotFoundLogsAndAddsNothing` |
| | 6, 7 | S | `AddOrUpdateProduct_Null_ThrowsArgumentNullAndLogs`, `GetProduct_NullFilter_ThrowsArgumentNullAndLogs` |
| | 8, 9, 10 | S | `GetProduct_ByName_…`, `GetProducts_FilterAndNoFilter` (asercja z `_db.Products.Count()`), `GetProduct_NoMatch_ReturnsNull` |
| | 11 filtry cen | S (SQLite) / SQL | `GetProducts_PriceFilters_ReturnOnlyMatchingProducts`; na SQL Server nie ma testu na poziomie usługi (uwaga nr 1) |
| | 12 `ImageBytes` = null zachowuje obraz | S | `ProductServiceTests.US_01_09_AddOrUpdateProduct_NullImage_KeepsExistingImage` i `..._NewImage_ReplacesExistingImage`, `MainProfile.cs:16` (`Condition(s => s.ImageBytes != null)`) |
| US-01-10 | 1 pakiety | S | `WebStore.Tests.csproj`: xunit.v3 4.0.2, Xunit.DependencyInjection 12.0.1, `US_01_10_TestProject_UsesXunitV3` |
| | 2 konfiguracja DI | S | `Startup.cs` (SQLite in-memory z otwartym połączeniem, Identity wg wymagań, AutoMapper/`MainProfile`, `ILogger`, usługi) |
| | 3 dane startowe | S | `Infrastructure/TestData.cs`, `SeedDataTests.US_01_10_*` |
| | 4 `dotnet test` | S | 100 passed, 0 failed, 3 skipped, kod wyjścia 0 |
| | 5 filtr | S | `--filter-class` wspierany przez runner MTP (`global.json`); nie powtórzyłem, bo nie jest sprzeczne z wynikiem pkt 4 |
| | 6 izolacja | S | `Startup.cs` (scoped połączenie i `DbContext` na test), `SeedDataTests` (izolacja per test) |
| | 7 klucze obce | S | `SeedDataTests` (`DbUpdateException` przy FK) |
| | 8 asercja liczby | S | `ProductServiceTests.GetProducts_FilterAndNoFilter`, `OrderServiceTests.GetOrder_AndGetOrders_FilterCorrectly` porównują z `_db…Count()` |
| | 9 testy na SQL Server | SQL | `SqlServerProviderTests` pominięte. Patrz uwaga nr 1 |
| US-01-11 | 1 | S | `OrderServiceTests.US_01_11_Structure_FollowsConventions` |
| | 2 | S | `AddOrUpdateOrder_PersistsItemsWithFrozenCurrentPrice` |
| | 3 | S | `GetOrder_AndGetOrders_FilterCorrectly` |
| | 4 | S | `NullArguments_ThrowArgumentNullAndAreLogged` |
| | 5 | S | `UnknownCustomerOrProduct_FailsAndSavesNothing` |
| | 6 ilość <= 0 | S | `US_01_11_NonPositiveQuantity_IsRejectedWithValidationErrorAndNothingSaved` (Theory), `OrderService.cs:ValidateItems`, `[Range]` w `AddOrUpdateOrderProductVm` |
| | 7 `ProductId` <= 0 lub nieistniejący | S | `US_01_11_InvalidOrUnknownProduct_IsRejectedWithValidationErrorAndNothingSaved` (Theory), `ValidationException` zamiast `KeyNotFoundException` |
| | 8 pusta lista pozycji | S | `US_01_11_EmptyItemList_IsRejectedWithValidationErrorAndNothingSaved` |
| | 9 nieistniejące `Id` zamówienia | S | `AddOrUpdateOrder_UnknownId_ThrowsNotFound` |
| | 10 `TotalAmount` ułamkowy | S (SQLite) / SQL | `TotalAmount_FractionalPrices_IsExactAfterSaveAndLoad` (60,04); SQL Server pominięty (uwaga nr 1) |
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

**1. Testy SQL Server prawdopodobnie nie przejdą po udostępnieniu instancji (ryzyko dla US-01-04 pkt 5, US-01-06 pkt 6, US-01-07 pkt 4, US-01-09 pkt 11, US-01-10 pkt 9, US-01-11 pkt 10).** Stan znany i wstrzymany przez Lidera do zajęć.
- Lokalizacja: `WebStore.Tests/Infrastructure/TestData.cs:21-65`, `WebStore.Tests/SqlServerProviderTests.cs:28`.
- Opis: `TestData.Seed` ustawia jawne wartości kluczy (`Id = 1`, `2` itd.) dla encji, których kolumny w migracji SQL Server są IDENTITY. EF Core nie włącza `IDENTITY_INSERT` przy `SaveChanges`, więc SQL Server zwykle odrzuca takie wstawienie. Nie mam instancji, więc nie mogłem tego sprawdzić.
- Scenariusz awarii: po uruchomieniu testów z `WEBSTORE_TEST_SQLSERVER` trzy testy kończą się błędem w `Open()` i nic nie dowodzą o cenach ani kluczach obcych.
- Dodatkowo: brak testu SQL Server na poziomie usług (filtry cen w `ProductService`, `TotalAmount` w `OrderService`), którego wymagają US-01-09 pkt 11 i US-01-11 pkt 10.
- Kierunek: w teście SQL Server wstawiać dane bez jawnych `Id` (lub użyć `SET IDENTITY_INSERT`), dodać po jednym teście usługowym i zweryfikować to jako pierwszy krok po uzyskaniu instancji.

### Drobne

**2. Powielony warunek w `Program.cs`.**
- Lokalizacja: `WebStore.Web/Program.cs:12-22`.
- Opis: dwa kolejne bloki `if (clientBuilt)` można połączyć w jeden z `else` (strona z instrukcją).
- Kierunek: kosmetyka, bez wpływu na kryteria.

**3. `TotalAmount` faktury jest odświeżany tylko przy zapisie faktury (US-01-04 pkt 7).**
- Lokalizacja: `InvoiceService.cs:51-52`, `OrderService.cs` (brak aktualizacji faktury).
- Opis: po zmianie pozycji zamówienia przez `OrderService` zapisana faktura zachowuje starą kwotę, dopóki nie zostanie ponownie zapisana przez `InvoiceService`. Zgodne z US-01-12 pkt 8 (odświeżenie przy ponownym zapisie), ale „zawsze równe” z US-01-04 pkt 7 nie jest gwarantowane w bazie. Przyjąłem interpretację z US-01-12 pkt 8 jako rozstrzygającą.
- Kierunek: jeżeli Lider chce ścisłej gwarancji, wyliczać kwotę faktury przy odczycie (`Invoice → Order.TotalAmount`) albo aktualizować fakturę w `OrderService`.

## 4. Zakres

Nie stwierdzono implementacji spoza zaakceptowanych user stories. Dodatki wymienione w raporcie implementacji (`Product.ProductStocks`, `Category.Products`, `ProductVm.Id`, `OrderVm.InvoiceId`, `StoreVm.Address`, osobne konteksty `SqliteDbContext`/`SqlServerDbContext`, minimalny klient React) są uzasadnione technicznie i nie wprowadzają nowej funkcjonalności. Zmiana `CLAUDE.md` (struktura, stos) jest zgodna z zakresem.

## 5. Pytania do Lidera

1. Uwaga nr 3: czy wystarczy odświeżanie kwoty faktury przy jej zapisie (bieżące zachowanie), czy kwota ma zawsze wynikać z zamówienia także po zmianie zamówienia bez ponownego zapisu faktury?
