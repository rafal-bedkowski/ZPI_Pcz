# Raport recenzji — Sprint 02

Zakres: stan katalogu roboczego względem HEAD = 8e2dc1a (poprawki po sprincie A, B, C opisane w `raport-implementacji.md`, sekcja 7; bez nowych user stories, oceniane względem ustaleń Lidera, jakości, bezpieczeństwa i czystości kodu) oraz US-02-01 do US-02-13.

## 1. Werdykt ogólny

**Zaakceptowano z uwagami.** Brak uwag krytycznych. Wszystkie kryteria akceptacji są spełnione. Kryteria zależne od SQL Server (US-02-02 pkt 8, US-02-06 pkt 10) są zweryfikowane na lokalnym SQL Server Express.

Uruchomienia (318 testów w każdej konfiguracji):
- `dotnet build -c Release`: 0 ostrzeżeń, 0 błędów.
- `dotnet test -c Release` bez `WEBSTORE_TEST_SQLSERVER`: powodzenie 310, niepowodzenie 0, pominięte 8 (testy SQL Server), 25 s.
- `dotnet test -c Release` z `WEBSTORE_TEST_SQLSERVER=Server=localhost\SQLEXPRESS;Integrated Security=true;TrustServerCertificate=true;` (ustawioną tylko na czas procesu): powodzenie 318, niepowodzenie 0, pominięte 0, 31 s.
- Ręcznie: start w środowisku Production bez `JwtOptions:SecretKey` kończy się `OptionsValidationException` z polskim komunikatem wskazującym `JwtOptions:SecretKey` (US-02-02 pkt 6); start w Development bez hasła kont testowych loguje polskie ostrzeżenie i nie przerywa startu (US-02-03 pkt 3).

Podsumowanie kryteriów: 13 US, 0 odrzuconych; kryteria: Spełnione — wszystkie; Niespełnione 0, Nie da się zweryfikować 0 (US-02-12 pkt 1–4 zweryfikowane tylko przeglądem konfiguracji, patrz tabela).

Poprawki po sprincie: A (409 dla duplikatu faktury) i B (SQL Server jako baza domyślna) zgodne z ustaleniami Lidera; C (limit 120 s) działa, ale nie eliminuje znanego ograniczenia środowiska (uwaga 3).

## 2. Weryfikacja kryteriów

Dowody: klasy testów w `WebStore.Tests` (wszystkie przechodzą), pliki w `WebStore.Web`. `Api/*` = `WebStore.Tests/Api`.

