# Sprint 02 — User stories

**Źródło wymagań:** `wymagania/ZPILab2.pdf` (Zaawansowane Programowanie Internetowe, Laboratorium 2: REST, REST API, ASP.NET Core Web API)

**Cel sprintu:** Udostępnić funkcjonalności sklepu (produkty, zamówienia, faktury, sklepy, adresy) przez REST API zabezpieczone tokenem JWT i opisane dokumentacją Swagger. Dzięki temu dowolny klient (React, aplikacja mobilna lub desktopowa) może z nich korzystać, a cała logika biznesowa pozostaje w usługach.

Uwagi ogólne:
- Dokument wymagań jest instrukcją laboratoryjną (kroki 1–10 oraz „Zadanie”) z kodem napisanym pod .NET 6 i `create-react-app`. Kod ten jest dostosowany do stosu projektu (.NET 10, Vite) według ustaleń poniżej. Wersje pakietów (np. 6.0.9 z instrukcji) nie są kryteriami akceptacji; wersje ustala programista poleceniem `dotnet package search`.
- Ustalenia techniczne obowiązujące w całym sprincie:
  - Swagger: pakiet Swashbuckle.AspNetCore, adresy `/swagger` i `/swagger/index.html` jak w instrukcji, dostępny tylko w środowisku Development.
  - Serializacja JSON: domyślny `System.Text.Json` (bez Newtonsoft.Json).
  - Rejestracja: `AddControllers` i trasy atrybutowe; bez `AddControllersWithViews`, bez trasy MVC `{controller}/{action}/{id?}`, bez Antiforgery, bez CORS.
  - HTTPS: przekierowanie na HTTPS włączone, `launchSettings.json` zawiera profil HTTPS.
  - Baza danych: mechanizm ze sprintu 1 bez zmian (provider wybierany konfiguracją, ciągi połączeń wyłącznie w user-secrets lub zmiennych środowiskowych, bez ciągu połączenia w `appsettings.json`; ciąg LocalDB z instrukcji nie trafia do repozytorium).
- Zasada nadrzędna z dokumentu: kontrolery **nie zawierają logiki biznesowej**. Ich zadaniem jest wyłącznie (1) odebranie danych od klienta i walidacja, (2) wywołanie metody usługi, (3) zwrócenie wyniku usługi. Kontrolery korzystają z usług i view modeli ze sprintu 1 (`IProductService`, `IOrderService`, `IInvoiceService`, `IStoreService`, `IAddressService`); nie sięgają do `ApplicationDbContext`.
- Konwencje nazw z dokumentu: kontrolery `<Zasób>ApiController` dziedziczą po `BaseApiController` (`[ApiController]`, `[Produces("application/json")]`, trasa `api/[controller]`, np. `/api/ProductApi`). Kontroler bazowy udostępnia `ILogger` i `IMapper`.
- Wspólna tabela kodów odpowiedzi (obowiązuje wszystkie kontrolery zasobów):
  - nieistniejący zasób przy `GET {id}`, `PUT`, `DELETE` — 404;
  - błąd walidacji (adnotacje view modelu, `ValidationException` z usług, niepoprawny lub pusty JSON) oraz wskazanie nieistniejącego powiązania (np. nieistniejący klient, zamówienie, kategoria, dostawca, adres, produkt) — 400;
  - druga faktura do tego samego zamówienia — 409;
  - `POST` — 201 z nagłówkiem `Location` wskazującym nowy zasób; `PUT` — 200; `DELETE` — 204;
  - `POST` z niepustym `Id` (różnym od null i od 0) oraz `PUT` bez `Id` (null lub 0) — 400;
  - nieoczekiwany wyjątek — 500 z ogólnym komunikatem;
  - brak, błędny lub wygasły token (poza `Login`) — 401.
- Wszystkie endpointy poza `POST /api/AccountApi/Login` wymagają poprawnego tokenu; role (Admin, Customer) nie ograniczają dostępu do żadnego endpointu.
- Sprint 2 świadomie zastępuje założenie sprintu 1 „brak kontrolerów API i Swagger” (US-01-01 pkt 8 oraz uwagi ogólne sprintu 1). Dokument sprintu 1 pozostaje bez zmian jako opis stanu zamkniętego. Założenie „klient React nie komunikuje się z serwerem” (US-01-17) pozostaje w mocy: klient React w sprincie 2 nie zmienia się poza konfiguracją proxy deweloperskiego (US-02-12).
- Zmiana domyślnego providera bazy na SQL Server, zmiany testów, `setup.ps1` i zapisów w `CLAUDE.md` z tym związane są poza zakresem sprintu 2.
- Poza zakresem user stories (czynności Lidera/studenta): instalacja i konfiguracja Postmana (m.in. wyłączenie weryfikacji SSL), commit i push. Ręczne sprawdzenie API Swaggerem lub Postmanem jest dopuszczalnym uzupełnieniem testów automatycznych.

---

## Lista user stories

### US-02-01: Szkielet Web API (kontroler bazowy, JSON, trasy)

**Status:** Zaakceptowana
**Źródło:** Rozdz. 3 pkt 2, 4, 6, 9; „Zadanie” (zasada: brak logiki biznesowej w kontrolerach)
**Priorytet:** Wysoki

**Jako** programista
**chcę** mieć w `WebStore.Web` skonfigurowane Web API z kontrolerem bazowym `BaseApiController`
**aby** kolejne kontrolery zasobów miały wspólne, spójne zachowanie i mogły być wywoływane przez dowolnego klienta HTTP.

