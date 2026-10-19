using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Tests.Infrastructure;

namespace WebStore.Tests;

public class DbContextTests
{
    private readonly ApplicationDbContext _db;

    public DbContextTests(ApplicationDbContext db) => _db = db;

    private IEntityType Entity<T>() => _db.Model.FindEntityType(typeof(T))!;

    private static IForeignKey ForeignKey(IEntityType dependent, Type principal, string property) =>
        dependent.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == principal
                                                && fk.Properties.Single().Name == property);

    [Fact]
    public void US_01_06_Context_ExposesProductsAndIdentityWithIntKeys()
    {
        Assert.IsAssignableFrom<IdentityDbContext<User, IdentityRole<int>, int>>(_db);
        Assert.NotNull(_db.Products);
        Assert.Equal(typeof(int), Entity<IdentityRole<int>>().FindPrimaryKey()!.Properties.Single().ClrType);
    }

    [Theory]
    [InlineData(typeof(Order), typeof(Customer), "CustomerId")]
    [InlineData(typeof(Product), typeof(Supplier), "SupplierId")]
    [InlineData(typeof(ProductStock), typeof(Product), "ProductId")]
    [InlineData(typeof(Product), typeof(Category), "CategoryId")]
    [InlineData(typeof(StationaryStoreEmployee), typeof(StationaryStore), "StationaryStoreId")]
    [InlineData(typeof(ProductStock), typeof(StationaryStore), "StationaryStoreId")]
    public void US_01_06_OneToManyRelations_Exist(Type dependent, Type principal, string property)
    {
        var fk = ForeignKey(_db.Model.FindEntityType(dependent)!, principal, property);

        Assert.False(fk.IsUnique);
    }

    [Fact]
    public void US_01_06_CustomerHasTwoSeparateAddressRelations_NotSharedBetweenCustomers()
    {
        var customer = Entity<Customer>();
        var billing = ForeignKey(customer, typeof(Address), "BillingAddressId");
        var shipping = ForeignKey(customer, typeof(Address), "ShippingAddressId");

        Assert.NotEqual(billing, shipping);
        Assert.Contains(customer.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == "BillingAddressId");
        Assert.Contains(customer.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == "ShippingAddressId");
    }

    [Fact]
    public void US_01_06_SharingAnAddressBetweenCustomers_IsRejected()
    {
        _db.Customers.Add(new Customer
        {
            Id = 10, UserName = "c10", BillingAddressId = TestData.FreeAddressId
        });
        _db.Customers.Add(new Customer
        {
            Id = 11, UserName = "c11", BillingAddressId = TestData.FreeAddressId
        });

        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    [Fact]
    public void US_01_06_StoreAddressAndInvoiceOrder_AreOneToOne()
    {
        Assert.True(ForeignKey(Entity<StationaryStore>(), typeof(Address), "AddressId").IsUnique);
        Assert.True(ForeignKey(Entity<Invoice>(), typeof(Order), "OrderId").IsUnique);
    }

    [Fact]
    public void US_01_06_OrderProduct_HasCompositeKeyOfOrderAndProduct()
    {
        var key = Entity<OrderProduct>().FindPrimaryKey()!;

        Assert.Equal(new[] { "OrderId", "ProductId" }, key.Properties.Select(p => p.Name));
    }

    [Fact]
    public void US_01_06_AllUserTypes_ShareOneTableWithDiscriminator()
    {
        var users = Entity<User>();

        Assert.Equal("Discriminator", users.FindDiscriminatorProperty()!.Name);
        foreach (var type in new[] { typeof(Customer), typeof(Supplier), typeof(StationaryStoreEmployee) })
        {
            Assert.Equal(users.GetTableName(), _db.Model.FindEntityType(type)!.GetTableName());
        }

        Assert.Equal("Customer", _db.Customers.Select(c => EF.Property<string>(c, "Discriminator")).Single());
    }

    [Fact]
    public void US_01_06_ProductWithMissingCategoryOrSupplier_ViolatesForeignKey()
    {
        _db.Products.Add(new Product { Name = "A", Description = "A", CategoryId = 99, SupplierId = 1 });
        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
        _db.ChangeTracker.Clear();

        _db.Products.Add(new Product { Name = "B", Description = "B", CategoryId = 1, SupplierId = 99 });
        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    [Fact]
    public void US_01_04_OrderWithFractionalPrices_KeepsExactTotalAfterRoundTrip()
    {
        var order = new Order
        {
            CustomerId = TestData.CustomerId, OrderDate = DateTime.Today, DeliveryDate = DateTime.Today
        };
        order.OrderProducts.Add(new OrderProduct { ProductId = TestData.MonitorId, Quantity = 3, UnitPrice = 19.99m });
        order.OrderProducts.Add(new OrderProduct { ProductId = TestData.MouseId, Quantity = 7, UnitPrice = 0.01m });
        _db.Orders.Add(order);
        _db.SaveChanges();
        _db.ChangeTracker.Clear();

        var loaded = _db.Orders.Include(o => o.OrderProducts).Single(o => o.Id == order.Id);

        Assert.Equal(60.04m, loaded.TotalAmount);
        Assert.Equal(19.99m, loaded.OrderProducts.Single(p => p.ProductId == TestData.MonitorId).UnitPrice);
    }

    [Fact]
    public void US_01_07_PriceOperations_GiveCorrectResultsOnSqlite()
    {
        _db.Products.RemoveRange(_db.Products);
        _db.SaveChanges();
        foreach (var price in new[] { 19.99m, 0.01m, 1000.00m })
        {
            _db.Products.Add(new Product { Name = $"P{price}", Description = "d", Price = price, CategoryId = 1, SupplierId = 1 });
        }

        _db.SaveChanges();

        Assert.Equal(1020.00m, _db.Products.Sum(p => p.Price));
        Assert.Equal(new[] { 0.01m, 19.99m, 1000.00m }, _db.Products.OrderBy(p => p.Price).Select(p => p.Price).ToList());
        Assert.Equal(new[] { 1000.00m, 19.99m, 0.01m }, _db.Products.OrderByDescending(p => p.Price).Select(p => p.Price).ToList());
        Assert.Equal(new[] { 19.99m, 1000.00m }, _db.Products.Where(p => p.Price > 19.98m).OrderBy(p => p.Price).Select(p => p.Price).ToList());
    }
}