| US | Kryterium | Wynik | Dowód |
|---|---|---|---|
| US-02-01 | 1 BaseApiController (abstrakcyjna, `[ApiController]`, `[Produces]`, `api/[controller]`, chronione readonly `Logger`/`Mapper`) | Spełnione | `Controllers/BaseApiController.cs:9-22`; `ControllerConventionTests` |
| | 2 Content-Type JSON / problem+json, 204 bez treści | Spełnione | `HostingTests.ErrorAndNoContentResponses_HaveExpectedContentTypes` |
| | 3 `/api/NieIstniejacyKontroler` → 404, nie index.html | Spełnione | `Program.cs:54` (regex wyłączający `api` i `swagger`); `HostingTests.UnknownApiController_Returns404InsteadOfClientPage` |
| | 4 `/` i `/counter` jak w US-01-17 | Spełnione | `HostingTests.ClientRoutes_ServeClientPage` |
| | 5 Kontrolery bez logiki i bez EF | Spełnione | przegląd 6 kontrolerów (tylko `I…Service`); `ControllerConventionTests.Controllers_DoNotDependOnDbContextOrEntityFramework` |
| | 6 Nieoczekiwany wyjątek → log + 500 ogólny | Spełnione | `BaseApiController.cs:99-105`; `ProductApiTests.Service_UnexpectedException_Returns500WithGenericMessageAndIsLogged` |
| | 7 400/404/409 zamiast 500 | Spełnione | `BaseApiController.HandleException`; testy 404/400/409 w `Api/*` |
| | 8 `AddControllers`, brak MVC route/Antiforgery/CORS, System.Text.Json | Spełnione | `Program.cs:11`; `ArchitectureTests.US_01_01_WebProject_ExposesApiControllersAndSwaggerOnlyInDevelopment`; brak Newtonsoft w csproj |
| | 9 Przekierowanie HTTPS, profil HTTPS | Spełnione | `Program.cs:21`; `launchSettings.json` (profil `https`); `HostingTests.HttpRequest_IsRedirectedToHttps` |
| | 10 build 0/0, testy zielone, bez Node.js | Spełnione | uruchomienia powyżej |
| US-02-02 | 1 `JwtOptionsVm`, appsettings bez `SecretKey` | Spełnione | `appsettings.json`; `AuthenticationTests.JwtOptions_AreReadFromConfiguration` |
| | 2 Polityka haseł | Spełnione | `AuthenticationServiceCollectionExtensions.cs:37-43`; `AuthenticationTests.IdentityPasswordPolicy_…` |
| | 3 Token poprawny uwierzytelnia, role dostępne | Spełnione | `AuthenticationTests.ValidToken_IsAccepted`, `RolesFromToken_…` |
| | 4 401: brak, zły podpis, wygasły (skew 5 min), zły iss/aud, zniekształcony | Spełnione | `AuthenticationServiceCollectionExtensions.cs:61-62`; `AuthenticationTests.*` (6 testów) |
| | 5 Development: losowy klucz ≥32 B, token nie przeżywa restartu | Spełnione | `JwtKeyTests.Development_*` |
| | 6 Poza Development: czytelny błąd po polsku | Spełnione | `JwtKeyTests.OutsideDevelopment_*`; uruchomienie ręczne |
| | 7 Brak klucza i haseł w repozytorium | Spełnione | przegląd `appsettings*.json`, README, kodu; `ControllerConventionTests.AppSettings_…`; `ConventionTests` (czystość repo) |
| | 8 Działanie na SQLite i SQL Server | Spełnione | SQLite: testy integracyjne zielone; SQL Server: `SqlServerApiTests.SqlServer_LoginIssuesTokenAcceptedByProtectedEndpoint` (login, 401 bez tokenu, 200 z tokenem) zielony na SQL Server Express; kod nie odwołuje się do providera (`US_01_06_…DoNotReferenceDatabaseProviders`) |
| US-02-03 | 1 Role i konta testowe w Development, bez duplikatów | Spełnione | `TestAccountSeedingTests.Development_WithPassword_…`, `SeedingAgain_DoesNotCreateDuplicates` |
| | 2 Poza Development brak kont | Spełnione | `TestAccountSeedingTests.OutsideDevelopment_…`; `Program.cs:62` |
| | 3 Brak hasła → ostrzeżenie po polsku, start trwa | Spełnione | `TestAccountSeeder.cs:37-42`; test i uruchomienie ręczne |
| | 4 Hasło kont nie występuje w plikach | Spełnione | przegląd repozytorium (hasła losowe w testach) |
| | 5 `LoginUserVm` — walidacja | Spełnione | `Login_InvalidInput_Returns400`, `Login_TooLongPassword_Returns400` |
| | 6 Login JSON, urlencoded, multipart → 200 + `access_token`/`expires_in` | Spełnione | `AccountApiTests.Login_As*`; `Infrastructure/FormLoginInputFormatter.cs` |
| | 7 Claims tokenu (nazwa, nbf, exp, iss, aud, role) | Spełnione | `LoginService.cs:44-59`; `Login_Token_ContainsExpectedClaims` |
| | 8 Błędne dane → 401, „Nieprawidłowy login lub hasło”, brak blokady | Spełnione | `AccountApiController.cs:24-27`; `Login_WrongCredentials_…`, `Login_ManyFailedAttempts_DoNotLockTheAccount` |
| | 9 Walidacja → 400 bez próby logowania | Spełnione | `Login_InvalidInput_DoesNotAttemptToLogIn` |
| | 10 Wyjątek → log + 500 ogólny | Spełnione | `Login_UnexpectedException_Returns500WithGenericMessageAndIsLogged` |
| | 11 Brak hasła i tokenu w logach | Spełnione | `Login_DoesNotWritePasswordOrTokenToLogs`; przegląd kodu logowania |
| | 12 Login publiczny | Spełnione | `[AllowAnonymous]`; `Login_IsPublic_ButRestOfApiRequiresToken` |
| | 13 Token działa dla Admin i Customer | Spełnione | `Login_Token_WorksForBothRolesOnProtectedEndpoint` |
| US-02-04 | 1–2 Strona „WebStore API” v1, endpointy, schemat Bearer | Spełnione | `SwaggerServiceCollectionExtensions.cs`; `HostingTests.SwaggerDocument_InDevelopment_DescribesEndpointsAndBearerScheme` |
| | 3 Okno Authorize wysyła Bearer | Spełnione (pośrednio) | schemat HTTP Bearer w dokumencie; klikanie UI nie było wykonane (przeglądarki brak) |
| | 4 Bez tokenu 401 | Spełnione | `AuthenticationTests.MissingToken_IsRejected` |
| | 5 Swagger zamiast klienta | Spełnione | `HostingTests.SwaggerUi_InDevelopment_ShowsDocumentationInsteadOfClient` |
| | 6 Poza Development 404 | Spełnione | `Program.cs:14-17,23-27`; `HostingTests.Swagger_OutsideDevelopment_Returns404` |
| US-02-05 | 1–2 Lista (z pustą) | Spełnione | `ProductApiTests.GetAll_ReturnsAllVisibleProducts`, `…WhenEveryProductIsHidden_ReturnsEmptyList` |
| | 3 GET {id} | Spełnione | `GetById_ReturnsProduct` |
| | 4–5 404 dla nieistniejącego/ukrytego, `Id` <1 lub `abc` | Spełnione | `ProductApiController.cs:21` (`{id:int:min(1)}`); `GetById_UnknownOrInvalidId_Returns404` |
| | 6 401 bez tokenu | Spełnione | `Get_WithoutValidToken_Returns401` |
| | 7 Wyjątek → 500 | Spełnione | `Service_UnexpectedException_…` |
| US-02-06 | 1 POST 201 + Location | Spełnione | `Post_CreatesProductAndReturns201WithLocation`, `Post_WithZeroId_…` |
| | 2 PUT 200 | Spełnione | `Put_UpdatesExistingProduct` |
| | 3–4 POST z Id / PUT bez Id → 400 | Spełnione | `BaseApiController.cs:56-62,71-77`; `Post_WithId_…`, `Put_WithoutId_…` |
| | 5 Walidacja 400 ze szczegółami | Spełnione | `Write_WithMissingRequiredField_Returns400WithValidationDetails` |
| | 6 Puste/niepoprawne JSON → 400 | Spełnione | `Write_WithEmptyOrMalformedBody_Returns400` |
| | 7 Nieistniejąca kategoria/dostawca → 400 | Spełnione | `Post_UnknownCategoryOrSupplier_…`, `Put_UnknownCategory_Returns400` |
| | 8 PUT nieistniejący/ukryty → 404 | Spełnione | `Put_UnknownId_Returns404AndAddsNothing`, `Put_HiddenProduct_Returns404` |
| | 9 Obraz bez zmian | Spełnione | `Put_WithoutImage_KeepsExistingImage` |
| | 10 Dokładność ceny | Spełnione | `Price_WithFractionalPart_IsStoredExactly` (SQLite); `SqlServerApiTests.SqlServer_Price_WithFractionalPart_IsStoredExactly` (SQL Server Express, zielony) |
| | 11 401 bez zapisu | Spełnione | `Write_WithoutValidToken_Returns401AndSavesNothing` |
| US-02-07 | 1 DELETE 204, rekord zostaje z flagą | Spełnione | `Delete_HidesProductButKeepsRecord` |
| | 2 404 dla nieistniejącego/ukrytego | Spełnione | `Delete_UnknownOrAlreadyHiddenProduct_Returns404` |
| | 3 Zamówienia nietknięte | Spełnione | `Delete_ProductInExistingOrder_LeavesOrderUntouched` |
| | 4 Id <1 / nie liczba → 404 | Spełnione | `Delete_InvalidId_Returns404` |
| | 5 401 | Spełnione | `Delete_WithoutValidToken_Returns401AndKeepsProductVisible` |
| | 6 Migracje SQLite i SQL Server, domyślnie widoczny, brak zmian modelu bez migracji | Spełnione | `Migrations/Sqlite/20261024152243_…`, `Migrations/SqlServer/20261024152408_…` (`defaultValue: false`); `MigrationTests` zielone |
| | 7 Zamówienie z ukrytym produktem → 400 | Spełnione | `OrderService.cs:36-42`; `Order_WithHiddenProduct_IsRejectedWithValidationError`, `Vis.Order_WithHiddenProduct_…` |
| | 8 Testy usług | Spełnione | `ProductVisibilityServiceTests` (7 testów) |
| US-02-08 | 1–7 Adresy (lista, GET, POST/PUT, walidacja, 401, brak DELETE) | Spełnione | `AddressApiTests` (7 testów), `ControllerConventionTests.OnlyProductApi_ExposesDelete` |
| US-02-09 | 1–7 Sklepy (lista, GET, POST/PUT, nieistniejący adres, 401, brak DELETE) | Spełnione | `StoreApiTests` |
| | 8–9 Adres zajęty → 400 po polsku, własny adres → 200 | Spełnione | `StoreApiTests.Write_AddressUsedByAnotherStore_Returns400WithMessageAndSavesNothing`, `Put_StoreWithOwnAddress_Returns200` |
| | 10 Walidacja w `StoreService` przed zapisem + testy usługi | Spełnione | `StoreService.cs:29-34`; `StoreServiceTests` (zajęty adres przy dodawaniu i modyfikacji, własny adres) |
| US-02-10 | 1–10 Zamówienia (lista, GET, POST z ceną zamrożoną, walidacja pozycji, klient, PUT, JSON, 401, brak DELETE, ukryty produkt) | Spełnione | `OrderApiTests` (14 testów), `ProductApiTests.Order_WithHiddenProduct_…` |
| US-02-11 | 1–9 Faktury (lista, GET, POST z `TotalAmount` z zamówienia, 409, 400, PUT, walidacja, 401, brak DELETE) | Spełnione | `InvoiceApiTests`, w tym `Post_SecondInvoiceForTheSameOrder_Returns409AndSavesNothing` |
| US-02-12 | 1–4 Proxy `/api` i `/swagger`, 502 przy niedostępnym API | Spełnione (przegląd konfiguracji) | `ClientApp/vite.config.js:4-29,36-41`; ręczne uruchomienie opisane w raporcie implementacji, w recenzji nie powtarzane |
| | 5 `npm run build` bez błędów, brak wywołań API w kliencie | Spełnione (część statyczna) | brak `fetch`/`XMLHttpRequest`/`axios` w `ClientApp/src`; build klienta wykonał Lider |
| | 6 Brak sekretów | Spełnione | `vite.config.js` (adres z `API_TARGET`, domyślnie localhost) |
| | 7 Zmiany klienta ograniczone do `vite.config.js` | Spełnione | `git diff` w `WebStore.Web/ClientApp` |
| US-02-13 | 1–2 Tests→Web, testy architektury | Spełnione | `WebStore.Tests.csproj`; `ArchitectureTests.US_01_01_CsprojFiles_ReferenceExpectedProjects`; `CLAUDE.md:37` zaktualizowany |
| | 3 Testy konwencji kontrolerów | Spełnione | `ControllerConventionTests` |
| | 4 Izolacja testów (własna baza i JWT) | Spełnione | `Infrastructure/ApiFactory.cs` |
| | 5 Zakres pokrycia | Spełnione | wszystkie wymienione scenariusze mają testy (tabele wyżej) |
| | 6 `dotnet test` zielony, SQL Server pominięty | Spełnione | uruchomienie powyżej |
| | 7 Brak usuniętych/osłabionych testów sprintu 1 | Spełnione | `git diff WebStore.Tests`: zmieniono tylko 2 testy architektury, jak opisano w raporcie implementacji |
| | 8 Nazwy nowych testów bez identyfikatorów US; README z filtrami | Spełnione | `SqlServerApiTests.cs:25,41` i testy w `Api/*` bez prefiksów `US_`; jedyne nazwy z prefiksem w kodzie sprintu 2 to `StoreServiceTests.cs:63,78` (`US_02_09_…`), objęte wyjątkiem z kryterium; README sekcja „Testy” |