**Kryteria akceptacji:**
1. **Zakładając, że** projekt `WebStore.Web` jest zbudowany, **gdy** zostanie sprawdzony folder `Controllers`, **wtedy** istnieje w nim abstrakcyjna klasa `BaseApiController` (przestrzeń nazw `WebStore.Web.Controllers`) oznaczona jako kontroler API, produkująca `application/json`, z trasą `api/[controller]`, z chronionymi polami tylko do odczytu `Logger` (`ILogger`) i `Mapper` (`IMapper`) przekazywanymi w konstruktorze.
2. **Zakładając, że** aplikacja działa, **gdy** zostanie wysłane żądanie do dowolnego endpointu API, **wtedy** odpowiedź ma nagłówek `Content-Type: application/json` (lub `application/problem+json` dla błędów); odpowiedzi 204 nie mają treści.
3. **Zakładając, że** aplikacja działa i klient React jest zbudowany, **gdy** zostanie wysłane `GET /api/NieIstniejacyKontroler`, **wtedy** serwer odpowiada kodem 404 i nie zwraca `index.html` klienta (ścieżki `/api` i `/swagger` nie są przechwytywane przez routing klienta).
4. **Zakładając, że** aplikacja działa i klient React jest zbudowany, **gdy** zostanie wysłane `GET /` lub `GET /counter`, **wtedy** zachowanie klienta jest bez zmian względem US-01-17 (kod 200 i `index.html` aplikacji).
5. **Zakładając, że** kontrolery istnieją, **gdy** zostanie sprawdzony ich kod, **wtedy** nie zawierają logiki biznesowej ani dostępu do bazy: nie odwołują się do `ApplicationDbContext` ani do typów EF Core, a operacje na danych wykonują wyłącznie przez interfejsy usług (`I…Service`).
6. **Zakładając, że** w trakcie obsługi żądania w usłudze wystąpi nieoczekiwany wyjątek, **gdy** kontroler go przechwyci, **wtedy** błąd jest zapisany przez `ILogger` (z wyjątkiem), a klient dostaje odpowiedź 500 z ogólnym komunikatem bez szczegółów wyjątku (bez stack trace i danych wewnętrznych).
7. **Zakładając, że** usługa zgłasza wyjątek walidacji, „nie znaleziono” lub naruszenia unikalności faktury, **gdy** kontroler go obsłuży, **wtedy** klient dostaje odpowiednio 400, 404 lub 409 zgodnie z tabelą kodów z uwag ogólnych (nie 500).
8. **Zakładając, że** projekt jest skonfigurowany, **gdy** zostanie sprawdzony kod startowy, **wtedy** kontrolery rejestrowane są przez `AddControllers` z trasami atrybutowymi; nie ma trasy MVC `{controller}/{action}/{id?}`, Antiforgery ani konfiguracji CORS, a serializacja używa `System.Text.Json`.
9. **Zakładając, że** aplikacja działa na HTTP, **gdy** zostanie wysłane żądanie pod adres HTTP, **wtedy** serwer przekierowuje je na HTTPS, a `launchSettings.json` zawiera profil uruchamiający aplikację także pod adresem HTTPS.
10. **Zakładając, że** zmiana jest wprowadzona, **gdy** zostanie wykonane `dotnet build` i `dotnet test`, **wtedy** build kończy się z 0 błędów i 0 ostrzeżeń, wszystkie testy są „Passed”, a polecenia nie wymagają Node.js (US-01-01 pkt 6).

**Uwagi:** Zgodność z testami konwencji i architektury (`ConventionTests`, `ArchitectureTests`) musi zostać zachowana po ich dostosowaniu do kontrolerów (US-02-13).

---

### US-02-02: Konfiguracja uwierzytelniania JWT i Identity

**Status:** Zaakceptowana
**Źródło:** Rozdz. 3 pkt 2–5, 7 (JwtBearer, `JwtOptions`, `JwtOptionsVm`, `AddIdentity`, `AddAuthentication`/`AddJwtBearer`)
**Priorytet:** Wysoki

**Jako** programista
**chcę** skonfigurować w aplikacji Identity oraz uwierzytelnianie tokenem JWT Bearer z parametrami z konfiguracji
**aby** API było bezstanowe, a żądania wymagające uwierzytelnienia mogły być autoryzowane tokenem.

**Kryteria akceptacji:**
1. **Zakładając, że** aplikacja ma sekcję konfiguracji `JwtOptions` (`Issuer`, `Audience`, `SecretKey`, `TokenExpirationMinutes`), **gdy** aplikacja startuje, **wtedy** wartości są odczytane do view modelu `JwtOptionsVm` (projekt ViewModels, folder `VM`) i dostępne przez `IOptions<JwtOptionsVm>`; `appsettings.json` zawiera `Issuer`, `Audience` i `TokenExpirationMinutes` = 20, a nie zawiera `SecretKey`.
2. **Zakładając, że** skonfigurowano Identity (`User`, `IdentityRole<int>`) na `ApplicationDbContext`, **gdy** zostanie sprawdzona polityka haseł, **wtedy** wymagana jest minimalna długość 6, nie jest wymagana cyfra, wielka litera, mała litera ani znak niealfanumeryczny, a unikalny e-mail nie jest wymagany.
3. **Zakładając, że** token jest podpisany kluczem symetrycznym z `JwtOptions:SecretKey` (HMAC-SHA256), **gdy** żądanie do chronionego endpointu zawiera nagłówek `Authorization: Bearer <poprawny token>`, **wtedy** żądanie jest uwierzytelnione, a role z tokenu są dostępne jako role użytkownika.
4. **Zakładając, że** endpoint wymaga uwierzytelnienia, **gdy** żądanie nie ma tokenu, ma token z błędnym podpisem, wygasły (po uwzględnieniu tolerancji zegara 5 minut), z innym `Issuer` lub `Audience` niż skonfigurowane albo niepoprawnie zbudowany, **wtedy** serwer odpowiada kodem 401 i nie wykonuje operacji.
5. **Zakładając, że** aplikacja działa w środowisku Development i `JwtOptions:SecretKey` nie jest skonfigurowany, **gdy** aplikacja startuje, **wtedy** generuje losowy klucz (min. 32 bajty) ważny do końca działania procesu; token wydany przed restartem aplikacji jest po restarcie odrzucany kodem 401.
6. **Zakładając, że** aplikacja działa w środowisku innym niż Development, **gdy** `JwtOptions:SecretKey` nie jest skonfigurowany lub ma mniej niż 32 bajty, **wtedy** start kończy się czytelnym komunikatem po polsku wskazującym `JwtOptions:SecretKey`, a nie niezwiązanym wyjątkiem; skonfigurowany klucz jest odczytywany wyłącznie z user-secrets lub zmiennej środowiskowej.
7. **Zakładając, że** repozytorium jest sprawdzone, **gdy** zostaną przejrzone jego pliki (w tym `appsettings*.json`, `README`), **wtedy** nie występuje w nich klucz podpisujący JWT ani hasła kont testowych (zgodnie z `CLAUDE.md`).
8. **Zakładając, że** aplikacja jest uruchomiona na SQLite (domyślnie) lub na SQL Server (skonfigurowanym jak w sprincie 1), **gdy** zostanie wykonane logowanie i wywołanie chronionego endpointu, **wtedy** działają tak samo na obu providerach bez zmian w modelu i usługach.

**Uwagi:** Zależy od US-01-06 (kontekst z Identity, `User`). Czas wydania i wygaśnięcia tokenu jest w UTC (US-02-03). Konta, na których działa logowanie: US-02-03.

---

### US-02-03: Konta testowe, logowanie i wydawanie tokenu (AccountApi/Login)

**Status:** Zaakceptowana
**Źródło:** Rozdz. 3 pkt 5 (`LoginUserVm`), pkt 7 (`AccountApiController`), Postman (pola Login, Password; odpowiedź `access_token`, `expires_in`)
**Priorytet:** Wysoki

**Jako** użytkownik API
**chcę** zalogować się loginem (e-mail) i hasłem i otrzymać token JWT
**aby** wysyłać go w nagłówku `Authorization` i korzystać z chronionych zasobów.

**Kryteria akceptacji:**

