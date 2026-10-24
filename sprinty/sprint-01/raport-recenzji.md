# Raport recenzji — Sprint 01

## 1. Werdykt ogólny

**Zaakceptowano z uwagami.**

Wszystkie 17 user stories ma status `Zweryfikowana`. Brak uwag krytycznych i istotnych. US-01-17 (klient React w układzie z instrukcji): 16 z 16 kryteriów spełnionych. Pozostają znane, zaakceptowane przez Lidera uwagi drobne 1-5; uwagi drobne 6 i 7 są naprawione. Próg wersji Node.js jest już dopisany w `CLAUDE.md` (zmiana w katalogu roboczym pojawiła się w trakcie recenzji; nie jest to moja zmiana). macOS i instancja prowadzącego nie były sprawdzane (poza zakresem).

Zakres tej recenzji: US-01-17 (zmienione `package.json` z polem `engines`, `package-lock.json`, `README.md`, `src/main.jsx`, `src/App.jsx`; nieśledzone `src/index.css`, `src/components/NavMenu.jsx`, `src/pages/Home.jsx`, `Counter.jsx`, `NotFound.jsx`), `raport-implementacji.md`, `user-stories.md` oraz regresje. Wcześniejsze zakresy (US-01-07, US-01-10, US-01-15, US-01-16) bez zmian w kodzie od ostatniej recenzji.

### Uruchomione weryfikacje

- `npm ci` i `npm run build` w `WebStore.Web/ClientApp` (Node.js v24.20.0, npm 11.19.0): kod 0, 97 modułów, 0 podatności, bez błędów, ostrzeżeń i EBADENGINE; w `wwwroot` powstają `index.html`, `assets/index-*.js`, `assets/index-*.css`.
- Pole `engines` (`node >=22.22.0`): `npm view react-router engines` -> `>=22.22.0`; w lockfile tylko `react-router` i pakiet główny mają ten próg, pozostałe zależności wymagają mniej (vite i część `@rolldown`/`@oxc`: `^20.19.0 || >=22.12.0`, reszta niżej), więc próg jest minimalnym wspólnym. `git diff`: w `package.json` i `package-lock.json` wyłącznie `engines` oraz zależność `react-router` z jej tranzytywnymi (`@remix-run/route-pattern`, `cookie-es`). Zdanie w `README.md:41` prawdziwe i zwięzłe. `npm ls` spójne z `package.json` (react 19.3.1, react-dom 19.3.1, react-router 8.5.0, vite 8.3.4, @vitejs/plugin-react 6.1.2); `npm audit`: 0 podatności; `npm outdated`: tylko vite 8.3.4 -> 8.3.5 (poprawka patch, opcjonalna, wersje są przypięte); `react-router` 8.5.0 to `latest`.
- `dotnet build -c Release` solucji: 0 błędów, 0 ostrzeżeń (także z PATH bez Node.js: `which node npm` puste, 0/0).
- `dotnet build WebStore.Tests -c Release`: 0 błędów, 0 ostrzeżeń. `dotnet test -c Release --project WebStore.Tests`: Smart App Control zablokował świeży `WebStore.Tests.exe` (kod 1, zero testów) także po ponownym buildzie; uruchomiono `dotnet WebStore.Tests.dll` z `bin\Release\net10.0`: 156 testów, 151 zaliczonych, 0 niezaliczonych, 5 pominiętych (`SqlServerProviderTests`, brak `WEBSTORE_TEST_SQLSERVER`). Testy `ConnectionSecretPattern` i `ArchitectureTests` zaliczone (skan obejmuje zmieniony raport i README). Testów SQL Server nie uruchamiano (kod serwera bez zmian).
- Serwer: sprawdzony w poprzednim przebiegu (`GET /`, `/counter`, nieznany adres -> 200, log bez ostrzeżeń); w ostatnim przeglądzie serwera nie uruchamiano, na porcie 5299 nic nie nasłuchuje (potwierdzone).
- Zachowanie w przeglądarce: panel przeglądarki (`mcp__Claude_Browser__*`) był niedostępny w tej sesji, więc użyto Microsoft Edge w trybie headless sterowanego protokołem DevTools (skrypt tymczasowy, usunięty): renderowanie `/`, `/counter`, nieznanego adresu; kliknięcia w odnośniki i przycisk; zdarzenia sieciowe. Nie jest to panel wskazany w poleceniu, ale to realna przeglądarka z pełnym wykonaniem JS.
- Bez `wwwroot` (tymczasowo przeniesiony poza repozytorium, przywrócony): `GET /` -> 200 z tekstem „Klient React nie jest zbudowany. Uruchom 'npm run build'...”; w logu brak ostrzeżenia o `WebRootPath`. (`GET /counter` bez `wwwroot` daje 404 — jak przed zmianą, kryterium 15 dotyczy tylko `/`.)
- `git status`: poza `sprinty/sprint-01/*`, `README.md` i `CLAUDE.md` (jedna linia o Node.js) zmienione wyłącznie pliki w `WebStore.Web/ClientApp`; serwer, usługi, baza i istniejące testy bez zmian.
- User-secrets: `No secrets configured`; w instancji SQLEXPRESS 0 baz `WebStore*`; `git status` bez plików tymczasowych i kopii; `TODO`/`FIXME`/`XXX`/`TBD` poza opisami zasad i testem konwencji — brak; w `src` brak `fetch`, `XMLHttpRequest`, `axios`, `localStorage`, `sessionStorage`, `console.`.