## 3. Uwagi

### Krytyczne
Brak.

### Istotne
Brak. Poprawka A (duplikat faktury): `DuplicateInvoiceException` (`WebStore.Services/Exceptions/DuplicateInvoiceException.cs`) jest rzucany przez `InvoiceService.cs:37` i `:58-62` (także po `DbUpdateException`, gdy inna faktura zamówienia istnieje; diagnoza niezależna od providera, inne błędy lecą dalej jako 500), a `InvoiceApiController.cs:44` mapuje wyłącznie ten typ na 409. Deterministyczne testy wyścigu: `InvoiceServiceTests.AddOrUpdateInvoice_DuplicateCreatedConcurrently_ThrowsDuplicateInvoiceException` (SQLite) i `SqlServerProviderTests.SqlServer_InvoiceDuplicateCreatedConcurrently_ThrowsDuplicateInvoiceException` (SQL Server) — zielone.

### Drobne

2. **`WebStore.Web/Controllers/BaseApiController.cs:99-105` — podwójne logowanie błędu 500.** `BaseService.Execute` loguje wyjątek, a kontroler loguje go ponownie. Scenariusz: każdy nieoczekiwany błąd usługi daje dwa wpisy `Error` w logu. Kierunek: wystarczy jeden wpis, ale US-02-01 pkt 6 wymaga logu po stronie kontrolera, więc rozstrzyga Lider.
3. **`WebStore.Web/Services/LoginService.cs:35-39` — różnica czasu odpowiedzi dla nieistniejącego loginu.** Dla nieistniejącego użytkownika hasło nie jest haszowane, więc odpowiedź jest szybsza niż przy błędnym haśle istniejącego konta, co umożliwia wnioskowanie o istnieniu loginu (US-02-03 pkt 8 wymaga, by odpowiedź tego nie ujawniała; treść i kod są identyczne, ale nie czas). Kierunek: wykonać fikcyjne haszowanie hasła, gdy użytkownik nie istnieje. Brak blokady konta i limitu prób to świadoma decyzja ze story (pkt 8), nie błąd.
4. **`WebStore.Web/Configuration/SwaggerServiceCollectionExtensions.cs:21-24` — globalny wymóg bezpieczeństwa dotyczy też `Login`.** W Swagger UI `POST /api/AccountApi/Login` ma kłódkę, choć jest publiczny. Efekt wyłącznie kosmetyczny. Kierunek: filtr operacji pomijający endpointy z `[AllowAnonymous]`.
5. **Start poza Development bez klucza JWT wypisuje pełny stack trace** (`OptionsValidationException`); komunikat po polsku jest czytelny, ale ginie wśród śladu stosu. Kierunek: opcjonalnie przechwycić wyjątek w `Program.cs` i wypisać sam komunikat.
6. **`WebStore.Tests/UnitTests/*` i `WebStore.Tests/*ServiceTests.cs` — dwie równoległe serie testów usług** (z sprintu 1: układ z instrukcji laboratorium oraz własne testy) pokrywają w dużej mierze te same scenariusze. Dotyczy zmian sprintu 1, bez wpływu na poprawność; do decyzji Lidera, czy zostawić.
7. **Poza repozytorium (scratchpad sesji): `api.log`, `pw.txt`, `smoke.db`, `vite.log`** w `/private/tmp/claude-501/-Users-rafalbedkowski-uczelnia-ZIP-ZPI-Pcz/fbc64f64-2ece-48ad-9ae0-f8849ff51e86/scratchpad/` — pozostałości ręcznych testów programisty, w tym plik z hasłem kont testowych. Nie są w repozytorium; usuwa je Lider (polecenie niżej).