Konta testowe:
1. **Zakładając, że** aplikacja startuje w środowisku Development i hasło kont testowych jest skonfigurowane w user-secrets lub zmiennej środowiskowej, **gdy** start się zakończy, **wtedy** w bazie istnieją role `Admin` i `Customer` oraz konta `admin@webstore.local` (rola `Admin`) i `customer@webstore.local` (rola `Customer`, encja `Customer`), oba z hasłem z ustawienia `Seed:TestAccountPassword`; ponowny start nie tworzy duplikatów kont ani ról.
2. **Zakładając, że** aplikacja startuje w środowisku innym niż Development, **gdy** start się zakończy, **wtedy** konta testowe nie są tworzone.
3. **Zakładając, że** aplikacja startuje w środowisku Development bez skonfigurowanego hasła kont testowych, **gdy** start się zakończy, **wtedy** konta testowe nie są tworzone, a w logu jest ostrzeżenie po polsku wskazujące brakującą wartość `Seed:TestAccountPassword` (aplikacja nie przerywa startu).
4. **Zakładając, że** repozytorium jest sprawdzone, **gdy** zostaną przejrzone jego pliki, **wtedy** hasło kont testowych nie występuje w żadnym z nich.

Logowanie:
5. **Zakładając, że** istnieje view model `LoginUserVm` (projekt ViewModels, folder `VM`), **gdy** zostanie sprawdzony, **wtedy** pola `Login` i `Password` są wymagane, `Login` ma długość 6–50 znaków, `Password` ma długość 6–100 znaków, a `Password` jest typu hasło.
6. **Zakładając, że** istnieje konto testowe, **gdy** zostanie wysłane `POST /api/AccountApi/Login` z poprawnym `Login` i `Password` jako formularz (`multipart/form-data` lub `application/x-www-form-urlencoded`) albo jako JSON (`application/json`), **wtedy** w obu przypadkach odpowiedź ma kod 200 i zawiera `access_token` (token JWT) oraz `expires_in` (czas wygaśnięcia UTC w formacie `yyyy-MM-ddTHH:mm:ss`).
7. **Zakładając, że** użytkownik zalogował się poprawnie, **gdy** zostanie zdekodowany `access_token`, **wtedy** zawiera: nazwę użytkownika (login), `nbf`, `exp` równy czasowi wydania plus `TokenExpirationMinutes` (domyślnie 20 minut), `iss` i `aud` zgodne z `JwtOptions` oraz po jednym claimie roli dla każdej roli użytkownika; czasy są liczone w UTC.
8. **Zakładając, że** konto istnieje, **gdy** zostanie wysłane logowanie z błędnym hasłem, **wtedy** odpowiedź ma kod 401 i komunikat „Nieprawidłowy login lub hasło” oraz nie zawiera tokenu; identyczny wynik dotyczy nieistniejącego loginu (odpowiedź nie ujawnia, czy login istnieje). Wielokrotne błędne próby nie blokują konta (po poprawnych danych następującym po 10 błędnych próbach logowanie się powiedzie).
9. **Zakładając, że** dane są niepoprawne walidacyjnie (brak pola, `Login` krótszy niż 6 znaków, `Password` krótszy niż 6 lub dłuższy niż 100 znaków, puste ciało), **gdy** zostanie wysłane logowanie, **wtedy** odpowiedź ma kod 400 i nie jest podejmowana próba logowania.
10. **Zakładając, że** w trakcie logowania wystąpi wyjątek, **gdy** kontroler go przechwyci, **wtedy** błąd jest zapisany w logu, a klient dostaje 500 z ogólnym komunikatem bez szczegółów wyjątku.
11. **Zakładając, że** użytkownik loguje się (poprawnie lub nie), **gdy** zostaną przejrzone logi aplikacji, **wtedy** nie zawierają hasła (kod z instrukcji loguje hasło — to jest niedozwolone) ani wydanego tokenu.
12. **Zakładając, że** endpoint logowania jest publiczny, **gdy** zostanie wywołany bez nagłówka `Authorization`, **wtedy** jest obsłużony (nie zwraca 401 z powodu braku tokenu).
13. **Zakładając, że** użytkownik zdobył token, **gdy** użyje go w nagłówku `Authorization: Bearer <token>` do wywołania chronionego endpointu, **wtedy** żądanie jest autoryzowane (kod inny niż 401), niezależnie od roli (Admin lub Customer).

**Uwagi:** Zależy od US-02-02. Role nie ograniczają dostępu do endpointów (decyzja Lidera); służą wyłącznie do rozróżnienia kont i trafiają do tokenu.

---

### US-02-04: Dokumentacja Swagger z obsługą tokenu Bearer

**Status:** Zaakceptowana
**Źródło:** Rozdz. 3 pkt 4 (konfiguracja Swagger), pkt 10 (Swagger UI, `/swagger/index.html`, Postman); okno autoryzacji tokenem w Swaggerze
**Priorytet:** Średni

**Jako** programista (oraz prowadzący oceniający projekt)
**chcę** dynamicznie generowaną dokumentację API z interfejsem do wysyłania żądań, w tym z możliwością podania tokenu
**aby** móc przetestować każdy endpoint bez dodatkowego narzędzia.

**Kryteria akceptacji:**
1. **Zakładając, że** aplikacja działa w środowisku Development, **gdy** zostanie otwarty adres `/swagger/index.html`, **wtedy** wyświetlana jest strona „WebStore API” (wersja `v1`) z listą kontrolerów `AccountApi`, `ProductApi`, `OrderApi`, `InvoiceApi`, `StoreApi`, `AddressApi` i ich endpointów (m.in. `POST /api/AccountApi/Login`, `GET`/`POST`/`PUT /api/ProductApi`, `GET`/`DELETE /api/ProductApi/{id}`) oraz sekcją schematów view modeli.
2. **Zakładając, że** opis OpenAPI jest generowany, **gdy** zostanie pobrany `/swagger/v1/swagger.json`, **wtedy** jest poprawnym dokumentem OpenAPI 3 zawierającym schemat bezpieczeństwa HTTP Bearer (JWT) oraz wszystkie endpointy z kroku 1.
3. **Zakładając, że** użytkownik uzyskał token (US-02-03), **gdy** poda go w oknie „Authorize” Swagger UI, **wtedy** kolejne żądania ze Swaggera zawierają nagłówek `Authorization: Bearer <token>` i chronione endpointy zwracają dane zamiast 401.
4. **Zakładając, że** użytkownik nie podał tokenu, **gdy** wykona ze Swaggera żądanie do chronionego endpointu, **wtedy** otrzymuje 401.
5. **Zakładając, że** klient React jest zbudowany, **gdy** zostanie otwarty `/swagger/index.html` w Development, **wtedy** wyświetlana jest dokumentacja API, a nie aplikacja React (US-02-01 pkt 3).
6. **Zakładając, że** aplikacja działa w środowisku innym niż Development, **gdy** zostanie wysłane `GET /swagger/index.html` lub `GET /swagger/v1/swagger.json`, **wtedy** serwer odpowiada kodem 404.