### Ręczny przegląd pod kątem ciągów połączenia i haseł (całe repozytorium)

Przeszukano (bez rozróżniania wielkości liter) słowa kluczowe ciągów połączenia (źródło danych, serwer, zaufane połączenie, zaufanie certyfikatowi, hasło w obu zapisach, identyfikator użytkownika, zapis host-ukośnik-instancja, uwierzytelnianie zintegrowane).

- `setup.ps1`: ciąg budowany w czasie działania przez `DbConnectionStringBuilder` (nazwy kluczy w kodzie, bez zapisanej wartości z nazwą instancji); brak haseł i loginów.
- `README.md`, `raport-implementacji.md`, `user-stories.md`: ciąg opisany słowami; brak składni ciągu, brak nazwy instancji w składni `host\instancja`. Fragment w składni „Data Source” z raportu implementacji został usunięty (potwierdzone).
- Kod, `appsettings*.json`, testy: jedyne wyjątki to ciąg zapasowy w `DesignTimeFactories.cs:31` (bez nazwy instancji i bez hasła) oraz literały bez instancji i bez hasła w `ConfigurationTests.cs:51` i `MigrationTests.cs:57`. Reszta trafień to źródło danych dla plików SQLite, opcje haseł Identity w `Startup.cs` oraz komentarz szablonu w `.gitignore`. Hasła nie występują.
- Literały w `ArchitectureTests.cs` (dane testowe wzorca) są świadomie pomijane przez sam test; nie są ciągami połączenia z prawdziwymi danymi.

## 2. Weryfikacja kryteriów akceptacji

Skrót: **S** = Spełnione.

### US-01-17

