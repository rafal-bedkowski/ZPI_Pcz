using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;

namespace WebStore.Tests;

public class MigrationTests
{
    private static readonly string[] ExpectedTables =
    {
        "AspNetUsers", "AspNetRoles", "Addresses", "Categories", "Products", "ProductStocks",
        "StationaryStores", "Orders", "OrderProducts", "Invoices"
    };

    [Fact]
    public void US_01_07_SqliteMigration_CreatesDatabaseFileWithAllTablesAndForeignKeys()
    {
        var path = Path.Combine(Path.GetTempPath(), $"webstore-migration-{Guid.NewGuid():N}.db");
        try
        {
            using (var db = new SqliteDbContext(new DbContextOptionsBuilder<SqliteDbContext>().UseSqlite($"Data Source={path}").Options))
            {
                db.Database.Migrate();
            }

            Assert.True(File.Exists(path));
            using var connection = new SqliteConnection($"Data Source={path};Pooling=False");
            connection.Open();

            var tables = Query(connection, "select name from sqlite_master where type = 'table'");
            Assert.All(ExpectedTables, t => Assert.Contains(t, tables));

            var orderProductKeys = Query(connection, "select \"table\" from pragma_foreign_key_list('OrderProducts')");
            Assert.Contains("Orders", orderProductKeys);
            Assert.Contains("Products", orderProductKeys);
            Assert.Contains("Categories", Query(connection, "select \"table\" from pragma_foreign_key_list('Products')"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(path);
        }
    }

    [Fact]
    public void US_01_07_SqliteMigrations_AreUpToDateWithModel()
    {
        using var db = new SqliteDbContext(new DbContextOptionsBuilder<SqliteDbContext>().UseSqlite("Data Source=:memory:").Options);

        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public void US_01_07_SqlServerMigrations_ExistAndAreUpToDateWithModel()
    {
        using var db = new SqlServerDbContext(new DbContextOptionsBuilder<SqlServerDbContext>()
            .UseSqlServer("Server=localhost;Database=Ignored").Options);

        Assert.NotEmpty(db.Database.GetMigrations());
        Assert.False(db.Database.HasPendingModelChanges());
    }

    private static List<string> Query(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        var result = new List<string>();
        while (reader.Read())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }
}