**Uwagi:** Adres `https://localhost:44433` z instrukcji zależy od `launchSettings.json` i nie jest kryterium. Pakiet: Swashbuckle.AspNetCore.

---

### US-02-05: ProductApi — odczyt produktów

**Status:** Zaakceptowana
**Źródło:** Rozdz. 3 pkt 8 (`GET`, `GET {id}`), akapit o CRUD (R)
**Priorytet:** Wysoki

**Jako** klient API
**chcę** pobrać listę produktów oraz pojedynczy produkt po identyfikatorze
**aby** wyświetlić asortyment sklepu.

**Kryteria akceptacji:**
1. **Zakładając, że** w bazie są widoczne (nieukryte) produkty i żądanie ma poprawny token, **gdy** zostanie wysłane `GET /api/ProductApi`, **wtedy** odpowiedź ma kod 200 i zawiera listę `ProductVm` o liczbie elementów równej liczbie widocznych produktów w bazie.
2. **Zakładając, że** w bazie nie ma widocznych produktów, **gdy** zostanie wysłane `GET /api/ProductApi`, **wtedy** odpowiedź ma kod 200 i pustą listę.
3. **Zakładając, że** istnieje widoczny produkt o `Id` = 1, **gdy** zostanie wysłane `GET /api/ProductApi/1`, **wtedy** odpowiedź ma kod 200 i zawiera `ProductVm` tego produktu.
4. **Zakładając, że** nie istnieje produkt o `Id` = 999 albo produkt jest ukryty, **gdy** zostanie wysłane `GET /api/ProductApi/{id}`, **wtedy** odpowiedź ma kod 404.
5. **Zakładając, że** `Id` w adresie jest mniejsze niż 1 (0, -1) lub nie jest liczbą całkowitą (`abc`), **gdy** zostanie wysłane `GET /api/ProductApi/{id}`, **wtedy** serwer nie wywołuje usługi i odpowiada kodem 404 (ograniczenie trasy `{id:int:min(1)}`).
6. **Zakładając, że** żądanie nie ma poprawnego tokenu, **gdy** zostanie wysłane `GET /api/ProductApi` lub `GET /api/ProductApi/1`, **wtedy** odpowiedź ma kod 401.
7. **Zakładając, że** usługa zgłasza nieoczekiwany wyjątek, **gdy** zostanie wysłane żądanie, **wtedy** zachowanie jest zgodne z US-02-01 pkt 6.

**Uwagi:** Zależy od US-02-01, US-02-02 i US-02-07 (flaga ukrycia: odfiltrowanie ukrytych produktów w usłudze). Pobranie po `Id` realizuje kontroler przez przekazanie filtra do usługi (`p => p.Id == id`).

---

### US-02-06: ProductApi — dodawanie i modyfikacja produktów

**Status:** Zaakceptowana
**Źródło:** Rozdz. 3 pkt 8 (`POST`, `PUT`, `PostOrPutHelper`), akapit o CRUD (C, U)
**Priorytet:** Wysoki

**Jako** klient API
**chcę** dodać nowy produkt oraz zmodyfikować istniejący
**aby** zarządzać asortymentem sklepu.

**Kryteria akceptacji:**
1. **Zakładając, że** `AddOrUpdateProductVm` zawiera poprawne dane bez `Id` (null lub 0), **gdy** zostanie wysłane `POST /api/ProductApi` z ciałem JSON i poprawnym tokenem, **wtedy** produkt jest dodany jako nowy rekord, odpowiedź ma kod 201 z nagłówkiem `Location` wskazującym `/api/ProductApi/{nowe Id}` i zawiera `ProductVm` z wysłanymi `Name` i `Price`.
2. **Zakładając, że** istnieje produkt o `Id` = 1, **gdy** zostanie wysłane `PUT /api/ProductApi` z `Id` = 1 i `Price` = 2000, **wtedy** produkt jest zmodyfikowany (nie powstaje nowy), odpowiedź ma kod 200, a `Price` = 2000.
3. **Zakładając, że** ciało `POST` zawiera `Id` różne od null i od 0, **gdy** żądanie zostanie wysłane, **wtedy** odpowiedź ma kod 400 i nic nie jest zapisane.
4. **Zakładając, że** ciało `PUT` nie zawiera `Id` (null lub 0), **gdy** żądanie zostanie wysłane, **wtedy** odpowiedź ma kod 400 i nic nie jest zapisane.
5. **Zakładając, że** ciało żądania jest niepoprawne walidacyjnie (np. brak `Name`, pola wymagane puste, według adnotacji view modelu), **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400 ze szczegółami błędów walidacji (pola i komunikaty), usługa nie jest wywoływana i nic nie jest zapisane.
6. **Zakładając, że** ciało żądania jest puste lub nie jest poprawnym JSON, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400 i nic nie jest zapisane.
7. **Zakładając, że** `CategoryId` lub `SupplierId` wskazuje nieistniejący rekord, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400 z czytelnym komunikatem (nie 500 z surowym wyjątkiem bazy), a nic nie jest zapisane.
8. **Zakładając, że** nie istnieje produkt o `Id` = 999 (lub produkt jest ukryty), **gdy** zostanie wysłane `PUT` z tym `Id`, **wtedy** odpowiedź ma kod 404 i w bazie nie przybywa rekordów.
9. **Zakładając, że** istnieje produkt z zapisanym obrazem, **gdy** zostanie wysłane `PUT` z jego `Id` i bez `ImageBytes`, **wtedy** dotychczasowy obraz pozostaje bez zmian (US-01-09 pkt 12).
10. **Zakładając, że** cena z częścią ułamkową (np. 19,99) została wysłana w JSON, **gdy** produkt zostanie zapisany i pobrany `GET /api/ProductApi/{id}`, **wtedy** `Price` jest identyczna jak wysłana (bez błędów zaokrągleń), na każdym providerze bazy.
11. **Zakładając, że** żądanie nie ma poprawnego tokenu, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 401 i nic nie jest zapisane.

**Uwagi:** Reguły walidacji i zapisu pozostają w `ProductService`; kontroler rozróżnia dodanie i modyfikację tylko na podstawie metody HTTP i `Id` (kryteria 3–4). Zależy od US-02-01 i US-02-02.

---

### US-02-07: ProductApi — ukrywanie produktu (DELETE)

**Status:** Zaakceptowana
**Źródło:** Rozdz. 3 pkt 8 (`DELETE {id}`, `DeleteProduct`), akapit o CRUD (D: „usunięcie / ukrycie”); decyzja Lidera: ukrycie produktu flagą
**Priorytet:** Średni

**Jako** klient API
**chcę** ukryć wybrany produkt
**aby** wycofać go z oferty sklepu, zachowując historię zamówień.

