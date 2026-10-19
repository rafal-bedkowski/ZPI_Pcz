using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebStore.Model.DataModels;

namespace WebStore.DAL.EF;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<StationaryStoreEmployee> StationaryStoreEmployees => Set<StationaryStoreEmployee>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
    public DbSet<StationaryStore> StationaryStores => Set<StationaryStore>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderProduct> OrderProducts => Set<OrderProduct>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Jedna tabela Users z kolumną Discriminator dla User, Customer, Supplier i StationaryStoreEmployee (TPH).
        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<User>(nameof(User))
            .HasValue<Customer>(nameof(Customer))
            .HasValue<Supplier>(nameof(Supplier))
            .HasValue<StationaryStoreEmployee>(nameof(StationaryStoreEmployee));

        // Każdy adres klienta jest własnym rekordem - unikalny indeks zabrania współdzielenia.
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasOne(c => c.BillingAddress).WithMany().HasForeignKey(c => c.BillingAddressId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.ShippingAddress).WithMany().HasForeignKey(c => c.ShippingAddressId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(c => c.BillingAddressId).IsUnique();
            e.HasIndex(c => c.ShippingAddressId).IsUnique();
            e.HasMany(c => c.Orders).WithOne(o => o.Customer).HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.Products).WithOne(p => p.Supplier).HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Products).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(p => p.ProductStocks).WithOne(ps => ps.Product).HasForeignKey(ps => ps.ProductId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StationaryStore>(e =>
        {
            e.HasOne(s => s.Address).WithOne().HasForeignKey<StationaryStore>(s => s.AddressId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(s => s.StationaryStoreEmployee).WithOne(emp => emp.StationaryStore).HasForeignKey(emp => emp.StationaryStoreId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(s => s.ProductStock).WithOne(ps => ps.StationaryStore).HasForeignKey(ps => ps.StationaryStoreId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Order).WithOne(o => o.Invoice).HasForeignKey<Invoice>(i => i.OrderId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderProduct>(e =>
        {
            e.HasKey(op => new { op.OrderId, op.ProductId });
            e.HasOne(op => op.Order).WithMany(o => o.OrderProducts).HasForeignKey(op => op.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(op => op.Product).WithMany().HasForeignKey(op => op.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>().Ignore(o => o.TotalAmount);

        ConfigureDecimals(modelBuilder);
    }

    // SQLite nie ma typu decimal (EF zapisuje go jako tekst, co psuje sortowanie, porównania i sumy),
    // dlatego tam kwoty trafiają do bazy jako grosze w kolumnie całkowitej. SQL Server używa decimal(18,2).
    private void ConfigureDecimals(ModelBuilder modelBuilder)
    {
        var useCents = Database.IsSqlite();
        var converter = new ValueConverter<decimal, long>(
            v => (long)Math.Round(v * 100m, 0, MidpointRounding.AwayFromZero),
            v => v / 100m);

        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal)))
        {
            if (useCents)
            {
                property.SetValueConverter(converter);
            }
            else
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }
}
