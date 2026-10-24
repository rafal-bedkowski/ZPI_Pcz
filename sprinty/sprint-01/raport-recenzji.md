# Raport recenzji — Sprint 01

## 1. Werdykt ogólny

**Zaakceptowano z uwagami.**

Wszystkie 16 user stories ma status `Zweryfikowana`. Brak uwag krytycznych i istotnych. Uwagi dotyczące siły nowych testów (US-01-16) zostały naprawione i potwierdzone próbą mutacyjną; pozostają znane, zaakceptowane przez Lidera uwagi drobne. macOS i instancja prowadzącego nie były sprawdzane (poza zakresem).

Zakres tej recenzji: US-01-16 (nieśledzone `WebStore.Tests/Extensions.cs` i `WebStore.Tests/UnitTests/*`, zmieniony `Startup.cs`, sekcja „Testy” w `README.md`, `raport-implementacji.md`, `user-stories.md`) oraz regresje w pozostałych story. Wcześniejsze zakresy (US-01-07, US-01-10, US-01-15) bez zmian od ostatniej recenzji.

### Uruchomione weryfikacje

- `dotnet build WebStore.Tests/WebStore.Tests.csproj -c Release --no-incremental`: 0 błędów, 0 ostrzeżeń (projektu Web w Release nie przebudowywano, zgodnie z poleceniem).
- `dotnet test -c Release --project WebStore.Tests` bez zmiennej: 156 testów, 151 zaliczonych, 0 niezaliczonych, 5 pominiętych (`SqlServerProviderTests`), kod wyjścia 0.
- To samo z `WEBSTORE_TEST_SQLSERVER` (lokalna instancja SQLEXPRESS, uwierzytelnianie Windows, zaufanie certyfikatowi; zmienna tylko na czas polecenia): 156 zaliczonych, 0 pominiętych, kod 0. Po przebiegu w instancji są tylko bazy systemowe (0 baz `WebStore*`).
- Filtry (Release): `--filter-method` z pełną nazwą (przestrzeń nazw, klasa, metoda) `UpdateProductTest` daje 1 test, kod 0; `--filter-class` z pełną nazwą `ProductServiceUnitTests` daje 5 testów, kod 0; `--filter-method UpdateProductTest` (sama krótka nazwa) daje 0 testów, kod 8; `--filter-class ProductServiceUnitTests` (krótka nazwa) daje 0 testów, kod 8; `--filter UpdateProductTest` daje 1 test, kod 0; `--filter ProductServiceUnitTests` daje 5 testów, kod 0. Zgodne z README i raportem implementacji.
- User-secrets: `No secrets configured` (puste, nie ruszane).
- `git diff` w `WebStore.Tests`: z dotychczasowych plików zmieniony wyłącznie `Startup.cs` (usunięty `using` i przeniesienie rejestracji kontekstu do `Extensions.SeedData`, logika identyczna); `ConventionTests` i `ArchitectureTests` bez zmian; kod produkcyjny bez zmian. 131 dotychczasowych + 25 nowych = 156. Brak duplikatów pełnych nazw klas.
- `git status`: tylko zmiany tej iteracji; brak plików tymczasowych i kopii zapasowych; `TODO`/`FIXME`/`XXX`/`TBD` tylko w opisach zasad (`CLAUDE.md`, `.claude/agents`) i w teście konwencji; brak zakomentowanego kodu w nowych plikach (komentarze opisują działanie).

### Ręczny przegląd pod kątem ciągów połączenia i haseł (całe repozytorium)

Przeszukano (bez rozróżniania wielkości liter) słowa kluczowe ciągów połączenia (źródło danych, serwer, zaufane połączenie, zaufanie certyfikatowi, hasło w obu zapisach, identyfikator użytkownika, zapis host-ukośnik-instancja, uwierzytelnianie zintegrowane).