**Kryteria akceptacji:**
1. **Zakładając, że** istnieje widoczny produkt o `Id` = 1, **gdy** zostanie wysłane `DELETE /api/ProductApi/1` z poprawnym tokenem, **wtedy** odpowiedź ma kod 204 bez treści, a produkt przestaje być zwracany przez `GET /api/ProductApi` oraz `GET /api/ProductApi/1` (404); rekord produktu pozostaje w bazie z ustawioną flagą ukrycia.
2. **Zakładając, że** nie istnieje produkt o `Id` = 999 albo jest już ukryty, **gdy** zostanie wysłane `DELETE /api/ProductApi/{id}`, **wtedy** odpowiedź ma kod 404 i stan bazy się nie zmienia.
3. **Zakładając, że** produkt występuje w pozycjach istniejących zamówień, **gdy** zostanie wysłane `DELETE`, **wtedy** odpowiedź ma kod 204, a zamówienia, ich pozycje (cena zamrożona) i `TotalAmount` pozostają bez zmian i nadal dają się pobrać przez `GET /api/OrderApi/{id}`.
4. **Zakładając, że** `Id` w adresie jest mniejsze niż 1 lub nie jest liczbą, **gdy** zostanie wysłane `DELETE`, **wtedy** odpowiedź ma kod 404 bez wywołania usługi.
5. **Zakładając, że** żądanie nie ma poprawnego tokenu, **gdy** zostanie wysłane `DELETE`, **wtedy** odpowiedź ma kod 401 i produkt nie jest ukryty.
6. **Zakładając, że** encja `Product` ma flagę ukrycia (domyślnie produkt widoczny), **gdy** zostanie wygenerowana migracja, **wtedy** istnieje nowa migracja zarówno dla SQLite, jak i dla SQL Server, dodająca flagę do tabeli produktów; istniejące produkty po zastosowaniu migracji są widoczne; kolejna migracja wygenerowana bez zmian modelu jest pusta (US-01-07 pkt 9 i 12); zmiana nie wprowadza odwołań do konkretnego providera w projektach Model, Services i ViewModels (US-01-06 pkt 7).
7. **Zakładając, że** produkt jest ukryty, **gdy** zostanie zapisane (`POST`/`PUT /api/OrderApi` lub bezpośrednio `OrderService`) zamówienie z pozycją wskazującą ten produkt, **wtedy** produkt jest traktowany jak nieistniejący: `OrderService` zgłasza błąd walidacji, API odpowiada 400, a nic nie jest zapisane; istniejące zamówienia zawierające ukryty produkt nadal są zwracane z pozycjami i `TotalAmount` (pkt 3).
8. **Zakładając, że** `IProductService` udostępnia operację ukrywania produktu, **gdy** zostaną uruchomione testy jednostkowe usługi, **wtedy** obejmują scenariusze z pkt 1–3 i 7 po stronie usług (ukryty produkt nie jest zwracany przez `GetProduct` i `GetProducts`; zapis zamówienia z ukrytym produktem jest odrzucany; istniejące zamówienia pozostają nienaruszone) i przechodzą; dotychczasowe testy sprintu 1 pozostają bez zmian i przechodzą.

**Uwagi:** To zmiana modelu i migracji zweryfikowanych w sprincie 1 (US-01-03, US-01-07), wynikająca z decyzji Lidera; obejmuje też regułę w `OrderService` (US-01-11) dla ukrytych produktów. Zależy od US-02-05.

---

### US-02-08: AddressApi — adresy

**Status:** Zaakceptowana
**Źródło:** „Zadanie” (`AddressApiController`), rozdz. 3 (przykład `GET /api/Address` w Swaggerze)
**Priorytet:** Średni

**Jako** klient API
**chcę** pobierać, dodawać i modyfikować adresy
**aby** zarządzać adresami klientów i sklepów.

**Kryteria akceptacji:**
1. **Zakładając, że** w bazie są adresy i żądanie ma poprawny token, **gdy** zostanie wysłane `GET /api/AddressApi`, **wtedy** odpowiedź ma kod 200 i zawiera listę `AddressVm` o liczbie elementów równej liczbie adresów w bazie (bez parametrów filtrowania).
2. **Zakładając, że** istnieje adres o `Id` = X, **gdy** zostanie wysłane `GET /api/AddressApi/X`, **wtedy** odpowiedź ma kod 200 i zawiera `AddressVm`; dla nieistniejącego `Id` odpowiedź ma kod 404; dla `Id` < 1 lub nieliczbowego — 404 bez wywołania usługi.
3. **Zakładając, że** `AddOrUpdateAddressVm` zawiera poprawne dane bez `Id`, **gdy** zostanie wysłane `POST /api/AddressApi`, **wtedy** adres jest dodany jako nowy rekord, odpowiedź ma kod 201 z nagłówkiem `Location` i zawiera zapisane dane; `POST` z niepustym `Id` kończy się kodem 400.
4. **Zakładając, że** istnieje adres, **gdy** zostanie wysłane `PUT /api/AddressApi` z jego `Id` i zmienionymi danymi, **wtedy** dane są zaktualizowane (nie powstaje nowy rekord), a odpowiedź ma kod 200; dla `Id` nieistniejącego — 404; `PUT` bez `Id` — 400.
5. **Zakładając, że** ciało żądania jest niepoprawne walidacyjnie lub niepoprawne jako JSON, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400 i nic nie jest zapisane.
6. **Zakładając, że** żądanie nie ma poprawnego tokenu, **gdy** zostanie wysłane dowolne żądanie do `AddressApi`, **wtedy** odpowiedź ma kod 401.
7. **Zakładając, że** kontroler jest sprawdzony, **gdy** zostanie przejrzony, **wtedy** nie udostępnia `DELETE` (usuwanie adresów jest poza zakresem).

**Uwagi:** Zakres odpowiada `IAddressService` (zapis lub aktualizacja, pobranie jednego, lista). Zależy od US-02-01 i US-02-02.

---

### US-02-09: StoreApi — sklepy stacjonarne

**Status:** Zaakceptowana
**Źródło:** „Zadanie” (`StoreApiController`)
**Priorytet:** Średni

**Jako** klient API
**chcę** pobierać, dodawać i modyfikować sklepy stacjonarne
**aby** zarządzać siecią sklepów.