| Kryterium | Wynik | Dowód |
|---|---|---|
| 1 nav z „WebStore”, menu Home i Counter, treść pod paskiem | Spełnione | DOM: jeden `nav` (`NavMenu.jsx`) z `a.brand` -> `/`, `Home` -> `/`, `Counter` -> `/counter`; `main` po `nav` (`App.jsx:10-17`) |
| 2 nawigacja bez przeładowania | Spełnione | Edge/DevTools: znacznik `window.__mark` przetrwał kliknięcie „Counter” i „Home”; adres `/counter`, potem `/`; kliknięcie „WebStore” wraca na `/`; w zdarzeniach sieciowych po kliknięciach brak nowych żądań `Document` |
| 3 bezpośrednie wejście i odświeżenie `/counter` | Spełnione | `curl` -> 200; render po wejściu i po `Page.reload`: „Counter” z paskiem nawigacji |
| 4 nieznany adres | Spełnione | `/nie-ma-takiej-strony` -> 200, `nav`, `h1` „Nie znaleziono strony”, odnośnik „Wróć do strony głównej” (`NotFound.jsx`) |
| 5 treść strony głównej | Spełnione | `Home.jsx`: `h1` „Witaj w WebStore”, opis projektu, 6 warstw z opisami, 5 technologii (.NET, Entity Framework Core, AutoMapper, xUnit, React), po polsku |
| 6 brak informacji o środowisku | Spełnione | treść stała w kodzie; brak provider-a, ciągu połączenia, liczby rekordów |
| 7 strona Counter | Spełnione | `h1` „Counter”, opis, „Aktualna wartość: 0”, przycisk „Zwiększ” |
| 8 trzy kliknięcia | Spełnione | kolejno 1, 2, 3 (`setCount(v => v + 1)`) |
| 9 stan lokalny | Spełnione | po „Home” i powrocie 0; po przeładowaniu 0; `localStorage` i `sessionStorage` puste (długość 0) |
| 10 brak API | Spełnione | żądania: tylko `Document`, `Script`, `Stylesheet` i `favicon.ico` (zasób statyczny); `grep` po `src`: brak `fetch`/XHR/bibliotek HTTP; zależności bez biblioteki HTTP; serwer bez zmian |
| 11 zakres diffu | Spełnione | `git status`: tylko `WebStore.Web/ClientApp`, `sprinty/sprint-01` oraz zdanie o wersji Node.js w `README.md` (wynika z uwagi recenzji, dopuszczone przez raport implementacji) |
| 12 dostępność i język | Spełnione | `lang="pl"`, `title` „WebStore”, jeden `nav` (z `aria-label`), jeden `main`, dokładnie jeden `h1` na Home, Counter i stronie błędu; elementy `a` i `button` (`type="button"`) |
| 13 `npm run build` | Spełnione | `npm ci` i `npm run build` kod 0, bez ostrzeżeń i EBADENGINE; `wwwroot/index.html` i `assets/*` |
| 14 `GET /` po buildzie | Spełnione | 200, `index.html` SPA ze skryptem i stylem; w pliku JS występują „Counter” (6x), „Zwiększ”, „Aktualna wartość” |
| 15 bez zbudowanego klienta | Spełnione | po przeniesieniu `wwwroot`: `GET /` 200 z instrukcją, brak ostrzeżenia `WebRootPath` |
| 16 build i test bez Node.js | Spełnione | PATH bez Node.js: `dotnet build -c Release` 0/0; testy 156 (151 zaliczonych, 5 pominiętych); `csproj` wywołuje npm tylko w celu `PublishClient` przed `ComputeFilesToPublish` |

Wynik US-01-17: 16 spełnionych, 0 niespełnionych, 0 niemożliwych do zweryfikowania.

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
| US-01-01 – US-01-06, US-01-08, US-01-09, US-01-11 – US-01-14 | wszystkie | S | testy `US_01_*` zaliczone w Release (156 testów, 151 zaliczonych, 5 pominiętych bez zmiennej); build 0/0; `GET /` -> 200 ze zbudowanym klientem i bez niego (US-01-01 pkt 4 i 6) |

## 3. Uwagi

### Krytyczne

Brak.

### Istotne

Brak. Dawne uwagi istotne 1 i 2 (siła testów `Get…`/`GetMultiple…` oraz `Update…`) są naprawione, patrz niżej.

### Naprawione wcześniej (tylko `WebStore.Tests/UnitTests`)

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

**6. Wymóg Node.js >= 22.22.0 dla `react-router` 8.5.0 — naprawione.**
- `package.json` i `package-lock.json` mają `"engines": {"node": ">=22.22.0"}` (próg poprawny i minimalny wspólny), `README.md:41` zawiera zdanie o minimalnej wersji; `npm ci` i `npm run build` na Node v24.20.0 bez EBADENGINE.
- `CLAUDE.md` (linia o Node.js) zawiera już próg 22.22.0 i odwołanie do `engines` (zmiana dopisana w katalogu roboczym w trakcie recenzji, poza zakresem deklarowanym przez programistę; spójna z `package.json`). `setup.sh` i `setup.ps1` sprawdzają tylko obecność Node.js, nie wersję (znane ograniczenie, odnotowane w raporcie implementacji); to decyzja, nie wada.

