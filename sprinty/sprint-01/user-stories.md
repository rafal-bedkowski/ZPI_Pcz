# Sprint 01 — User stories

**Źródło wymagań:** `wymagania/ZPILab1.pdf` (Zaawansowane Programowanie Internetowe, Laboratorium 1)

Uwagi ogólne:
- Dokument wymagań jest instrukcją laboratoryjną. Rodzaje technologii wynikają z niego wprost: .NET (C#), Entity Framework Core, AutoMapper, framework testowy xUnit, klient React. Wersje mają być aktualne i stabilne: .NET 10 (LTS), EF Core 10.x, AutoMapper 16.x, xunit.v3. Dokładne numery wersji pakietów ustala programista poleceniem `dotnet package search` i nie są one kryteriami akceptacji. Przykładowy kod z instrukcji (m.in. testy) wymaga dostosowania do wybranych wersji, w szczególności do xUnit v3.
- Baza danych: domyślnym providerem jest SQLite (działa na macOS i Windows bez serwera). SQL Server (wskazany w instrukcji) jest wariantem alternatywnym włączanym konfiguracją, bez zmian w modelu i usługach.
- Nazwa solucji: `WebStore`. Nazwy projektów: `WebStore.Model`, `WebStore.DAL`, `WebStore.Services`, `WebStore.ViewModels`, `WebStore.Web`, `WebStore.Tests`.
- Cała solucja, w tym projekt testów, używa jednej wersji platformy .NET.
- Zasady zwracania wyników przez wszystkie usługi: brak dopasowania do filtra daje wynik `null`; zapis z `Id` wskazującym nieistniejący rekord kończy się wyjątkiem „nie znaleziono”; `Id` puste lub równe 0 oznacza dodanie nowego rekordu.
- Poza zakresem user stories (czynności Lidera lub osobne zlecenia Lidera): klonowanie repozytorium, gałęzie, token, commit i push, instalacja narzędzi (VS Code, rozszerzenia, SDK), skrypty konfiguracji środowiska (`setup.sh`, `setup.ps1`) oraz sekcja „Środowisko” w `CLAUDE.md`. Na maszynie z Windows Lider ma uprawnienia administratora, więc `setup.ps1` może instalować .NET SDK globalnie.
- Poza zakresem sprintu: testy integracyjne i testy GUI (Selenium), mierzalny próg pokrycia kodu, kontrolery API i Swagger.

---

## Lista user stories

### US-01-01: Struktura solucji WebStore

**Status:** Zweryfikowana
**Źródło:** Rozdz. 2 (pkt 4–13), rozdz. 3 pkt 1
**Priorytet:** Wysoki

**Jako** programista
**chcę** mieć wielowarstwową solucję WebStore z poprawnymi referencjami między projektami
**aby** móc rozwijać model, dostęp do danych, usługi i warstwę webową w rozdzielonych projektach.

**Kryteria akceptacji:**
1. **Zakładając, że** repozytorium zawiera solucję, **gdy** zostanie wylistowana jej zawartość, **wtedy** zawiera ona projekty: `WebStore.Model` (class library), `WebStore.DAL` (class library), `WebStore.Services` (class library), `WebStore.ViewModels` (class library), `WebStore.Web` (aplikacja webowa z klientem React) oraz `WebStore.Tests` (projekt testów, patrz US-01-10).
2. **Zakładając, że** solucja istnieje, **gdy** sprawdzone zostaną referencje projektów, **wtedy** występują dokładnie: DAL→Model; ViewModels→Model; Services→Model, DAL, ViewModels; Web→Model, DAL, ViewModels, Services; Tests→Services, Model, DAL. Nie występują referencje przeciwne (np. Model→DAL).
3. **Zakładając, że** zainstalowane jest .NET SDK, **gdy** w katalogu solucji zostanie wykonane `dotnet build`, **wtedy** kompilacja kończy się sukcesem z 0 błędów i 0 ostrzeżeń.
4. **Zakładając, że** kompilacja się powiodła, **gdy** zostanie wykonane `dotnet run --project WebStore.Web`, **wtedy** aplikacja startuje bez wyjątku i odpowiada na żądanie HTTP do strony głównej kodem 200.
5. **Zakładając, że** wszystkie projekty istnieją, **gdy** sprawdzone zostaną pliki `*.csproj`, **wtedy** wszystkie projekty (w tym `WebStore.Tests`) mają tę samą docelową wersję platformy .NET, a użyte pakiety (EF Core, AutoMapper, Identity, framework testowy) są w aktualnych stabilnych wersjach zgodnych z tą platformą (żaden pakiet nie jest oznaczony jako przestarzały).
6. **Zakładając, że** w projekcie Web brakuje katalogu `node_modules` w konfiguracji Debug, **gdy** zostanie wykonane `dotnet build` bez zainstalowanego Node.js, **wtedy** build kończy się błędem z komunikatem o wymaganym Node.js (zachowanie z instrukcji).
7. **Zakładając, że** repozytorium jest sklonowane, **gdy** zostanie sprawdzony katalog główny solucji, **wtedy** istnieje w nim plik `global.json` przypinający wersję .NET SDK zgodną z docelową platformą solucji.
8. **Zakładając, że** projekt Web uruchamia się, **gdy** zostanie sprawdzona jego konfiguracja, **wtedy** usługi z tego sprintu (`IProductService`, `IOrderService`, `IInvoiceService`, `IStoreService`, `IAddressService`) są zarejestrowane w kontenerze zależności i dają się rozwiązać; projekt nie zawiera kontrolerów API ani konfiguracji Swagger.

**Uwagi:** Wymóg wersji (.NET 10 LTS, EF Core 10.x, AutoMapper 16.x, xunit.v3) — patrz uwagi ogólne.

---

### US-01-02: Model użytkowników i adresów

**Status:** Zweryfikowana
**Źródło:** Rozdz. 3 pkt 2, 3, 4 (diagram UML)
**Priorytet:** Wysoki

**Jako** programista
**chcę** model klas User, Customer, Supplier, StationaryStoreEmployee i Address
**aby** system mógł przechowywać dane osób oraz ich adresów.

**Kryteria akceptacji:**
1. **Zakładając, że** projekt Model istnieje, **gdy** zostanie sprawdzona klasa `User`, **wtedy** dziedziczy po `IdentityUser<int>` i zawiera właściwości `FirstName` (string), `LastName` (string), `RegistrationDate` (DateTime).
2. **Zakładając, że** klasa `User` istnieje, **gdy** zostaną sprawdzone klasy `Customer`, `Supplier` i `StationaryStoreEmployee`, **wtedy** każda dziedziczy po `User`.
3. **Zakładając, że** klasa `Supplier` istnieje, **gdy** zostanie sprawdzona, **wtedy** zawiera kolekcję `Products` typu `IList<Product>`.
4. **Zakładając, że** klasa `Customer` istnieje, **gdy** zostanie sprawdzona, **wtedy** zawiera kolekcję `Orders` typu `IList<Order>` oraz dwie osobne właściwości adresowe: adres rozliczeniowy (`BillingAddress`) i adres wysyłki (`ShippingAddress`) typu `Address` (nie string). Każdy z nich wskazuje własny rekord adresu — rekord adresu nie jest współdzielony między klientami ani między adresem rozliczeniowym a adresem wysyłki.
5. **Zakładając, że** klasa `Address` istnieje, **gdy** zostanie sprawdzona, **wtedy** zawiera dokładnie pola: `Id`, `Street`, `BuildingNumber`, `ApartmentNumber`, `City`, `PostalCode`, `Country`.
6. **Zakładając, że** klasa `StationaryStoreEmployee` istnieje, **gdy** zostanie sprawdzona, **wtedy** (oprócz pól `User`) zawiera `StationaryStoreId` oraz powiązanie ze sklepem stacjonarnym.

**Uwagi:** Klasy w przestrzeni nazw `WebStore.Model.DataModels`. Zależy od US-01-03, US-01-04 i US-01-05 (typy Product, Order, StationaryStore).

---

### US-01-03: Model produktów, kategorii i stanów magazynowych

**Status:** Zweryfikowana
**Źródło:** Rozdz. 3 pkt 2, 5 (diagram UML)
**Priorytet:** Wysoki

**Jako** programista
**chcę** model klas Product, Category i ProductStock
**aby** system mógł przechowywać asortyment, jego kategorie oraz dostępne ilości w sklepach.

**Kryteria akceptacji:**
1. **Zakładając, że** projekt Model istnieje, **gdy** zostanie sprawdzona klasa `Product`, **wtedy** zawiera: `Id` (int), `Name` (string), `Description` (string), `ImageBytes` (byte[]), `Price` (decimal), `Weight` (float), powiązanie z `Supplier` oraz powiązanie z `Category` (obiekt `Category` zamiast pola tekstowego z diagramu), a także klucze obce `CategoryId` i `SupplierId` (używane w danych testowych z rozdz. 5.2).
2. **Zakładając, że** klasa `Category` istnieje, **gdy** zostanie sprawdzona, **wtedy** zawiera dokładnie pola: `Id`, `Name`, `Tag`.
3. **Zakładając, że** klasa `ProductStock` istnieje, **gdy** zostanie sprawdzona, **wtedy** zawiera własny klucz `Id`, `ProductId` z powiązaniem z `Product`, powiązanie ze sklepem stacjonarnym (`StationaryStore`, z kluczem obcym) oraz `Quantity` (int).

---

### US-01-04: Model zamówień, pozycji zamówień i faktur

**Status:** Zweryfikowana
**Źródło:** Rozdz. 3 pkt 2, 6 (diagram UML)
**Priorytet:** Wysoki

**Jako** programista
**chcę** model klas Order, OrderProduct i Invoice
**aby** system mógł przechowywać zamówienia klientów, ich pozycje i faktury.

**Kryteria akceptacji:**
1. **Zakładając, że** projekt Model istnieje, **gdy** zostanie sprawdzona klasa `Order`, **wtedy** zawiera: `Id` (int), `Customer`, `OrderDate` (DateTime), `DeliveryDate` (DateTime), `TotalAmount` (decimal, tylko do odczytu — `get`), `TrackingNumber` (long) oraz powiązanie z `Invoice`.
2. **Zakładając, że** klasa `OrderProduct` istnieje, **gdy** zostanie sprawdzona, **wtedy** zawiera powiązanie z `Order`, powiązanie z `Product`, `Quantity` (int) oraz pole ceny jednostkowej (decimal) zamrożonej w chwili złożenia zamówienia; kluczem głównym jest para (`OrderId`, `ProductId`).
3. **Zakładając, że** klasa `Invoice` istnieje, **gdy** zostanie sprawdzona, **wtedy** zawiera dokładnie pola: `Id`, `InvoiceNumber`, `IssueDate`, `PaymentDueDate`, `TotalAmount` oraz powiązanie z zamówieniem.
4. **Zakładając, że** zamówienie ma pozycje, **gdy** odczytane zostanie `TotalAmount`, **wtedy** jest ono równe sumie (zamrożona cena jednostkowa × ilość) wszystkich pozycji zamówienia.
5. **Zakładając, że** zamówienie ma pozycje o cenach z częścią ułamkową (np. 19,99 × 3 oraz 0,01 × 7), **gdy** zostanie wyznaczone `TotalAmount` i zamówienie zostanie zapisane oraz odczytane z bazy, **wtedy** wartość jest dokładna do 2 miejsc po przecinku (bez błędów zaokrągleń typu double), na każdym z providerów bazy (SQLite i SQL Server).
6. **Zakładając, że** zamówienie zostało złożone z produktem o cenie 100, **gdy** cena produktu zostanie następnie zmieniona na 200, **wtedy** cena w pozycji zamówienia i `TotalAmount` zamówienia pozostają bez zmian.

---

### US-01-05: Model sklepu stacjonarnego

**Status:** Zweryfikowana
**Źródło:** Rozdz. 3 pkt 4
**Priorytet:** Średni

**Jako** programista
**chcę** klasę StationaryStore powiązaną z adresem, pracownikami i stanami magazynowymi
**aby** system mógł reprezentować sklepy stacjonarne.

**Kryteria akceptacji:**
1. **Zakładając, że** projekt Model istnieje, **gdy** zostanie sprawdzona klasa `StationaryStore`, **wtedy** zawiera dokładnie pola `Id`, `Name`, powiązanie z `Address` (z kluczem obcym) oraz kolekcje `StationaryStoreEmployee` i `ProductStock`.

---

### US-01-06: Konfiguracja kontekstu bazy danych i relacji (EF Core)

**Status:** Zweryfikowana
**Źródło:** Rozdz. 4 pkt 1, 2 oraz akapit o TPH
**Priorytet:** Wysoki

**Jako** programista
**chcę** kontekst `ApplicationDbContext` ze zdefiniowanymi relacjami i dziedziczeniem TPH
**aby** model danych był poprawnie odwzorowany w bazie.

**Kryteria akceptacji:**
1. **Zakładając, że** projekt DAL istnieje, **gdy** zostanie sprawdzony kontekst `ApplicationDbContext` (przestrzeń nazw `WebStore.DAL.EF`), **wtedy** udostępnia zbiory encji, w tym `Products`, oraz obsługuje tożsamość z kluczem `int` i rolami `IdentityRole<int>`.
2. **Zakładając, że** model jest skonfigurowany, **gdy** zostanie wygenerowany schemat bazy, **wtedy** istnieją relacje jeden-do-wielu (strona „1” jest pierwsza w parze): Customer–Order, Supplier–Product, Product–ProductStock, Category–Product, StationaryStore–StationaryStoreEmployee, StationaryStore–ProductStock oraz dwie osobne relacje Customer–Address (adres rozliczeniowy i adres wysyłki), przy czym jeden rekord adresu nie jest współdzielony przez wielu klientów.
3. **Zakładając, że** model jest skonfigurowany, **gdy** zostanie wygenerowany schemat, **wtedy** relacja StationaryStore–Address oraz relacja Invoice–Order są relacjami jeden-do-jednego (jedna faktura na jedno zamówienie, jeden adres na jeden sklep).
4. **Zakładając, że** model jest skonfigurowany, **gdy** zostanie wygenerowany schemat, **wtedy** relacja wiele-do-wielu Order–Product jest zrealizowana przez encję `OrderProduct` z kluczem złożonym (`OrderId`, `ProductId`).
5. **Zakładając, że** model jest skonfigurowany, **gdy** zostanie wygenerowany schemat, **wtedy** User, Customer, Supplier i StationaryStoreEmployee są odwzorowane w jednej tabeli (strategia TPH) z kolumną rozróżniającą typ.
6. **Zakładając, że** baza jest utworzona, **gdy** zostanie podjęta próba zapisu produktu z nieistniejącym `CategoryId` lub `SupplierId`, **wtedy** zapis kończy się wyjątkiem naruszenia klucza obcego, na każdym z providerów (SQLite i SQL Server).
7. **Zakładając, że** zmieniono provider bazy w konfiguracji, **gdy** zostanie sprawdzony kod projektów Model, Services i ViewModels, **wtedy** nie zawiera on odwołań do pakietu ani typów konkretnego providera (SQLite, SQL Server) — zależność od providera jest ograniczona do konfiguracji kontekstu i migracji.

**Uwagi:** Zależy od US-01-02–US-01-05.

---

### US-01-07: Provider bazy danych, migracje i utworzenie bazy

**Status:** Zweryfikowana
**Źródło:** Rozdz. 1.2, rozdz. 4 (wstęp i akapit końcowy); decyzja Lidera o domyślnym providerze SQLite
**Priorytet:** Wysoki

**Jako** programista
**chcę** wybierać provider bazy konfiguracją (domyślnie SQLite, alternatywnie SQL Server) i mieć dla każdego z nich migracje
**aby** baza odpowiadała modelowi na macOS i Windows bez serwera, a w razie wymogu prowadzącego dało się przejść na SQL Server bez zmian w modelu i usługach.

**Kryteria akceptacji:**

Wspólne:
1. **Zakładając, że** aplikacja nie ma jawnie ustawionego providera, **gdy** zostanie uruchomiona, **wtedy** używa SQLite.
2. **Zakładając, że** w konfiguracji wskazano provider SQL Server wraz z danymi połączenia, **gdy** aplikacja zostanie uruchomiona, **wtedy** używa SQL Server, a kod modelu, view modeli i usług nie wymaga żadnych zmian.
3. **Zakładając, że** w konfiguracji wskazano nieobsługiwaną nazwę providera, **gdy** aplikacja startuje, **wtedy** kończy się czytelnym błędem wymieniającym obsługiwane wartości (SQLite, SQL Server), a nie niezwiązanym wyjątkiem.
4. **Zakładając, że** zapisane są produkty o cenach 19,99, 0,01 i 1000,00 oraz zamówienia z kilkoma pozycjami, **gdy** zostanie wykonane sumowanie cen, sortowanie rosnące i malejące po cenie, filtr zakresu cen (np. cena > 19,98) oraz wyliczenie `TotalAmount`, **wtedy** wyniki są poprawne i identyczne na SQLite i na SQL Server (SQLite nie ma natywnego typu decimal, więc operacje na cenach nie mogą dawać wyników zależnych od providera).
5. **Zakładając, że** repozytorium jest sprawdzone, **gdy** zostaną przejrzone jego pliki i historia konfiguracji, **wtedy** ciąg połączenia SQL Server ani hasło nie występują w żadnym pliku repozytorium; wartości te są odczytywane wyłącznie z user-secrets lub zmiennej środowiskowej.

Wariant (a) — SQLite (domyślny):
6. **Zakładając, że** nie istnieje plik bazy SQLite, **gdy** zostanie zastosowana migracja SQLite, **wtedy** polecenie kończy się sukcesem, plik bazy zostaje utworzony w katalogu projektu `WebStore.Web`, a w bazie istnieją tabele wszystkich encji z US-01-02–US-01-05 oraz tabela łącząca OrderProduct z kluczami obcymi relacji z US-01-06.
7. **Zakładając, że** plik bazy SQLite został utworzony, **gdy** zostanie sprawdzone polecenie `git status`, **wtedy** plik bazy nie jest widoczny jako nieśledzony, ponieważ jest wykluczony wpisem w `.gitignore`.
8. **Zakładając, że** model nie zmienił się od ostatniej migracji SQLite, **gdy** zostanie wygenerowana kolejna migracja, **wtedy** jest ona pusta.
9. **Zakładając, że** ścieżka pliku bazy jest niedostępna do zapisu, **gdy** zostanie zastosowana migracja, **wtedy** polecenie kończy się błędem dostępu, a w bazie nie pozostają częściowe zmiany.

Wariant (b) — SQL Server (alternatywny):
10. **Zakładając, że** działa instancja SQL Server (LocalDB lub kontener) i skonfigurowano połączenie, **gdy** zostanie zastosowana migracja SQL Server, **wtedy** polecenie kończy się sukcesem, a w bazie istnieją te same tabele i klucze obce co w wariancie (a).
11. **Zakładając, że** model nie zmienił się od ostatniej migracji SQL Server, **gdy** zostanie wygenerowana kolejna migracja, **wtedy** jest ona pusta.
12. **Zakładając, że** serwer SQL Server jest niedostępny, **gdy** zostanie zastosowana migracja, **wtedy** polecenie kończy się błędem połączenia, a w bazie nie pozostają częściowe zmiany.

**Uwagi:** Lider ustala z prowadzącym, czy SQL Server jest obowiązkowy; do tego czasu oba warianty pozostają w zakresie.

---

### US-01-08: Konwencje warstwy usług, view modele i mapowanie

**Status:** Zweryfikowana
**Źródło:** Rozdz. 5.1 pkt 1–7
**Priorytet:** Wysoki

**Jako** programista
**chcę** wspólną infrastrukturę usług (foldery, konwencje nazw, BaseService, mapowanie view modeli)
**aby** wszystkie usługi były spójne i testowalne.

**Kryteria akceptacji:**
1. **Zakładając, że** projekt Services istnieje, **gdy** zostanie sprawdzona jego struktura, **wtedy** zawiera foldery `Interfaces` i `ConcreteServices`.
2. **Zakładając, że** projekt ViewModels istnieje, **gdy** zostanie sprawdzona jego struktura, **wtedy** wszystkie view modele leżą w folderze `VM` (przestrzeń nazw `WebStore.ViewModels.VM`), a nazwy ich klas kończą się `Vm`.
3. **Zakładając, że** interfejsy usług istnieją, **gdy** zostaną sprawdzone, **wtedy** są publiczne, nazwy zaczynają się od `I` i kończą na `Service`.
4. **Zakładając, że** istnieje abstrakcyjna klasa `BaseService` w `ConcreteServices`, **gdy** zostanie sprawdzona, **wtedy** przyjmuje w konstruktorze `ApplicationDbContext`, `IMapper` i `ILogger` oraz udostępnia je klasom pochodnym jako pola chronione tylko do odczytu; każda usługa dziedziczy po niej.
5. **Zakładając, że** projekt Services używa AutoMapper i logowania, **gdy** zostanie sprawdzony, **wtedy** istnieje profil mapowania `MainProfile` w przestrzeni `WebStore.Services.Configuration.Profiles` (wymagany przez konfigurację z rozdz. 5.2) obejmujący mapowania encja ↔ view model dla wszystkich usług z tego sprintu.
6. **Zakładając, że** view model `AddOrUpdateProductVm` ma adnotacje `[Required]`, **gdy** walidowany jest obiekt z pustą wartością pola wymaganego (np. `Name`), **wtedy** walidacja zwraca błąd dla tego pola.
7. **Zakładając, że** skonfigurowano mapowania, **gdy** zostanie wykonana weryfikacja konfiguracji AutoMapper, **wtedy** nie zgłasza błędów niezmapowanych pól.

---

### US-01-09: Usługa produktów (ProductService)

**Status:** Zweryfikowana
**Źródło:** Rozdz. 5.1 pkt 3–8
**Priorytet:** Wysoki

**Jako** programista
**chcę** usługę `ProductService` z interfejsem `IProductService`
**aby** dodawać, modyfikować i pobierać produkty przez view modele.

**Kryteria akceptacji:**
1. **Zakładając, że** `IProductService` istnieje, **gdy** zostanie sprawdzony, **wtedy** deklaruje `AddOrUpdateProduct(AddOrUpdateProductVm)` zwracającą `ProductVm`, `GetProduct(Expression<Func<Product,bool>>)` zwracającą `ProductVm` oraz `GetProducts(Expression<Func<Product,bool>>? = null)` zwracającą `IEnumerable<ProductVm>`.
2. **Zakładając, że** view modele istnieją, **gdy** zostaną sprawdzone, **wtedy** `AddOrUpdateProductVm` zawiera `Id` (int?), `Description`, `ImageBytes`, `Name`, `Price`, `Weight`, `CategoryId`, `SupplierId`, a `ProductVm` zawiera `Description`, `ImageBytes`, `Name`, `Price`, `Weight`, `Quantity`.
3. **Zakładając, że** `Id` w `AddOrUpdateProductVm` jest puste lub równe 0, **gdy** zostanie wywołane `AddOrUpdateProduct` z poprawnymi danymi, **wtedy** produkt jest dodany do bazy jako nowy rekord i zwrócony `ProductVm` ma te same wartości `Name` i `Price`.
4. **Zakładając, że** istnieje produkt o `Id` = 1, **gdy** zostanie wywołane `AddOrUpdateProduct` z `Id` = 1 i `Price` = 2000, **wtedy** produkt jest zmodyfikowany (nie powstaje nowy), a zwrócony `Price` = 2000.
5. **Zakładając, że** nie istnieje produkt o `Id` = 999, **gdy** zostanie wywołane `AddOrUpdateProduct` z `Id` = 999, **wtedy** rzucany jest wyjątek „nie znaleziono”, błąd jest zapisany w logu i w bazie nie przybywa rekordów.
6. **Zakładając, że** argument jest `null`, **gdy** zostanie wywołane `AddOrUpdateProduct`, **wtedy** rzucany jest `ArgumentNullException`, a błąd jest zapisany przez `ILogger`.
7. **Zakładając, że** filtr jest `null`, **gdy** zostanie wywołane `GetProduct`, **wtedy** rzucany jest `ArgumentNullException` i błąd jest zapisany w logu.
8. **Zakładając, że** w bazie są produkty „Monitor Dell 32” i „Mysz Logitech”, **gdy** zostanie wywołane `GetProduct(p => p.Name == "Monitor Dell 32")`, **wtedy** zwrócony jest `ProductVm` o tej nazwie.
9. **Zakładając, że** w bazie są dwa produkty o `Id` 1 i 2, **gdy** zostanie wywołane `GetProducts(p => p.Id >= 1 && p.Id <= 2)`, **wtedy** zwrócone są dokładnie 2 elementy; wywołanie `GetProducts()` bez filtra zwraca wszystkie produkty (liczba równa liczbie produktów w bazie).
10. **Zakładając, że** w bazie nie ma produktu spełniającego filtr, **gdy** zostanie wywołane `GetProduct`, **wtedy** zwracany jest `null`.
11. **Zakładając, że** w bazie są produkty „Monitor Dell 32” (1000) i „Mysz Logitech” (500), **gdy** zostanie wywołane `GetProducts(p => p.Price > 600)` oraz `GetProducts(p => p.Price <= 500)`, **wtedy** zwracane są odpowiednio tylko monitor i tylko mysz, na każdym providerze bazy (SQLite i SQL Server).

**Uwagi:** Zależy od US-01-08.

---

### US-01-10: Projekt testów jednostkowych (xUnit v3, baza testowa, dane startowe)

**Status:** Zweryfikowana
**Źródło:** Rozdz. 5.2, 5.3 pkt 1, 3
**Priorytet:** Wysoki

**Jako** programista
**chcę** projekt `WebStore.Tests` opartych na xUnit v3 z konfiguracją wstrzykiwania zależności, bazą testową i danymi startowymi
**aby** uruchamiać testy jednostkowe usług bez zewnętrznego serwera bazy.

**Kryteria akceptacji:**
1. **Zakładając, że** projekt `WebStore.Tests` istnieje, **gdy** sprawdzone zostaną jego pakiety, **wtedy** używa xunit.v3 (nie przestarzałego pakietu `xunit` 2.x), biblioteki wstrzykiwania zależności zgodnej z xUnit v3 oraz pakietów potrzebnych do bazy testowej i Identity, w aktualnych stabilnych wersjach.
2. **Zakładając, że** projekt testów istnieje, **gdy** uruchomiona zostanie konfiguracja testów, **wtedy** rejestruje AutoMapper z `MainProfile`, `ApplicationDbContext` na bazie SQLite w pamięci (z utrzymanym otwartym połączeniem na czas testu), Identity (`User`, `IdentityRole<int>`; min. długość hasła 6, bez wymogu znaków specjalnych i unikalnych znaków, bez wymaganego potwierdzenia konta), `ILogger` oraz wszystkie usługi z tego sprintu.
3. **Zakładając, że** uruchomiono konfigurację testów, **gdy** testy zaczynają działanie, **wtedy** baza zawiera: dostawcę (Id 1, „Adam Bednarski”, supp1@eg.eg), kategorię (Id 1, „Computers”, „#computer”), produkty „Monitor Dell 32” (Id 1, cena 1000, waga 20) i „Mysz Logitech” (Id 2, cena 500, waga 0,5) — oraz dane startowe niezbędne testom usług z US-01-11–US-01-14.
4. **Zakładając, że** testy są zaimplementowane, **gdy** zostanie wykonane `dotnet test`, **wtedy** wszystkie testy kończą się wynikiem „Passed”, a polecenie zwraca kod wyjścia 0.
5. **Zakładając, że** testy są zaimplementowane, **gdy** zostanie wykonane `dotnet test` z filtrem na nazwę testu lub klasy, **wtedy** uruchamiane są tylko wskazane testy.
6. **Zakładając, że** każdy test ma własną, odrębną bazę SQLite w pamięci wypełnioną danymi startowymi, **gdy** testy zostaną uruchomione w dowolnej kolejności lub równolegle, **wtedy** wynik każdego testu nie zależy od zmian wprowadzonych przez inne testy.
7. **Zakładając, że** baza testowa to SQLite w pamięci, **gdy** test zapisuje encję z nieistniejącym kluczem obcym, **wtedy** zapis kończy się wyjątkiem (baza testowa egzekwuje ograniczenia kluczy obcych).
8. **Zakładając, że** test pobiera wszystkie produkty, **gdy** zostanie wykonana asercja liczby wyników, **wtedy** porównuje ją z liczbą produktów faktycznie obecnych w bazie (a nie ze sobą samą).
9. **Zakładając, że** dostępna jest instancja SQL Server i skonfigurowano wariant (b) z US-01-07, **gdy** testy dotyczące cen i `TotalAmount` (US-01-04 pkt 5, US-01-07 pkt 4, US-01-09 pkt 11) zostaną uruchomione na tym providerze, **wtedy** przechodzą tak samo jak na SQLite.

**Uwagi:** Przykładowy kod testów z instrukcji (atrybuty testów, wstrzykiwanie przez konstruktor, `Startup`) napisano pod xUnit 2.x i wymaga dostosowania do xUnit v3. Testy integracyjne i GUI oraz mierzalny próg pokrycia są poza zakresem sprintu.

---

### US-01-11: Usługa zamówień (OrderService)

**Status:** Zweryfikowana
**Źródło:** Rozdz. 5.4
**Priorytet:** Wysoki

**Jako** programista
**chcę** usługę `OrderService` z interfejsem `IOrderService`, view modelami i testami jednostkowymi
**aby** zarządzać zamówieniami klientów.

**Kryteria akceptacji:**
1. **Zakładając, że** usługa istnieje, **gdy** zostanie sprawdzona, **wtedy** składa się z interfejsu `IOrderService` i klasy `OrderService` dziedziczącej po `BaseService`, a jej view modele mają sufiks `Vm` i leżą w folderze `VM`.
2. **Zakładając, że** w bazie istnieje klient i produkty, **gdy** zostanie zapisane zamówienie z poprawnymi pozycjami, **wtedy** zamówienie jest utrwalone wraz z pozycjami, w każdej pozycji zapisana jest aktualna cena produktu jako cena zamrożona, a zwrócony view model zawiera dane zamówienia.
3. **Zakładając, że** istnieje zamówienie, **gdy** zostanie pobrane po filtrze lub w postaci listy, **wtedy** zwracane są view modele zgodne z filtrem; brak filtra zwraca wszystkie; brak dopasowania zwraca `null` (dla pobrania jednego).
4. **Zakładając, że** argument lub filtr jest `null`, **gdy** zostanie wywołana odpowiednia metoda, **wtedy** rzucany jest `ArgumentNullException` i błąd trafia do loga.
5. **Zakładając, że** zamówienie wskazuje nieistniejącego klienta lub produkt, **gdy** zostanie zapisane, **wtedy** operacja kończy się błędem i nic nie jest zapisane.
6. **Zakładając, że** zapis zamówienia ma `Id` wskazujące nieistniejące zamówienie, **gdy** zostanie wywołany, **wtedy** rzucany jest wyjątek „nie znaleziono”.
7. **Zakładając, że** zamówienie ma pozycje z cenami ułamkowymi, **gdy** zostanie zapisane i pobrane, **wtedy** `TotalAmount` w view modelu jest dokładną sumą (cena zamrożona × ilość) pozycji (US-01-04 pkt 4 i 5), na każdym providerze bazy.
8. **Zakładając, że** usługa jest zaimplementowana, **gdy** uruchomione zostaną testy jednostkowe `OrderService`, **wtedy** obejmują scenariusze z pkt 2–7 i wszystkie przechodzą.

**Uwagi:** Zakres: zapis lub aktualizacja, pobranie jednego, lista z filtrem oraz wyliczenie `TotalAmount`. `OrderService` może korzystać z `ProductService`.

---

### US-01-12: Usługa faktur (InvoiceService)

**Status:** Zweryfikowana
**Źródło:** Rozdz. 5.4
**Priorytet:** Średni

**Jako** programista
**chcę** usługę `InvoiceService` z interfejsem `IInvoiceService`, view modelami i testami jednostkowymi
**aby** zarządzać fakturami zamówień.

**Kryteria akceptacji:**
1. **Zakładając, że** usługa istnieje, **gdy** zostanie sprawdzona, **wtedy** składa się z `IInvoiceService` i `InvoiceService` dziedziczącej po `BaseService`, a view modele mają sufiks `Vm`.
2. **Zakładając, że** istnieje zamówienie bez faktury, **gdy** zostanie zapisana faktura dla tego zamówienia, **wtedy** faktura jest utrwalona i powiązana z zamówieniem.
3. **Zakładając, że** zamówienie ma już fakturę, **gdy** zostanie zapisana kolejna faktura dla tego zamówienia, **wtedy** operacja kończy się błędem i nic nie jest zapisane (relacja 1:1).
4. **Zakładając, że** istnieje faktura, **gdy** zostanie pobrana po filtrze lub jako lista, **wtedy** zwracane są view modele zgodne z filtrem; brak dopasowania zwraca `null` (dla pobrania jednego).
5. **Zakładając, że** argument lub filtr jest `null`, **gdy** zostanie wywołana odpowiednia metoda, **wtedy** rzucany jest `ArgumentNullException` i błąd trafia do loga.
6. **Zakładając, że** faktura wskazuje nieistniejące zamówienie, **gdy** zostanie zapisana, **wtedy** operacja kończy się błędem.
7. **Zakładając, że** zapis faktury ma `Id` wskazujące nieistniejącą fakturę, **gdy** zostanie wywołany, **wtedy** rzucany jest wyjątek „nie znaleziono”.
8. **Zakładając, że** usługa jest zaimplementowana, **gdy** uruchomione zostaną testy jednostkowe `InvoiceService`, **wtedy** obejmują scenariusze z pkt 2–7 i przechodzą.

**Uwagi:** Zakres: zapis lub aktualizacja, pobranie jednego, lista z filtrem.

---

### US-01-13: Usługa sklepów stacjonarnych (StoreService)

**Status:** Zweryfikowana
**Źródło:** Rozdz. 5.4
**Priorytet:** Średni

**Jako** programista
**chcę** usługę `StoreService` z interfejsem `IStoreService`, view modelami i testami jednostkowymi
**aby** zarządzać sklepami stacjonarnymi.

**Kryteria akceptacji:**
1. **Zakładając, że** usługa istnieje, **gdy** zostanie sprawdzona, **wtedy** składa się z `IStoreService` i `StoreService` dziedziczącej po `BaseService`, a view modele mają sufiks `Vm`.
2. **Zakładając, że** istnieje adres, **gdy** zostanie zapisany sklep z tym adresem, **wtedy** sklep jest utrwalony wraz z powiązaniem adresu.
3. **Zakładając, że** istnieje sklep, **gdy** zostanie pobrany po filtrze lub jako lista, **wtedy** zwracane są view modele zgodne z filtrem; brak dopasowania zwraca `null` (dla pobrania jednego).
4. **Zakładając, że** argument lub filtr jest `null`, **gdy** zostanie wywołana odpowiednia metoda, **wtedy** rzucany jest `ArgumentNullException` i błąd trafia do loga.
5. **Zakładając, że** zapis sklepu ma `Id` wskazujące nieistniejący sklep, **gdy** zostanie wywołany, **wtedy** rzucany jest wyjątek „nie znaleziono”.
6. **Zakładając, że** usługa jest zaimplementowana, **gdy** uruchomione zostaną testy jednostkowe `StoreService`, **wtedy** obejmują scenariusze z pkt 2–5 i przechodzą.

**Uwagi:** Zakres: zapis lub aktualizacja, pobranie jednego, lista z filtrem. `StoreService` może korzystać z `AddressService`.

---

### US-01-14: Usługa adresów (AddressService)

**Status:** Zweryfikowana
**Źródło:** Rozdz. 5.4
**Priorytet:** Średni

**Jako** programista
**chcę** usługę `AddressService` z interfejsem `IAddressService`, view modelami i testami jednostkowymi
**aby** zarządzać adresami klientów i sklepów.

**Kryteria akceptacji:**
1. **Zakładając, że** usługa istnieje, **gdy** zostanie sprawdzona, **wtedy** składa się z `IAddressService` i `AddressService` dziedziczącej po `BaseService`, a view modele mają sufiks `Vm`.
2. **Zakładając, że** przekazano poprawne dane adresu, **gdy** zostanie zapisany adres, **wtedy** jest utrwalony, a zwrócony view model zawiera jego dane.
3. **Zakładając, że** istnieje adres, **gdy** zostanie zapisany z istniejącym identyfikatorem i zmienionymi danymi, **wtedy** dane adresu są zaktualizowane (nie powstaje nowy rekord).
4. **Zakładając, że** zapis adresu ma `Id` wskazujące nieistniejący adres, **gdy** zostanie wywołany, **wtedy** rzucany jest wyjątek „nie znaleziono”.
5. **Zakładając, że** istnieje adres, **gdy** zostanie pobrany po filtrze lub jako lista, **wtedy** zwracane są view modele zgodne z filtrem; brak dopasowania zwraca `null` (dla pobrania jednego).
6. **Zakładając, że** argument lub filtr jest `null`, **gdy** zostanie wywołana odpowiednia metoda, **wtedy** rzucany jest `ArgumentNullException` i błąd trafia do loga.
7. **Zakładając, że** usługa jest zaimplementowana, **gdy** uruchomione zostaną testy jednostkowe `AddressService`, **wtedy** obejmują scenariusze z pkt 2–6 i przechodzą.

**Uwagi:** Zakres: zapis lub aktualizacja, pobranie jednego, lista z filtrem.

---

## Macierz pokrycia

| Wymaganie z dokumentu | Identyfikatory US |
|---|---|
| 1.1 .NET SDK | US-01-01 (platforma, wersje i `global.json` wg decyzji Lidera) |
| 1.2 Baza SQL Server LocalDB/Express (Docker azure-sql-edge) | US-01-07 (SQLite domyślnie, SQL Server jako wariant alternatywny) |
| 1.3 Visual Studio Code i rozszerzenia | Poza zakresem (czynność Lidera) |
| 1.4 Git: klonowanie, gałąź na studenta, token, commit i push | Poza zakresem (czynność Lidera; commity wyłącznie Lider) |
| 2. Solucja, 5 projektów, referencje, build, uruchomienie | US-01-01 |
| 3.1–3.2 Solucja i model wg diagramu UML | US-01-01, US-01-02, US-01-03, US-01-04 |
| 3.3 Address, Customer z obiektem adresu | US-01-02 |
| 3.4 StationaryStore, StationaryStoreEmployee | US-01-02, US-01-05 |
| 3.5 Category, zmiana Product | US-01-03 |
| 3.6 Invoice, zmiana Order | US-01-04 |
| 4.1 Relacje 1:M (8 par encji) | US-01-06 |
| 4.2 Relacja N:M Order–Product (OrderProduct) | US-01-06 |
| 4 TPH (User, Customer, Supplier, StationaryStoreEmployee) | US-01-06 |
| 4 Migracja i jej zastosowanie | US-01-07 |
| 5.1 pkt 1–7 Foldery, view modele, konwencje, BaseService, pakiety | US-01-08 |
| 5.1 pkt 8 ProductService | US-01-09 |
| 5.2 Konfiguracja projektu testów, Startup, dane startowe | US-01-10 |
| 5.2 Poziomy testów (integracyjne, GUI) | Poza zakresem sprintu |
| 5.3 Testy jednostkowe ProductService i uruchamianie | US-01-09, US-01-10 |
| 5.4 OrderService, InvoiceService, StoreService, AddressService + testy | US-01-11, US-01-12, US-01-13, US-01-14 |
| 5.4 Commit & Push | Poza zakresem (czynność Lidera) |
| Niefunkcjonalne: konwencje nazewnicze (I…, …Service, …Vm), logowanie błędów, walidacja `[Required]` | US-01-08, US-01-09 |
| Niefunkcjonalne: „dążenie do pokrycia testami jak największej ilości kodu” | US-01-10 (testy usług), bez progu pokrycia |
| Decyzja Lidera: poprawność operacji na cenach i TotalAmount na obu providerach | US-01-04, US-01-07, US-01-09, US-01-10, US-01-11 |
| Decyzja Lidera: rejestracja usług w DI, brak kontrolerów i Swaggera | US-01-01 |
| Skrypty konfiguracji środowiska (`setup.sh`, `setup.ps1`), sekcja „Środowisko” w `CLAUDE.md` | Poza zakresem stories (osobne zlecenie Lidera) |