**Kryteria akceptacji:**
1. **Zakładając, że** w bazie są sklepy i żądanie ma poprawny token, **gdy** zostanie wysłane `GET /api/StoreApi`, **wtedy** odpowiedź ma kod 200 i zawiera listę `StoreVm` o liczbie elementów równej liczbie sklepów w bazie.
2. **Zakładając, że** istnieje sklep o `Id` = X, **gdy** zostanie wysłane `GET /api/StoreApi/X`, **wtedy** odpowiedź ma kod 200 i zawiera `StoreVm`; dla nieistniejącego `Id` — 404; dla `Id` < 1 lub nieliczbowego — 404 bez wywołania usługi.
3. **Zakładając, że** istnieje adres, **gdy** zostanie wysłane `POST /api/StoreApi` z poprawnym `AddOrUpdateStoreVm` wskazującym ten adres, **wtedy** sklep jest dodany jako nowy rekord z powiązaniem adresu, odpowiedź ma kod 201 z nagłówkiem `Location` i zawiera zapisane dane; `POST` z niepustym `Id` kończy się kodem 400.
4. **Zakładając, że** istnieje sklep, **gdy** zostanie wysłane `PUT /api/StoreApi` z jego `Id` i zmienionymi danymi, **wtedy** dane są zaktualizowane bez tworzenia nowego rekordu, a odpowiedź ma kod 200; dla `Id` nieistniejącego — 404; `PUT` bez `Id` — 400.
5. **Zakładając, że** ciało żądania jest niepoprawne walidacyjnie lub niepoprawne jako JSON albo wskazuje nieistniejący adres, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400, a nic nie jest zapisane.
6. **Zakładając, że** żądanie nie ma poprawnego tokenu, **gdy** zostanie wysłane dowolne żądanie do `StoreApi`, **wtedy** odpowiedź ma kod 401.
7. **Zakładając, że** kontroler jest sprawdzony, **gdy** zostanie przejrzony, **wtedy** nie udostępnia `DELETE`.

**Uwagi:** Zakres odpowiada `IStoreService`. Zależy od US-02-01 i US-02-02.

---

### US-02-10: OrderApi — zamówienia

**Status:** Zaakceptowana
**Źródło:** „Zadanie” (`OrderApiController`)
**Priorytet:** Wysoki

**Jako** klient API
**chcę** pobierać zamówienia oraz składać i modyfikować zamówienie z pozycjami
**aby** obsługiwać proces zakupowy klientów.

**Kryteria akceptacji:**
1. **Zakładając, że** w bazie są zamówienia i żądanie ma poprawny token, **gdy** zostanie wysłane `GET /api/OrderApi`, **wtedy** odpowiedź ma kod 200 i zawiera listę `OrderVm` o liczbie elementów równej liczbie zamówień w bazie (niezależnie od roli, bez filtrowania).
2. **Zakładając, że** istnieje zamówienie o `Id` = X, **gdy** zostanie wysłane `GET /api/OrderApi/X`, **wtedy** odpowiedź ma kod 200 i zawiera `OrderVm` z pozycjami i `TotalAmount`; dla nieistniejącego `Id` — 404; dla `Id` < 1 lub nieliczbowego — 404 bez wywołania usługi.
3. **Zakładając, że** istnieje klient i produkty, **gdy** zostanie wysłane `POST /api/OrderApi` z poprawnym `AddOrUpdateOrderVm` (co najmniej jedna pozycja), **wtedy** zamówienie jest zapisane wraz z pozycjami, w każdej pozycji zapisana jest aktualna cena produktu (cena zamrożona), odpowiedź ma kod 201 z nagłówkiem `Location` i zawiera `OrderVm` z `TotalAmount` równym sumie (cena zamrożona × ilość); `POST` z niepustym `Id` kończy się kodem 400.
4. **Zakładając, że** pozycja ma `Quantity` ≤ 0, `ProductId` ≤ 0 lub wskazujący nieistniejący produkt albo lista pozycji jest pusta, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400 z czytelnym komunikatem walidacji (z usługi), a nic nie jest zapisane.
5. **Zakładając, że** zamówienie wskazuje nieistniejącego klienta, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400, a nic nie jest zapisane.
6. **Zakładając, że** nie istnieje zamówienie o `Id` = 999, **gdy** zostanie wysłane `PUT /api/OrderApi` z `Id` = 999, **wtedy** odpowiedź ma kod 404; `PUT` bez `Id` kończy się kodem 400; poprawny `PUT` istniejącego zamówienia zwraca 200.
7. **Zakładając, że** ciało żądania jest puste lub niepoprawne jako JSON, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400.
8. **Zakładając, że** żądanie nie ma poprawnego tokenu, **gdy** zostanie wysłane dowolne żądanie do `OrderApi`, **wtedy** odpowiedź ma kod 401.
9. **Zakładając, że** kontroler jest sprawdzony, **gdy** zostanie przejrzony, **wtedy** nie udostępnia `DELETE`.
10. **Zakładając, że** produkt jest ukryty (US-02-07), **gdy** zostanie wysłane `POST` lub `PUT` z pozycją wskazującą ten produkt, **wtedy** odpowiedź ma kod 400 z komunikatem walidacji, a nic nie jest zapisane; `GET` istniejących zamówień z ukrytymi produktami nadal zwraca ich pozycje.

**Uwagi:** Reguły walidacji i wyliczania `TotalAmount` pozostają w `OrderService` (US-01-11). Zależy od US-02-01, US-02-02 i US-02-07.

---

### US-02-11: InvoiceApi — faktury

**Status:** Zaakceptowana
**Źródło:** „Zadanie” (`InvoiceApiController`)
**Priorytet:** Średni

**Jako** klient API
**chcę** pobierać faktury oraz wystawiać i modyfikować fakturę do zamówienia
**aby** dokumentować sprzedaż.

**Kryteria akceptacji:**
1. **Zakładając, że** w bazie są faktury i żądanie ma poprawny token, **gdy** zostanie wysłane `GET /api/InvoiceApi`, **wtedy** odpowiedź ma kod 200 i zawiera listę `InvoiceVm` o liczbie elementów równej liczbie faktur w bazie.
2. **Zakładając, że** istnieje faktura o `Id` = X, **gdy** zostanie wysłane `GET /api/InvoiceApi/X`, **wtedy** odpowiedź ma kod 200 i zawiera `InvoiceVm`; dla nieistniejącego `Id` — 404; dla `Id` < 1 lub nieliczbowego — 404 bez wywołania usługi.
3. **Zakładając, że** istnieje zamówienie bez faktury, **gdy** zostanie wysłane `POST /api/InvoiceApi` z poprawnym `AddOrUpdateInvoiceVm`, **wtedy** faktura jest zapisana i powiązana z zamówieniem, odpowiedź ma kod 201 z nagłówkiem `Location`, a `TotalAmount` faktury jest równe `TotalAmount` zamówienia (nie jest przyjmowane od klienta); `POST` z niepustym `Id` kończy się kodem 400.
4. **Zakładając, że** zamówienie ma już fakturę, **gdy** zostanie wysłane `POST` dla tego zamówienia, **wtedy** odpowiedź ma kod 409, a nic nie jest zapisane.
5. **Zakładając, że** faktura wskazuje nieistniejące zamówienie, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400, a nic nie jest zapisane.
6. **Zakładając, że** istnieje faktura, **gdy** zostanie wysłane `PUT /api/InvoiceApi` z jej `Id` i zmienionymi danymi, **wtedy** dane są zaktualizowane bez tworzenia nowego rekordu, a odpowiedź ma kod 200; dla nieistniejącego `Id` — 404; `PUT` bez `Id` — 400.
7. **Zakładając, że** ciało żądania jest niepoprawne walidacyjnie lub jako JSON, **gdy** zostanie wysłane `POST` lub `PUT`, **wtedy** odpowiedź ma kod 400, a nic nie jest zapisane.
8. **Zakładając, że** żądanie nie ma poprawnego tokenu, **gdy** zostanie wysłane dowolne żądanie do `InvoiceApi`, **wtedy** odpowiedź ma kod 401.
9. **Zakładając, że** kontroler jest sprawdzony, **gdy** zostanie przejrzony, **wtedy** nie udostępnia `DELETE`.