```
rm -f "/private/tmp/claude-501/-Users-rafalbedkowski-uczelnia-ZIP-ZPI-Pcz/fbc64f64-2ece-48ad-9ae0-f8849ff51e86/scratchpad/"{api.log,pw.txt,smoke.db,vite.log}
```

8. **Test API na prawdziwą równoległość `POST` faktury nie istnieje.** Mapowanie wyścigu na 409 jest pokryte testem usługi (z przechwytywaczem) i testem 409 dla sekwencyjnego duplikatu, ale nie jednym testem end-to-end. Świadoma decyzja programisty (test byłby niedeterministyczny); odnotowane informacyjnie.

### Uwaga środowiskowa (nie defekt)

9. **Niestabilność testów na lokalnym SQL Server Express (zaakceptowane przez Lidera).** `CREATE DATABASE` na instancji SQLEXPRESS bywa chwilowo bardzo wolny (przebiegi od ok. 8 s do 3,5 min), co jednorazowo powodowało porażkę losowego testu (np. `SqlServerProviderTests.US_01_10_SqlServer_TotalAmountIsExactAfterRoundTrip`, wyjątek w `SqlCommand.RunExecuteNonQueryTds`). Przyczyny nie ustalono i nie ma dowodu, że leży w kodzie testów. Limit 120 s (`WebStore.Tests/Infrastructure/SqlServerTestOptions.cs`, użyty w `SqlServerProviderTests` i `ApiFactory`) łagodzi problem, lecz przy spowolnieniach dłuższych niż 120 s porażka może wystąpić. W tej recenzji przebieg z SQL Server: 318/318 w 31 s. Pozostałość `WebStoreTests_d8f23dec9f52467a8c7eea361dfb2910` w SQLEXPRESS zostawiona zgodnie z poleceniem.