- `setup.ps1`: ciąg budowany w czasie działania przez `DbConnectionStringBuilder` (nazwy kluczy w kodzie, bez zapisanej wartości z nazwą instancji); brak haseł i loginów.
- `README.md`, `raport-implementacji.md`, `user-stories.md`: ciąg opisany słowami; brak składni ciągu, brak nazwy instancji w składni `host\instancja`. Fragment w składni „Data Source” z raportu implementacji został usunięty (potwierdzone).
- Kod, `appsettings*.json`, testy: jedyne wyjątki to ciąg zapasowy w `DesignTimeFactories.cs:31` (bez nazwy instancji i bez hasła) oraz literały bez instancji i bez hasła w `ConfigurationTests.cs:51` i `MigrationTests.cs:57`. Reszta trafień to źródło danych dla plików SQLite, opcje haseł Identity w `Startup.cs` oraz komentarz szablonu w `.gitignore`. Hasła nie występują.
- Literały w `ArchitectureTests.cs` (dane testowe wzorca) są świadomie pomijane przez sam test; nie są ciągami połączenia z prawdziwymi danymi.

## 2. Weryfikacja kryteriów akceptacji

Skrót: **S** = Spełnione.

### US-01-15

| Kryterium | Wynik | Dowód |
|---|---|---|
| 1 wykrycie usługi i pytanie `[t/N]` | S | `setup.ps1` (`Get-Service`, `Confirm-Action` z domyślnym „nie”); poprzednia recenzja: scenariusze na kopiach skryptu, krok SQL Server niezmieniony w tej iteracji |
| 2 ustawienie user-secrets, ciąg składany w czasie działania | S | `setup.ps1:178-188`; brak zmian logiki od poprzedniej recenzji |
| 3 osobne pytanie o nadpisanie | S | `setup.ps1:166-176`; brak zmian logiki |
| 4 odpowiedź inna niż `t` | S | `setup.ps1:162-163`; brak zmian logiki |
| 5 brak usługi | S | `setup.ps1:149-150`; diff względem poprzedniej wersji zmienia tylko argumenty kroków build i test |
| 6 `-CheckOnly` | S | prawdziwe uruchomienie: kod 0, brak pytań, user-secrets bez zmian; `SetupScriptTests.US_01_15_CheckOnly_DoesNotAskAndDoesNotChangeUserSecrets` zaliczony |
| 7 brak ciągu połączenia w plikach | S | test architektury zaliczony (skan obejmuje `sprinty`), ręczny przegląd całego repozytorium czysty (sekcja 1) |
| 8 `dotnet run` na SQL Server | S | sprawdzone w poprzedniej recenzji; brak zmian w kodzie aplikacji |
| 9 `dotnet ef database update` | S | sprawdzone w poprzedniej recenzji; brak zmian w migracjach i fabryce poza opisanymi |
| 10 testy SQL Server z `WEBSTORE_TEST_SQLSERVER` | S | 156/156 zaliczonych, 0 pominiętych w Release. Treść kryterium bez liczby testów; sens bez zmian (testy SQL Server nie są pomijane, wszystkie „Passed”) |
| 11 bez konfiguracji SQLite | S | `ConfigurationTests.US_01_07_NoProviderConfigured_UsesSqlite`, `SetupScriptTests.US_01_07_AppSettings_DefaultProviderIsSqlite` |
| 12 README: Windows i macOS | S | README „Baza danych”; `SetupScriptTests.US_01_15_Readme_*` zaliczone; README zawiera krótkie wyjaśnienie Release i Smart App Control |
| 13 README: ręczna konfiguracja innej instancji | S | README „Windows: ręczna konfiguracja innej instancji SQL Server”; brak nieaktualnych zdań „do ustalenia z prowadzącym” |

### US-01-07 (zmienione kryteria)