**Uwagi:** Model wejściowy nie zawiera `TotalAmount` (US-01-12 pkt 8). Zależy od US-02-01 i US-02-02.

---

### US-02-12: Serwer deweloperski klienta React kierujący /api i /swagger do serwera

**Status:** Zaakceptowana
**Źródło:** Rozdz. 3 pkt 9 (`setupProxy.js`: ścieżki `/api` i `/swagger`); decyzja Lidera: odpowiednikiem w Vite jest `server.proxy`
**Priorytet:** Niski

**Jako** programista pracujący nad klientem React
**chcę** aby serwer deweloperski klienta (`npm run dev`) przekazywał żądania `/api` i `/swagger` do `WebStore.Web`
**aby** klient mógł w przyszłości wywoływać API bez CORS i bez przechwytywania tych ścieżek przez routing klienta.

**Kryteria akceptacji:**
1. **Zakładając, że** uruchomiono `dotnet run --project WebStore.Web` (Development) oraz `npm run dev` w `WebStore.Web/ClientApp`, **gdy** w przeglądarce zostanie otwarty adres serwera deweloperskiego klienta z końcówką `/swagger/index.html`, **wtedy** wyświetlana jest dokumentacja Swagger serwera, a nie aplikacja React.
2. **Zakładając, że** działa jak w pkt 1, **gdy** zostanie wysłane przez serwer deweloperski klienta `POST /api/AccountApi/Login` (poprawne dane), **wtedy** odpowiedź pochodzi z serwera API (kod 200 z tokenem), bez błędu CORS i bez włączania CORS na serwerze.
3. **Zakładając, że** działa jak w pkt 1, **gdy** zostaną otwarte `/` i `/counter` w serwerze deweloperskim klienta, **wtedy** wyświetlane są strony klienta bez zmian względem US-01-17.
4. **Zakładając, że** serwer API nie działa, **gdy** zostanie wysłane żądanie `/api/...` przez serwer deweloperski klienta, **wtedy** odpowiedź jest błędem bramy (502 lub równoważnym), a aplikacja kliencka się nie zawiesza.
5. **Zakładając, że** zmiana jest wprowadzona, **gdy** zostanie wykonane `npm run build` w `ClientApp`, **wtedy** polecenie kończy się kodem 0, bez błędów i ostrzeżeń (US-01-17 pkt 13), a w kodzie klienta nie ma wywołań API (`fetch`, `XMLHttpRequest`, biblioteki HTTP).
6. **Zakładając, że** adres docelowy serwera API jest ustawiany w konfiguracji klienta, **gdy** zostaną przejrzone pliki repozytorium, **wtedy** nie występują w nich sekrety ani dane połączenia z bazą.
7. **Zakładając, że** zmiana jest wprowadzona, **gdy** zostanie sprawdzony `git diff` po stronie klienta, **wtedy** zmiany w `WebStore.Web/ClientApp` ograniczają się do konfiguracji proxy deweloperskiego (`vite.config.js`) oraz ewentualnie opisu w `README`.

**Uwagi:** Zależy od US-02-01 i US-02-04. Klient React poza proxy deweloperskim pozostaje bez zmian (decyzja Lidera).

---

### US-02-13: Testy integracyjne API i dostosowanie testów architektury

**Status:** Zaakceptowana
**Źródło:** Rozdz. 3 pkt 10 (testowanie endpointów); decyzja Lidera: testy integracyjne `WebApplicationFactory` z referencją Tests→Web; zasady wspólne projektu (brak regresji sprintu 1)
**Priorytet:** Średni

**Jako** programista
**chcę** automatyczne testy integracyjne kluczowych zachowań API (logowanie, autoryzacja, kody odpowiedzi, brak logiki w kontrolerach)
**aby** weryfikować kryteria US-02-01–US-02-11 bez ręcznego klikania w Swaggerze i wykrywać regresję.

**Kryteria akceptacji:**
1. **Zakładając, że** projekt `WebStore.Tests` ma testy integracyjne uruchamiające aplikację w pamięci przez `WebApplicationFactory`, **gdy** zostaną sprawdzone referencje projektu, **wtedy** `WebStore.Tests` odwołuje się dodatkowo do `WebStore.Web` (schemat zależności: Tests→Services, Model, DAL, Web).
2. **Zakładając, że** zmieniony schemat zależności wchodzi do zakresu sprintu, **gdy** zostaną uruchomione testy architektury, **wtedy** są dostosowane do nowej referencji Tests→Web (pozostałe reguły zależności, w tym brak referencji przeciwnych, nie są osłabione), a wymaganie zmiany zapisu w `CLAUDE.md` jest zgłoszone Liderowi w raporcie implementacji (edycję wykonuje główna sesja, nie programista ani analityk).
3. **Zakładając, że** dodano testy architektury i konwencji dla kontrolerów, **gdy** zostaną uruchomione, **wtedy** sprawdzają, że kontrolery dziedziczą po `BaseApiController`, mają sufiks `ApiController` i nie zależą od `ApplicationDbContext`; dotychczasowe testy konwencji i architektury przechodzą po dostosowaniu (obejmującym zmianę założenia „brak kontrolerów”, US-01-01 pkt 8) bez osłabiania pozostałych asercji.
4. **Zakładając, że** każdy test integracyjny ma własną, odrębną bazę SQLite w pamięci z danymi startowymi i własną konfigurację JWT (klucz, `Issuer`, `Audience`), **gdy** testy zostaną uruchomione w dowolnej kolejności lub równolegle, **wtedy** wynik każdego nie zależy od innych (US-01-10 pkt 6).
5. **Zakładając, że** testy są zaimplementowane, **gdy** zostaną przejrzone, **wtedy** obejmują co najmniej:
   - logowanie poprawne (formularz i JSON) oraz błędne (401, 400) — US-02-03 pkt 6, 8, 9;
   - 401 dla chronionego endpointu bez tokenu, z tokenem o błędnym podpisie, wygasłym oraz o innym `Issuer`/`Audience` — US-02-02 pkt 4;
   - produkt: `GET` lista i po `Id`, `POST` (201 i `Location`), `PUT` (200, 404, 400 bez `Id`), `DELETE` (204, 404, produkt ukryty niewidoczny w `GET`, zamówienia nietknięte) — US-02-05–US-02-07;
   - po co najmniej jednym scenariuszu pozytywnym i negatywnym dla `AddressApi`, `StoreApi`, `OrderApi` (w tym 400 dla walidacji pozycji) i `InvoiceApi` (w tym 409 dla drugiej faktury);
   - 404 dla `/api/NieIstniejacyKontroler` i brak Swaggera poza Development — US-02-01 pkt 3, US-02-04 pkt 6;
   - tworzenie kont testowych tylko w Development i bez duplikatów po ponownym starcie — US-02-03 pkt 1–3.
