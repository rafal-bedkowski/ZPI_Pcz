using WebStore.DAL.EF;
using WebStore.Model.DataModels;

namespace WebStore.Tests.Infrastructure;

// Dane startowe wspólne dla wszystkich testów usług.
public static class TestData
{
    public const int SupplierId = 1;
    public const int CustomerId = 2;
    public const int CategoryId = 1;
    public const int MonitorId = 1;
    public const int MouseId = 2;
    public const int StoreId = 1;
    public const int CustomerBillingAddressId = 1;
    public const int CustomerShippingAddressId = 2;
    public const int StoreAddressId = 3;
    public const int FreeAddressId = 4;

    // Id nadaje baza (kolumny IDENTITY w SQL Server nie przyjmują jawnych wartości). Na świeżej bazie
    // numeracja startuje od 1, a rekordy zapisywane są etapami w kolejności zgodnej ze stałymi Id.
    public static void Seed(ApplicationDbContext db)
    {
        db.Addresses.AddRange(
            NewAddress("Rozliczeniowa", "1"),
            NewAddress("Wysyłkowa", "2"),
            NewAddress("Sklepowa", "3"),
            NewAddress("Wolna", "4"));
        db.SaveChanges();

        db.Suppliers.Add(new Supplier
        {
            FirstName = "Adam",
            LastName = "Bednarski",
            UserName = "supp1@eg.eg",
            NormalizedUserName = "SUPP1@EG.EG",
            Email = "supp1@eg.eg",
            NormalizedEmail = "SUPP1@EG.EG",
            RegistrationDate = new DateTime(2024, 1, 1)
        });
        db.SaveChanges();

        db.Customers.Add(new Customer
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            UserName = "cust1@eg.eg",
            NormalizedUserName = "CUST1@EG.EG",
            Email = "cust1@eg.eg",
            NormalizedEmail = "CUST1@EG.EG",
            RegistrationDate = new DateTime(2024, 1, 2),
            BillingAddressId = CustomerBillingAddressId,
            ShippingAddressId = CustomerShippingAddressId
        });
        db.Categories.Add(new Category { Name = "Computers", Tag = "#computer" });
        db.SaveChanges();

        db.Products.AddRange(
            new Product
            {
                Name = "Monitor Dell 32", Description = "Monitor 32 cale",
                Price = 1000m, Weight = 20f, CategoryId = CategoryId, SupplierId = SupplierId
            },
            new Product
            {
                Name = "Mysz Logitech", Description = "Mysz bezprzewodowa",
                Price = 500m, Weight = 0.5f, CategoryId = CategoryId, SupplierId = SupplierId
            });
        db.SaveChanges();

        db.StationaryStores.Add(new StationaryStore { Name = "Sklep Centralny", AddressId = StoreAddressId });
        db.SaveChanges();

        db.ProductStocks.AddRange(
            new ProductStock { ProductId = MonitorId, StationaryStoreId = StoreId, Quantity = 10 },
            new ProductStock { ProductId = MouseId, StationaryStoreId = StoreId, Quantity = 5 });

        db.SaveChanges();
    }

    private static Address NewAddress(string street, string number) => new()
    {
        Street = street, BuildingNumber = number, City = "Częstochowa",
        PostalCode = "42-200", Country = "Polska"
    };
}