| Kryterium | Wynik | Dowód |
|---|---|---|
| 1 domyślnie SQLite | S | `ConfigurationTests.US_01_07_NoProviderConfigured_UsesSqlite` |
| 2 SQL Server z konfiguracji | S | `US_01_07_SqlServerConfigured_UsesSqlServerWithoutChangingServices` |
| 3 nieobsługiwany provider | S | `US_01_07_UnsupportedProvider_FailsListingSupportedValues` |
| 4 operacje na cenach | S | `DbContextTests.US_01_07_PriceOperations_GiveCorrectResultsOnSqlite`; `SqlServerProviderTests.US_01_10_SqlServer_PriceOperationsMatchSqlite` (zaliczony na SQLEXPRESS) |
| 5 brak ciągu połączenia i haseł w plikach (z README i skryptami) | S | test architektury i testy wzorca zaliczone; ręczny przegląd czysty; fragment w raporcie implementacji usunięty |
| 6 `Database:Provider` = `Sqlite` w `appsettings.json` | S | `SetupScriptTests.US_01_07_AppSettings_DefaultProviderIsSqlite` |
| 7 migracja SQLite tworzy plik i tabele | S | `MigrationTests.US_01_07_SqliteMigration_CreatesDatabaseFileWithAllTablesAndForeignKeys` |
| 8 `.gitignore` | S | `ArchitectureTests.US_01_07_GitIgnore_ExcludesSqliteDatabaseFile` |
| 9 pusta kolejna migracja SQLite | S | `MigrationTests.US_01_07_SqliteMigrations_AreUpToDateWithModel` |
| 10 ścieżka niedostępna do zapisu | S (wg raportu) | brak testu automatycznego; ręczny test z raportu implementacji nie został przeze mnie powtórzony |
| 11 `database update` SQL Server | S | sprawdzone w poprzedniej recenzji |
| 12 pusta kolejna migracja SQL Server | S | `MigrationTests.US_01_07_SqlServerMigrations_ExistAndAreUpToDateWithModel` |
| 13 serwer niedostępny | S | sprawdzone w poprzedniej recenzji |

### US-01-16