6. **Zakładając, że** dodano testy, **gdy** zostanie wykonane `dotnet test`, **wtedy** wszystkie testy (istniejące i nowe) kończą się wynikiem „Passed”, polecenie zwraca kod 0, a testy wariantu SQL Server są pomijane, dopóki `WEBSTORE_TEST_SQLSERVER` nie zawiera ciągu połączenia; `dotnet build` i `dotnet test` nie wymagają Node.js.
7. **Zakładając, że** dotychczasowe testy sprintu 1 istnieją, **gdy** zostanie porównana lista testów przed i po zmianie, **wtedy** żaden dotychczasowy test nie został usunięty, przemianowany ani osłabiony (poza dostosowaniem testów architektury z pkt 2–3, opisanym w raporcie implementacji).
8. **Zakładając, że** nowe testy są dodane, **gdy** zostaną sprawdzone nazwy metod, **wtedy** nie zawierają identyfikatorów US, a `README` opisuje sposób uruchomienia wszystkich testów, pojedynczej metody i klasy (`--filter-method`, `--filter-class`).

**Uwagi:** Instrukcja wymaga tylko ręcznego testu Swaggerem/Postmanem; zakres testowy wynika z decyzji Lidera. Zmiana schematu zależności projektów dotyka `CLAUDE.md` (sekcja „Struktura katalogów”) — edycji dokona główna sesja po akceptacji. Zależy od US-02-01–US-02-11.

---

## Macierz pokrycia

| Wymaganie z dokumentu | Identyfikatory US |
|---|---|
| 1. Wstęp do REST i REST API (opis teoretyczny: styl REST, bezstanowość, klient–serwer) | US-02-01, US-02-02 (bezstanowe API z tokenem); część teoretyczna bez osobnych wymagań |
| 2. Web API, metody HTTP, kody odpowiedzi | Tabela kodów w uwagach ogólnych; US-02-01 pkt 6–7, US-02-03, US-02-05–US-02-11 |
| 3 pkt 1 Usunięcie plików szablonu (`WeatherForecastController`, `WeatherForecast`) | US-02-01 (projekt nie zawiera przykładowych kontrolerów szablonu; pliki te nie istnieją w repozytorium) |
| 3 pkt 2 Pakiety JwtBearer, Newtonsoft.Json, Mvc.NewtonsoftJson | US-02-02 (JwtBearer); Newtonsoft.Json nie jest używany (decyzja Lidera: `System.Text.Json`, US-02-01 pkt 8) |
| 3 pkt 3 `appsettings.json` (ConnectionStrings LocalDB, `JwtOptions`) | US-02-02 pkt 1, 5–7 (`JwtOptions`: Issuer, Audience, TokenExpirationMinutes w pliku; klucz poza repozytorium); ciąg LocalDB nie trafia do repozytorium (mechanizm bazy ze sprintu 1 bez zmian) |
| 3 pkt 4 `Program.cs` (AutoMapper, DbContext, Identity, JWT, kontrolery, Swagger, routing, HTTPS) | US-02-01 pkt 8–9, US-02-02, US-02-04; Antiforgery, trasa MVC, `AddControllersWithViews` i CORS pominięte (decyzje Lidera) |
| 3 pkt 5 `JwtOptionsVm`, `LoginUserVm` | US-02-02 pkt 1, US-02-03 pkt 5 |
| 3 pkt 6 `BaseApiController` | US-02-01 pkt 1 |
| 3 pkt 7 `AccountApiController` (Login, JWT, claims, `access_token`/`expires_in`) | US-02-03 pkt 6–13 |
| Konta użytkowników do logowania (brak w dokumencie; przykład `a1@eg.eg`) | US-02-03 pkt 1–4 (decyzja Lidera: konta testowe w Development, role Admin i Customer) |
| 3 CRUD produktu: C, R, U, D | US-02-05 (R), US-02-06 (C, U), US-02-07 (D jako ukrycie) |
| 3 pkt 8 `ProductApiController` (GET, GET {id}, PUT, POST, DELETE) | US-02-05, US-02-06, US-02-07 |
| 3 pkt 9 Wyłączenie `/api` i `/swagger` z routingu klienta (`setupProxy.js`) | US-02-01 pkt 3–4, US-02-04 pkt 5, US-02-12 (proxy w `vite.config.js`) |
| 3 pkt 10 Uruchomienie `dotnet run`, Swagger, `/swagger/index.html` | US-02-04, US-02-01 pkt 10 |
| 3 Postman (żądania, `Authorization: Bearer`, wyłączenie weryfikacji SSL) | Weryfikacja ręczna jako uzupełnienie; konfiguracja Postmana poza zakresem (czynność Lidera/studenta); automatyzacja: US-02-13 |
| 3 Token w Swaggerze (okno Authorize) | US-02-04 pkt 3–4 |
| Zadanie: `OrderApiController` | US-02-10 |
| Zadanie: `InvoiceApiController` | US-02-11 |
| Zadanie: `StoreApiController` | US-02-09 |
| Zadanie: `AddressApiController` | US-02-08 |
| Zadanie: brak logiki biznesowej w kontrolerach (odbiór i walidacja, wywołanie serwisu, zwrot wyniku) | US-02-01 pkt 5, US-02-13 pkt 3 |
| Zadanie: API jako warstwa service backend dla wielu klientów (mobilne, desktopowe, SPA) | Cel sprintu; US-02-01 pkt 2, US-02-12; klient React bez wywołań API (decyzja Lidera) |
| Niefunkcjonalne: brak sekretów w repozytorium, bezpieczne logowanie (bez haseł w logach) | US-02-02 pkt 7, US-02-03 pkt 4, 11 |
| Niefunkcjonalne: brak regresji sprintu 1, `dotnet build` bez ostrzeżeń, praca bez Node.js | US-02-01 pkt 10, US-02-13 pkt 6–7 |
| Decyzja Lidera: sprint 2 zastępuje US-01-01 pkt 8; dostosowanie testów architektury i konwencji | US-02-13 pkt 2–3, uwagi ogólne |
| Decyzja Lidera: testy integracyjne z referencją Tests→Web, zmiana schematu zależności | US-02-13 pkt 1–2 |
