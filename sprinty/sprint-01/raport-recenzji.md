# Raport recenzji — Sprint 01

## 1. Werdykt ogólny

**Zaakceptowano z uwagami.**

Wszystkie 14 user stories otrzymały status `Zweryfikowana`. Brak uwag krytycznych. Części kryteriów dotyczących SQL Server (migracja, testy cen i kluczy obcych na tym providerze) nie da się zweryfikować bez instancji. Lider zna ten stan i zaakceptował go. Uwaga nr 1 pokazuje, że testy SQL Server mogą jednak nie przejść po udostępnieniu instancji.

Uruchomione weryfikacje:
- `dotnet build --no-incremental`: 0 błędów, 0 ostrzeżeń.
- `dotnet test`: 103 testy, 100 zaliczonych, 0 niezaliczonych, 3 pominięte (`SqlServerProviderTests`, brak `WEBSTORE_TEST_SQLSERVER`).
- `dotnet list package --deprecated` i `--vulnerable`: brak przestarzałych i podatnych pakietów.
- `dotnet run --project WebStore.Web --urls http://127.0.0.1:5987`: odpowiedź HTTP 200 na `/`. Treść to komunikat o niezbudowanym kliencie.
- `Database__Provider=Oracle`: start kończy się wyjątkiem „Nieobsługiwany provider bazy danych 'Oracle'… Obsługiwane wartości: Sqlite (SQLite), SqlServer (SQL Server)”.
- Przeszukanie repozytorium (bez `node_modules`, `bin`, `obj`): brak `TODO`/`FIXME`/`XXX`/`TBD`, zakomentowanego kodu, plików `*.bak`/`*_old`/`draft_*`/`tmp`, logów debugowych (`Console.Write`), komentarzy o historii zmian. Brak haseł i ciągów połączenia (test `US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword` oraz przegląd ręczny; w `DesignTimeFactories.cs` jest tylko neutralny ciąg LocalDB z `Trusted_Connection`). `webstore.db` jest ignorowany przez git (`.gitignore: *.db`).
- `git status`: zmiany wyłącznie nieśledzone lub zmodyfikowany `CLAUDE.md`. Niczego nie zacommitowano.

## 2. Weryfikacja kryteriów akceptacji

Skróty: **S** = Spełnione, **SQL** = część dotycząca SQL Server nie do zweryfikowania (brak instancji, stan zaakceptowany przez Lidera), **Odst.** = odstępstwo od treści zatwierdzone decyzją Lidera.

| US | Kryterium | Wynik | Dowód |
|---|---|---|---|
| US-01-01 | 1 projekty w solucji | S | `ArchitectureTests.US_01_01_Solution_ContainsAllProjects`, `WebStore.sln` |
| | 2 referencje | S | `US_01_01_ProjectReferences_MatchLayering` (assembly) i `US_01_01_CsprojFiles_ReferenceExpectedProjects` (csproj), zgodne z `*.csproj` |
| | 3 build 0/0 | S | `dotnet build --no-incremental`: Ostrzeżenia 0, Błędy 0 |
| | 4 start i HTTP 200 | S | `dotnet run`, `curl` zwrócił 200 |
| | 5 jedna wersja platformy i pakiety | S | wszystkie csproj `net10.0` (`US_01_01_AllProjects_ShareTheSameTargetFramework`), EF 10.0.13, AutoMapper 16.2.0, xunit.v3 4.0.2; `--deprecated` i `--vulnerable` czyste |
| | 6 build bez Node.js | Odst. | Treść kryterium (build kończy się błędem bez Node) jest nieaktualna wobec decyzji Lidera: build/test nie wymagają Node.js, a `dotnet publish` przerywa komunikatem o wymaganym Node.js (`WebStore.Web.csproj:35-38`). Patrz uwaga nr 4 |
| | 7 `global.json` | S | `global.json` (SDK 10.0.402), `US_01_01_GlobalJson_PinsSdkMatchingTargetFramework` |
| | 8 DI, brak API/Swagger | S | `ConfigurationTests.US_01_01_AllSprintServices_AreRegisteredAndResolvable` (z `validateScopes`), `US_01_01_WebProject_HasNoApiControllersNorSwagger`, `Program.cs` |
| US-01-02 | 1–6 | S | `ModelTests.US_01_02_*`, `Customer.cs`, `Address.cs`, `StationaryStoreEmployee.cs`, `DbContextTests.US_01_06_SharingAnAddressBetweenCustomers_IsRejected` |
| US-01-03 | 1–3 | S | `ModelTests.US_01_03_*`, `Product.cs`, `Category.cs`, `ProductStock.cs` |
| US-01-04 | 1–3 | S | `ModelTests.US_01_04_*`, `Order.cs:322`, `OrderProduct.cs`, `Invoice.cs` |
| | 4 suma | S | `ModelTests.US_01_04_TotalAmount_IsSumOfFrozenUnitPriceTimesQuantity` |
| | 5 dokładność 2 miejsc | S (SQLite) / SQL | `DbContextTests.US_01_04_OrderWithFractionalPrices_KeepsExactTotalAfterRoundTrip` (60,04); SQL Server: `SqlServerProviderTests` pominięty. Patrz uwaga nr 1 |
| | 6 zamrożona cena | S | `OrderServiceTests.US_01_11_ProductPriceChange_DoesNotAffectExistingOrder` |
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
| | 6 | S | `AddOrUpdateOrder_UnknownId_ThrowsNotFound` |
| | 7 | S (SQLite) / SQL | `TotalAmount_FractionalPrices_IsExactAfterSaveAndLoad` (60,04) |
| | 8 | S | testy obejmują pkt 2–7 i przechodzą |
| US-01-12 | 1–8 | S | `InvoiceServiceTests.US_01_12_*` (w tym drugi zapis faktury dla zamówienia = `InvalidOperationException`, `InvoiceService.cs:544-547`, unikalny indeks w migracji) |
| US-01-13 | 1–6 | S | `StoreServiceTests.US_01_13_*`, `StoreService.cs` |
| US-01-14 | 1–7 | S | `AddressServiceTests.US_01_14_*`, `AddressService.cs` |