| Kryterium | Wynik | Dowód |
|---|---|---|
| 1 `BaseUnitTests` | Spełnione | `UnitTests/BaseUnitTests.cs:5-13`: abstrakcyjna, przestrzeń `WebStore.Tests.UnitTests`, publiczny konstruktor z `ApplicationDbContext`, `protected readonly DbContext` |
| 2 dokładnie pięć klas `<Usługa>UnitTests` | Spełnione | w folderze 6 plików (Base + 5); każda dziedziczy po `BaseUnitTests`, konstruktor `(ApplicationDbContext, I<Usługa>)`; wstrzykiwanie działa (testy przechodzą) |
| 3 `Extensions.SeedData` | Spełnione | `Extensions.cs:9`, wywołanie w `Startup.cs` (`services.SeedData()`) |
| 4 dane z instrukcji | Spełnione | `TestData.Seed` (dostawca 1, kategoria 1, „Monitor Dell 32” Id 1 cena 1000, „Mysz Logitech” Id 2 cena 500); `SeedDataTests` zaliczony bez zmian; `GetProductTest`, `GetMultipleProductsTest` |
| 5 jedno źródło danych | Spełnione | `Extensions.cs:21` wywołuje `TestData.Seed`; brak drugiej definicji danych; testy US-01-08–14 i `SeedDataTests` zaliczone bez zmian oczekiwań |
| 6 izolacja per test | Spełnione | połączenie SQLite i kontekst `AddScoped` w tym samym zakresie (`Startup.cs`, `Extensions.cs`); `SeedDataTests.US_01_10_Database_IsolatedPerTest_*` zaliczone; nowe testy zapisujące dane zaliczone w obu uruchomieniach |
| 7 `ProductServiceUnitTests` | Spełnione | pięć metod o wymaganych nazwach i scenariuszach; `GetAllProductsTest` porównuje z `DbContext.Products.Count()` (`ProductServiceUnitTests.cs:38`); Add: „MacBook Pro”, `CategoryId` 1, `SupplierId` 1, liczba +1; Update: Id 1, cena 2000, nazwa bez zmian, liczba bez zmian (Get…/GetMultiple… z rekordem spoza filtra; Add…/Update… weryfikowane odczytem z bazy po `ChangeTracker.Clear()`) |
| 8 cztery pozostałe klasy | Spełnione | po pięć metod o nazwach z kryterium (np. `GetMultipleInvoicesTest`, `GetAllAddressesTest`), nazwy sprawdzone w kodzie |
| 9 tylko ścieżki pozytywne, asercje zgodne z kryterium | Spełnione | `GetAll…` porównują z `DbContext.<Zbiór>.Count()` (nie samo ze sobą); `AddNew…` sprawdza zwrócone dane i wzrost o 1; `Update…` sprawdza zwrócone dane, brak zmiany liczby i stan z bazy; zamówienia i faktury tworzone w teście |
| 10 zgodność z faktycznymi zasadami usług | Spełnione | brak testów `null` i wyjątków w nowych klasach; scenariusze negatywne zostają w istniejących testach |
| 11 `dotnet test` | Spełnione | 156 testów, 0 niezaliczonych, 5 pominiętych bez zmiennej, kod 0; ze zmienną 156/156 |
| 12 brak regresji | Spełnione | `git diff`: dotychczasowe pliki testów niezmienione (poza `Startup.cs`); 131 + 25 = 156, wszystkie zaliczone |
| 13 build 0/0, brak duplikatów klas | Spełnione | build `--no-incremental` 0/0; skan nazw klas w projekcie nie wykazał duplikatów |
| 14 testy konwencji i architektury | Spełnione | `ConventionTests`, `ArchitectureTests` bez zmian i zaliczone; nowe metody bez identyfikatorów US |
| 15 filtry | Spełnione | przebiegi filtrów w sekcji 1 (pełna nazwa działa, krótka nie wybiera testów; kod 8) |
| 16 README | Spełnione | README „Testy”: wszystkie, metoda, klasa (`--filter-method`, `--filter-class`); README bez ciągu połączenia i haseł (test architektury zaliczony, przegląd ręczny) |
| 17 mapowanie w raporcie implementacji | Spełnione | `raport-implementacji.md`: wiersze US-01-16 pkt 1–17 z testami lub sprawdzeniami i mapowaniem metod na kryteria US-01-09, 11–14 |

Wynik US-01-16: 17 spełnionych, 0 niespełnionych, 0 niemożliwych do zweryfikowania.

### US-01-10 i regresje

| US | Kryterium | Wynik | Dowód |
|---|---|---|---|
| US-01-10 | 1–9 | S | `SqlServerProviderTests` (5 testów) zaliczone ze zmienną; 156/156; `dotnet test -c Release` kod 0 |
| US-01-01 – US-01-06, US-01-08, US-01-09, US-01-11 – US-01-14 | wszystkie | S | testy `US_01_*` zaliczone w Release; build 0/0 |

## 3. Uwagi

### Krytyczne

Brak.

### Istotne

Brak. Dawne uwagi istotne 1 i 2 (siła testów `Get…`/`GetMultiple…` oraz `Update…`) są naprawione, patrz niżej.

### Naprawione w tej iteracji (tylko `WebStore.Tests/UnitTests`)