### Artefakty redakcyjne, sekrety, zakres

- Brak `TODO`/`FIXME`/`XXX`/`TBD` (poza opisami zasad w CLAUDE.md i definicjach agentów), logów debugowych, zakomentowanego i martwego kodu, plików `*.bak`/`tmp`, komentarzy o historii zmian (przeszukano `.cs`, `.js`, `.jsx`, `.json`, `.md`, `.sh`, `.ps1`, `.csproj`, `.html`).
- Testy SQL Server: `ApiFactory` tworzy osobną bazę `WebStoreApiTests_<guid>` i usuwa ją w `Dispose`; ciąg połączenia pochodzi wyłącznie ze zmiennej środowiskowej. W repozytorium brak ciągów połączenia i haseł (wystąpienia `SQLEXPRESS` to opisy w README oraz dane wejściowe testu `ArchitectureTests`). `CLAUDE.md`, `user-stories.md` i `raport-implementacji.md` opisują stan bieżący, bez śladów historii.
- Brak sekretów: `appsettings*.json` bez `SecretKey`, ciągu połączenia i haseł; hasło kont i klucz tylko z konfiguracji; w testach losowe. `webstore.db` i `wwwroot` są ignorowane przez git; w śledzonych plikach brak `*.db` i `TestResults`.
- Poprawka B: `DatabaseServiceCollectionExtensions.cs:25-33` bez domyślnego providera (komunikat po polsku); `appsettings.json` bez sekcji `Database` i ciągów połączenia; profil `https-sqlite` bez sekretów; `setup.ps1` i `setup.sh` spójne z README i CLAUDE.md (wszystkie komunikaty wskazują jawny wybór SQLite lub SQL Server; testy i build po `git clone` działają bez konfiguracji bazy, bo używają SQLite in-memory). W repozytorium brak ciągów połączenia i haseł. `CLAUDE.md` (stos, Web API, stan i otwarte sprawy) spójny z kodem.
- `CLAUDE.md`: usunięto sekcję „Stan prac w sprincie 2” (nieaktualna po implementacji), schemat zależności zawiera Tests→Web; zgodne z `ArchitectureTests`.
- Zależności Web↔Tests: tylko Tests→Web, nic nie referuje Web ani Tests (test architektury).
- Role nie ograniczają dostępu: `[Authorize]` bez ról na kontrolerze bazowym, `[AllowAnonymous]` tylko na `Login`.
- Swagger i konta testowe wyłącznie w Development (`Program.cs:14-17,23-27,60-64`).
- Brak funkcji spoza zakresu user stories; kontrolery cienkie (logika Create/Update w `BaseApiController` to tylko orkiestracja i mapowanie wyjątków na kody HTTP).
- Klient React: zmieniono tylko `vite.config.js` (poza zmianami sprintu 1 w historii).

## 4. Pytania do Lidera

Brak otwartych pytań.