## 3. Uwagi

### Krytyczne

Brak.

### Istotne

**1. Testy SQL Server prawdopodobnie nie przejdą po udostępnieniu instancji (ryzyko dla US-01-04 pkt 5, US-01-06 pkt 6, US-01-07 pkt 4, US-01-09 pkt 11, US-01-10 pkt 9, US-01-11 pkt 7).**
- Lokalizacja: `WebStore.Tests/Infrastructure/TestData.cs:21-65`, `WebStore.Tests/SqlServerProviderTests.cs:28`.
- Opis: `TestData.Seed` ustawia jawne wartości kluczy (`Id = 1`, `2` itd.) dla encji, których kolumny w migracji SQL Server są IDENTITY. EF Core nie włącza `IDENTITY_INSERT` przy `SaveChanges`, więc SQL Server zwykle odrzuca takie wstawienie („Cannot insert explicit value for identity column”). Nie mam instancji, więc nie mogłem tego sprawdzić.
- Scenariusz awarii: po uruchomieniu testów z `WEBSTORE_TEST_SQLSERVER` wszystkie trzy testy kończą się błędem w `Open()`. Nie dowodzą wtedy niczego o cenach ani kluczach obcych.
- Dodatkowo: na SQL Server nie ma testu na poziomie usług (filtry cen w `ProductService`, `TotalAmount` w `OrderService`), a kryteria US-01-09 pkt 11 i US-01-11 pkt 7 tego wymagają. Istnieją tylko testy na poziomie `DbContext`.
- Kierunek: w teście SQL Server wstawiać dane bez jawnych `Id` (lub opakować wstawienie w `SET IDENTITY_INSERT`), oraz dodać po jednym teście usługowym `GetProducts(p => p.Price > 600)` i `AddOrUpdateOrder` z cenami ułamkowymi. Zweryfikować to jako pierwszy krok po uzyskaniu instancji.

**2. Brak walidacji ilości w pozycjach zamówienia.**
- Lokalizacja: `WebStore.Services/ConcreteServices/OrderService.cs:602-641`, `WebStore.ViewModels/VM/AddOrUpdateOrderProductVm.cs`.
- Opis: `Quantity` może być 0 lub ujemne, a `Products` może być pustą listą. Nie ma żadnej adnotacji ani sprawdzenia w usłudze.
- Scenariusz awarii: `AddOrUpdateOrder` z `Quantity = -5` zapisuje się i daje ujemne `TotalAmount`, czyli niespójne dane finansowe.
- Kierunek: dodać `[Range(1, int.MaxValue)]` na `Quantity` oraz sprawdzenie w usłudze (np. `ArgumentException` dla `Quantity <= 0`). Kwestię pustej listy rozstrzygnąć z Liderem (patrz pytanie 2).