- Dawna istotna 1 (filtr w `Get…`/`GetMultiple…`): każdy test ma rekord spoza filtra lub szuka elementu innego niż pierwszy (`GetProductTest`, `GetStoreTest`, `GetOrderTest`, `GetInvoiceTest`; `GetAddressTest` szuka adresu o Id 3; `GetMultiple…` z trzecim rekordem i `DoesNotContain`). Próba mutacyjna: zignorowanie filtra w `ProductService` i `OrderService` (`FirstOrDefault()` i brak `Where`) zawiodło w `GetProductTest`, `GetMultipleProductsTest`, `GetOrderTest`, `GetMultipleOrdersTest`. Pozostałych trzech usług nie mutowałem; wniosek z czytania kodu testów (ten sam wzorzec).
- Dawna istotna 2 (`Update…`): `ChangeTracker.Clear()`, odczyt `AsNoTracking`, asercje pól i liczby rekordów. Próba mutacyjna: pominięcie `SaveChanges()` w `ProductService` i `OrderService` zawiodło w `UpdateProductTest` i `UpdateOrderTest` (oraz w `AddNew…Test` obu usług).
- Dawna drobna 1 (`AddNew…`): odczyt z bazy po `ChangeTracker.Clear()`; produkt: opis, cena, waga, kategoria, dostawca; zamówienie: daty, pozycja, ilość, cena jednostkowa, `TotalAmount`; faktura: kwota, daty, zamówienie; sklep i adres: wszystkie pola.
- Po próbie pliki usług przywrócone z kopii; `git status` i `git diff` dla `WebStore.Services`, `WebStore.DAL`, `WebStore.Web`, `WebStore.Model`, `WebStore.ViewModels` czyste (kod produkcyjny bez zmian).
- Odporność: testy nie zależą od kolejności ani od stałych Id nadanych przez bazę (Id pobierane ze zwróconych lub zapisanych encji, stałe tylko dla danych startowych), nie dzielą stanu (baza per test); przechodzą także na SQL Server.

### Drobne

**1. Ciąg zapasowy fabryki wskazuje domyślną instancję lokalną (znane, zaakceptowane przez Lidera bez zmian).**
- Lokalizacja: `WebStore.DAL/EF/DesignTimeFactories.cs:31`.
- Opis: bez konfiguracji `dotnet ef database update --context SqlServerDbContext` łączy się z domyślną instancją `localhost`. Ciąg nie zawiera nazwy instancji ani hasła.
- Kierunek poprawki (opcjonalnie): niewiarygodny host zapasowy lub odmowa przy `database update` bez konfiguracji.

**2. Zduplikowany `UserSecretsId` (znane, zaakceptowane przez Lidera bez zmian).**
- Lokalizacja: `WebStore.DAL/WebStore.DAL.csproj:25` i `WebStore.Web/WebStore.Web.csproj`.
- Opis: zmiana w jednym pliku bez drugiego po cichu rozdzieli konfigurację.
- Kierunek poprawki (opcjonalnie): test równości identyfikatorów lub wspólna właściwość.

**3. Brak automatycznego testu logiki pytań `setup.ps1` (znane, zaakceptowane przez Lidera bez zmian).**
- Lokalizacja: `WebStore.Tests/SetupScriptTests.cs`, `setup.ps1:147-198`.
- Opis: automatyczny test obejmuje tylko `-CheckOnly`, README i domyślny provider; kryteria 1–5 US-01-15 były sprawdzane ręcznie na kopiach skryptu. Zmiana kolejności pytań lub nadpisanie bez pytania nie zostanie wykryta przez `dotnet test`.

**4. Wzorzec `ConnectionSecretPattern` może dawać fałszywe alarmy na zwykłym przypisaniu wartości do pola hasła w kodzie.**
- Lokalizacja: `WebStore.Tests/ArchitectureTests.cs:130`.
- Opis: wzorzec hasła wymaga tylko, by przed nazwą nie stał znak alfanumeryczny ani podkreślenie. Zwykłe przypisanie w C# (inicjalizator obiektu lub przypisanie do właściwości o nazwie hasła, ze spacją przed znakiem równości) dopasuje się, bo kropka lub spacja przed nazwą nie blokuje dopasowania. Obecnie żaden plik nie jest oznaczany (testy `IgnoresOrdinaryCode` obejmują `PasswordHash` i `o.Password.RequiredLength`, ale nie przypisanie wartości do takiej właściwości). Dla loginu (`User Id`) wzorzec jest już poprawnie zawężony do kontekstu po separatorze lub cudzysłowie.
- Scenariusz: przyszły kod seedujący lub rejestracji z takim inicjalizatorem zepsuje test architektury mimo braku ciągu połączenia.
- Kierunek poprawki: dla hasła wymagać kontekstu ciągu połączenia (separator `;` lub cudzysłów przed nazwą, albo obecność `Server`/`Data Source` w tej samej linii).

