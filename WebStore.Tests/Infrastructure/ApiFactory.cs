using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebStore.DAL.EF;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Infrastructure;

// Aplikacja uruchomiona w pamięci z własną bazą SQLite (z danymi startowymi z TestData) oraz własną konfiguracją JWT.
// Po ustawieniu SqlServerConnectionString zamiast SQLite używa osobnej bazy SQL Server o losowej nazwie, usuwanej przy zwolnieniu fabryki.
// Klucz i hasło kont testowych są losowe dla każdej instancji, więc nie ma ich w repozytorium.
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminLogin = "admin@webstore.local";
    public const string CustomerLogin = "customer@webstore.local";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private string? _sqlServerDatabaseConnection;

    public string EnvironmentName { get; init; } = "Development";
    public string Issuer { get; init; } = "webstore-test-issuer";
    public string Audience { get; init; } = "webstore-test-audience";
    public string? SecretKey { get; init; } = NewSecret();
    public string? TestAccountPassword { get; init; } = NewSecret();
    // Katalog z wwwroot/index.html udającym zbudowanego klienta (zamiast katalogu projektu Web).
    public string? ContentRoot { get; init; }
    // Ciąg połączenia z instancją SQL Server (bez nazwy bazy); nazwę bazy nadaje fabryka.
    public string? SqlServerConnectionString { get; init; }
    // Wartość Database:Provider widziana przez aplikację (testy używają własnej bazy, więc zwykle "Sqlite"); null = brak ustawienia.
    public string? DatabaseProvider { get; init; } = "Sqlite";
    public string? HttpsPort { get; init; }
    public Action<IServiceCollection>? ConfigureServices { get; init; }
    public CapturingLoggerProvider Logs { get; } = new();

    // Przypisanie krotką: pole Password w inicjalizatorze obiektu wyglądałoby jak hasło zapisane w pliku (test czystości repozytorium).
    public static LoginUserVm Credentials(string login, string pwd)
    {
        var model = new LoginUserVm();
        (model.Login, model.Password) = (login, pwd);
        return model;
    }

    public static string NewSecret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        Action<DbContextOptionsBuilder> useDatabase;
        if (SqlServerConnectionString is null)
        {
            useDatabase = o => o.UseSqlite(_connection);
        }
        else
        {
            _sqlServerDatabaseConnection = new SqlConnectionStringBuilder(SqlServerConnectionString)
            {
                InitialCatalog = $"WebStoreApiTests_{Guid.NewGuid():N}"
            }.ConnectionString;
            useDatabase = o => o.UseSqlServerForTests(_sqlServerDatabaseConnection);
        }

        using (var db = new ApplicationDbContext(CreateOptions(useDatabase)))
        {
            db.Database.EnsureCreated();
            TestData.Seed(db);
        }

        builder.UseEnvironment(EnvironmentName);
        // Provider jest odczytywany już przy rejestracji usług w Program.cs, zanim zadziała ConfigureAppConfiguration,
        // dlatego trafia do ustawień hosta; pusta wartość nadpisuje ewentualne user-secrets programisty.
        builder.UseSetting("Database:Provider", DatabaseProvider ?? string.Empty);
        if (HttpsPort is not null)
        {
            builder.UseSetting("https_port", HttpsPort);
        }

        // Puste wartości nadpisują ewentualne user-secrets programisty, więc test nie zależy od jego środowiska.
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["JwtOptions:Issuer"] = Issuer,
                ["JwtOptions:Audience"] = Audience,
                ["JwtOptions:TokenExpirationMinutes"] = "20",
                ["JwtOptions:SecretKey"] = SecretKey ?? string.Empty,
                ["Seed:TestAccountPassword"] = TestAccountPassword ?? string.Empty
            }));

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddProvider(Logs);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<SqliteDbContext>>();
            services.RemoveAll<DbContextOptions<SqlServerDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<SqliteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<SqlServerDbContext>>();
            services.RemoveAll<SqliteDbContext>();
            services.RemoveAll<SqlServerDbContext>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(useDatabase);

            if (ContentRoot is not null)
            {
                var original = (IWebHostEnvironment)services.Last(d => d.ServiceType == typeof(IWebHostEnvironment)).ImplementationInstance!;
                services.RemoveAll<IWebHostEnvironment>();
                services.AddSingleton<IWebHostEnvironment>(new ClientRootEnvironment(original, ContentRoot));
            }

            ConfigureServices?.Invoke(services);
        });
    }

    private static DbContextOptions<ApplicationDbContext> CreateOptions(Action<DbContextOptionsBuilder> useDatabase)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        useDatabase(builder);
        return builder.Options;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            if (_sqlServerDatabaseConnection is not null)
            {
                using var db = new ApplicationDbContext(CreateOptions(o => o.UseSqlServerForTests(_sqlServerDatabaseConnection)));
                db.Database.EnsureDeleted();
            }

            _connection.Dispose();
        }
    }

    // Klient HTTPS bez automatycznych przekierowań.
    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    public HttpClient CreateAuthorizedClient(string login = AdminLogin, string role = "Admin")
    {
        var client = CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(login, role));
        return client;
    }

    // Token podpisany kluczem tej instancji; parametry pozwalają zbudować tokeny niepoprawne.
    public string CreateToken(string login = AdminLogin, string role = "Admin", string? key = null, string? issuer = null,
        string? audience = null, DateTime? notBefore = null, DateTime? expires = null)
    {
        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("unique_name", login), new Claim("role", role) }),
            Issuer = issuer ?? Issuer,
            Audience = audience ?? Audience,
            NotBefore = notBefore ?? now.AddMinutes(-1),
            Expires = expires ?? now.AddMinutes(20),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? SecretKey ?? NewSecret())),
                SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public async Task<string> LoginAsync(string login, string password)
    {
        using var client = CreateApiClient();
        var response = await client.PostAsJsonAsync("/api/AccountApi/Login", ApiFactory.Credentials(login, password), TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResultVm>(TestContext.Current.CancellationToken))!.AccessToken;
    }

    public void WithDb(Action<ApplicationDbContext> action)
    {
        using var scope = Services.CreateScope();
        action(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    public T WithDb<T>(Func<ApplicationDbContext, T> func)
    {
        using var scope = Services.CreateScope();
        return func(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }
}

// Środowisko jak oryginalne, ale z innym katalogiem głównym i wwwroot: aplikacja sama sprawdza, czy klient jest zbudowany.
internal sealed class ClientRootEnvironment : IWebHostEnvironment
{
    private readonly IWebHostEnvironment _original;

    public ClientRootEnvironment(IWebHostEnvironment original, string contentRoot)
    {
        _original = original;
        ContentRootPath = contentRoot;
        ContentRootFileProvider = new PhysicalFileProvider(contentRoot);
        WebRootPath = Path.Combine(contentRoot, "wwwroot");
        WebRootFileProvider = Directory.Exists(WebRootPath) ? new PhysicalFileProvider(WebRootPath) : new NullFileProvider();
    }

    public string ApplicationName { get => _original.ApplicationName; set => _original.ApplicationName = value; }
    public string EnvironmentName { get => _original.EnvironmentName; set => _original.EnvironmentName = value; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }
    public string WebRootPath { get; set; }
    public IFileProvider WebRootFileProvider { get; set; }
}
