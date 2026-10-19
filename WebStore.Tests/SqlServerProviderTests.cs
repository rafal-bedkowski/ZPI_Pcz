using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Tests.Infrastructure;

namespace WebStore.Tests;

// Uruchamiane tylko, gdy zmienna WEBSTORE_TEST_SQLSERVER wskazuje działającą instancję SQL Server;
// w przeciwnym razie są pomijane. Każdy przebieg tworzy i usuwa własną bazę.
public sealed class SqlServerProviderTests : IDisposable
{
    private const string ConnectionVariable = "WEBSTORE_TEST_SQLSERVER";
    private SqlServerDbContext? _db;

    private SqlServerDbContext Open()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
        Assert.SkipUnless(!string.IsNullOrWhiteSpace(connectionString), $"Brak zmiennej {ConnectionVariable}.");

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = $"WebStoreTests_{Guid.NewGuid():N}"
        };
        _db = new SqlServerDbContext(new DbContextOptionsBuilder<SqlServerDbContext>().UseSqlServer(builder.ConnectionString).Options);
        _db.Database.EnsureCreated();
        TestData.Seed(_db);
        return _db;
    }

    [Fact]
    public void US_01_10_SqlServer_PriceOperationsMatchSqlite()
    {
        var db = Open();
        db.Products.RemoveRange(db.Products);
        db.SaveChanges();
        foreach (var price in new[] { 19.99m, 0.01m, 1000.00m })
        {
            db.Products.Add(new Product { Name = $"P{price}", Description = "d", Price = price, CategoryId = 1, SupplierId = 1 });
        }

        db.SaveChanges();

        Assert.Equal(1020.00m, db.Products.Sum(p => p.Price));
        Assert.Equal(new[] { 0.01m, 19.99m, 1000.00m }, db.Products.OrderBy(p => p.Price).Select(p => p.Price).ToList());
        Assert.Equal(new[] { 19.99m, 1000.00m }, db.Products.Where(p => p.Price > 19.98m).OrderBy(p => p.Price).Select(p => p.Price).ToList());
    }

    [Fact]
    public void US_01_10_SqlServer_TotalAmountIsExactAfterRoundTrip()
    {
        var db = Open();
        var order = new Order { CustomerId = TestData.CustomerId, OrderDate = DateTime.Today, DeliveryDate = DateTime.Today };
        order.OrderProducts.Add(new OrderProduct { ProductId = TestData.MonitorId, Quantity = 3, UnitPrice = 19.99m });
        order.OrderProducts.Add(new OrderProduct { ProductId = TestData.MouseId, Quantity = 7, UnitPrice = 0.01m });
        db.Orders.Add(order);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        Assert.Equal(60.04m, db.Orders.Include(o => o.OrderProducts).Single(o => o.Id == order.Id).TotalAmount);
    }

    [Fact]
    public void US_01_10_SqlServer_MissingForeignKeyIsRejected()
    {
        var db = Open();
        db.Products.Add(new Product { Name = "X", Description = "X", CategoryId = 99, SupplierId = 1 });

        Assert.Throws<DbUpdateException>(() => db.SaveChanges());
    }

    public void Dispose() => _db?.Database.EnsureDeleted();
}