**5. Wzorzec nie łapie wszystkich form ciągu połączenia (zgodne z założeniem, tylko odnotowane).**
- Lokalizacja: `WebStore.Tests/ArchitectureTests.cs:128-133`.
- Opis: serwer bez nazwy instancji i bez hasła (np. adres z portem lub prefiks `tcp:`) nie jest wykrywany, a login zapisany bez poprzedzającego separatora lub cudzysłowa (np. na początku linii) też nie. Nazwa testu („ContainsNoSqlServerConnectionStringOrPassword”) obiecuje nieco więcej niż pokrywa wzorzec. Wzorzec poprawnie wykrywa przypadki z kryteriów: hasło, login po separatorze i `host\instancja` (także z podwójnym ukośnikiem w tekście JSON/C#); skan potwierdzony w testach `DetectsConnectionStrings` (6 przypadków) i `IgnoresOrdinaryCode` (7 przypadków).
- Kierunek poprawki (opcjonalnie): doprecyzować nazwę lub komentarz do zakresu wzorca.

## 4. Zakres

Nie stwierdzono implementacji spoza zaakceptowanych user stories. Zmiany tej iteracji (`Extensions`, `UnitTests`, `Startup`, README, dokumenty) mieszczą się w US-01-16; kod produkcyjny bez zmian. Wcześniejsze poprawki (Release w `setup.ps1`, wzorzec w teście architektury, `RepositoryFiles`) mieszczą się w US-01-07, US-01-10 i US-01-15.

Poprawność poszczególnych punktów:
- `setup.ps1`: zmiana ogranicza się do dodania `-c Release` w krokach build i test (z komentarzem o Smart App Control); kolejność kroków, podsumowanie i kod wyjścia bez zmian; krok SQL Server wykonuje się po kroku testów i nie zależy od jego wyniku. Plik zachował BOM (`EF BB BF`) i zakończenia linii CRLF (wszystkie 209 linii). README opisuje powód Release.
- `RepositoryPaths.RepositoryFiles()`: separator ścieżek normalizowany tylko w Windows (na macOS ścieżki z `/` używane bez zmian); wyłączenia: `bin`, `obj`, `node_modules`, `.git`, `wwwroot`, `wymagania`, `.claude`; skan obejmuje `sprinty`. Test wykryłby fragment typu usuniętego z raportu implementacji (przypadek `Data Source` z `host\instancja` jest w `DetectsConnectionStrings`), a ten raport go nie zawiera.
- `user-stories.md`: US-01-15 pkt 10 bez liczby testów, sens niezmieniony; statusy wszystkich 16 story: `Zweryfikowana`.
- `raport-implementacji.md`: stan faktyczny zgodny z moimi wynikami (156 testów: 151 zaliczonych i 5 pominiętych bez zmiennej; 156/156 ze zmienną); jedyne otwarte kwestie to niezweryfikowana instancja prowadzącego i macOS.

## 5. Wpływ ograniczenia środowiska (Code Integrity)

Dotychczasowy problem (Smart App Control blokuje ładowanie bibliotek z `bin\Debug`, więc krok testów w `setup.ps1` kończył się błędem) został usunięty przez użycie Release w krokach build i test. Czysty `dotnet test` w Debug nadal może zawieść na komputerach z tą blokadą; README i raport implementacji to opisują. Prawdziwego `setup.ps1` bez `-CheckOnly` nie uruchamiałem (zgodnie z poleceniem), więc zachowanie pełnego przebiegu wynika z analizy diffu (zmienione wyłącznie argumenty dwóch kroków) i z przebiegu `dotnet test -c Release`.

## 6. Pytania do Lidera

Brak otwartych pytań. Do ustalenia poza repozytorium pozostają: zachowanie na instancji SQL Server prowadzącego (inna wersja, logowanie hasłem) oraz na macOS; nie były weryfikowane.