**7. Niespójności dokumentacji — naprawione.**
- `raport-implementacji.md` przejrzany w całości: akapity historyczne usunięte, nie ma sformułowań typu „w tej iteracji”, „po poprawkach”, „dotychczas”, „dawniej”; zachowane mapowanie kryteriów (US-01-01 do US-01-17), lista plików, uruchomienie, wyniki, decyzje oraz stan SQL Server i user-secrets. Liczby aktualne (156 / 151 zaliczonych / 5 pominiętych bez zmiennej; 156 zaliczonych z `WEBSTORE_TEST_SQLSERVER`). Brak ciągu połączenia i haseł (test architektury zaliczony). Resztkowe zdania opisowe o weryfikacji (np. mutacje cofnięte, baza `WebStore` usunięta po teście) opisują stan końcowy, nie historię zmian dokumentu.
- Podniesienie vite do 8.3.5 było tylko opcjonalne i nie zostało wykonane (wersja przypięta, `npm audit` 0 podatności); nie jest wadą.

## 4. Zakres

Nie stwierdzono implementacji spoza zaakceptowanych user stories. Zmiany klienta React, `package.json`, `package-lock.json`, zdania w README i dokumentów mieszczą się w US-01-17; jedyna nowa zależność to `react-router` (uzasadniona w uwagach story); brak nowych plików poza kodem klienta, serwer, usługi, baza i testy bez zmian. Wcześniejsze poprawki (Release w `setup.ps1`, wzorzec w teście architektury, `RepositoryFiles`) mieszczą się w US-01-07, US-01-10 i US-01-15.

Poprawność poszczególnych punktów:
- `setup.ps1`: zmiana ogranicza się do dodania `-c Release` w krokach build i test (z komentarzem o Smart App Control); kolejność kroków, podsumowanie i kod wyjścia bez zmian; krok SQL Server wykonuje się po kroku testów i nie zależy od jego wyniku. Plik zachował BOM (`EF BB BF`) i zakończenia linii CRLF (wszystkie 209 linii). README opisuje powód Release.
- `RepositoryPaths.RepositoryFiles()`: separator ścieżek normalizowany tylko w Windows (na macOS ścieżki z `/` używane bez zmian); wyłączenia: `bin`, `obj`, `node_modules`, `.git`, `wwwroot`, `wymagania`, `.claude`; skan obejmuje `sprinty`. Test wykryłby fragment typu usuniętego z raportu implementacji (przypadek `Data Source` z `host\instancja` jest w `DetectsConnectionStrings`), a ten raport go nie zawiera.
- `user-stories.md`: US-01-15 pkt 10 bez liczby testów, sens niezmieniony; statusy wszystkich 17 story: `Zweryfikowana` (US-01-17 ustawiona w tej recenzji).
- `raport-implementacji.md`: wiersze US-01-17 i wyniki (build 0, 156 testów: 151 zaliczonych i 5 pominiętych, blokada Smart App Control i obejście `dotnet WebStore.Tests.dll`) zgodne z moimi wynikami; raport opisuje stan bieżący; otwarte kwestie: instancja prowadzącego, macOS i próg wersji Node.js w `CLAUDE.md` (pytanie 1).

## 5. Wpływ ograniczenia środowiska (Code Integrity)

Dotychczasowy problem (Smart App Control blokuje ładowanie bibliotek z `bin\Debug`, więc krok testów w `setup.ps1` kończył się błędem) został usunięty przez użycie Release w krokach build i test. Czysty `dotnet test` w Debug nadal może zawieść na komputerach z tą blokadą; README i raport implementacji to opisują. Prawdziwego `setup.ps1` bez `-CheckOnly` nie uruchamiałem (zgodnie z poleceniem), więc zachowanie pełnego przebiegu wynika z analizy diffu (zmienione wyłącznie argumenty dwóch kroków) i z przebiegu `dotnet test -c Release`.

## 6. Pytania do Lidera

1. Potwierdzenie: w katalogu roboczym `CLAUDE.md` ma już zapis „Node.js (LTS, minimum 22.22.0 ...)”. Czy to zamierzona decyzja Lidera (wtedy pytanie zamknięte)? `setup.sh` (`node@22`, macOS niezweryfikowany) i `setup.ps1` nie sprawdzają wersji — czy zostawić jako znane ograniczenie?

Do ustalenia poza repozytorium pozostają: zachowanie na instancji SQL Server prowadzącego (inna wersja, logowanie hasłem) oraz na macOS; nie były weryfikowane.