### Drobne

**3. Wartownik `0` przy wykrywaniu brakującego produktu.**
- Lokalizacja: `OrderService.cs:621-625`.
- Opis: `FirstOrDefault` zwraca 0 zarówno gdy niczego nie brakuje, jak i gdy brakuje produktu o `ProductId = 0`.
- Scenariusz awarii: pozycja z `ProductId = 0` przechodzi walidację, a później `products[productId]` w `SyncItems` rzuca niezwiązany `KeyNotFoundException` z komunikatem o kluczu słownika. Nowe zamówienie pozostaje dodane do śledzonego kontekstu, choć nie zostaje zapisane.
- Kierunek: użyć `Where(...).Cast<int?>().FirstOrDefault()` albo `Except` i sprawdzić, czy wynik jest niepusty.

**4. Nieaktualne kryterium w dokumencie user stories.**
- Lokalizacja: `sprinty/sprint-01/user-stories.md`, US-01-01 pkt 6 (oraz wiersz o Node.js w uwagach).
- Opis: tekst nadal wymaga błędu `dotnet build` bez Node.js, a Lider zdecydował inaczej (błąd dopiero przy `dotnet publish`). Zasada nr 4 z `CLAUDE.md` mówi, że dokumenty opisują stan bieżący.
- Kierunek: Analityk lub Lider aktualizuje treść kryterium. Ja nie zmieniam treści stories, tylko pole `Status`.

**5. Aktualizacja produktu z `ImageBytes = null` kasuje obraz.**
- Lokalizacja: `ProductService.cs:727-729`, `MainProfile.cs:858-862`.
- Opis: `Mapper.Map(model, product)` nadpisuje wszystkie pola, także `ImageBytes`.
- Scenariusz awarii: klient aktualizujący tylko cenę (bez przesyłania obrazu) usuwa zapisany obraz produktu.
- Kierunek: albo przyjąć to jako semantykę „pełnej aktualizacji” i zapisać w dokumentacji, albo mapować `ImageBytes` warunkowo (`Condition(src => src.ImageBytes != null)`).

**6. `TotalAmount` faktury pochodzi od wywołującego i nie jest porównywane z zamówieniem.**
- Lokalizacja: `InvoiceService.cs:552`, `AddOrUpdateInvoiceVm.cs`.
- Opis: zgodnie z decyzją nr 6 programisty. Faktura może mieć kwotę inną niż `Order.TotalAmount`.
- Kierunek: do rozstrzygnięcia przez Lidera (pytanie 3). Specyfikacja US-01-12 tego nie wymaga.

**7. Ostrzeżenie przy starcie bez zbudowanego klienta.**
- Lokalizacja: `WebStore.Web/Program.cs:11`.
- Opis: `UseStaticFiles` loguje „The WebRootPath was not found”, gdy nie ma `wwwroot`.
- Kierunek: kosmetyka (np. warunkowe `UseStaticFiles` tylko gdy klient istnieje). Nie wpływa na kryteria.

## 4. Zakres

Nie stwierdzono implementacji spoza zaakceptowanych user stories. Dodatki wymienione w raporcie implementacji (`Product.ProductStocks`, `Category.Products`, `ProductVm.Id`, `OrderVm.InvoiceId`, `StoreVm.Address`, osobne konteksty `SqliteDbContext`/`SqlServerDbContext`, minimalny klient React) są uzasadnione technicznie i nie wprowadzają nowej funkcjonalności. Zmiana `CLAUDE.md` (struktura, stos) jest zgodna z zakresem.

## 5. Pytania do Lidera

1. Czy mam uznać uwagę nr 1 za zadanie dla programisty już w tym sprincie (poprawka testów SQL Server), czy dopiero po udostępnieniu instancji? Test powinien być sprawdzony na prawdziwym SQL Server przed zamknięciem US-01-07 pkt 10 i 12 oraz US-01-10 pkt 9.
2. Czy zamówienie z pustą listą pozycji ma być poprawne (obecnie jest akceptowane z `TotalAmount` = 0)?
3. Czy `TotalAmount` faktury ma być wyliczane z zamówienia lub walidowane względem niego (uwaga nr 6)?
