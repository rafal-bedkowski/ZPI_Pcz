using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;

namespace WebStore.Web.Services;

// Konta testowe służą do logowania w środowisku Development; hasło pochodzi wyłącznie z konfiguracji
// (user-secrets lub zmienna środowiskowa), nigdy z repozytorium.
public class TestAccountSeeder
{
    public const string PasswordKey = "Seed:TestAccountPassword";
    public const string AdminRole = "Admin";
    public const string CustomerRole = "Customer";
    public const string AdminLogin = "admin@webstore.local";
    public const string CustomerLogin = "customer@webstore.local";

    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TestAccountSeeder> _logger;

    public TestAccountSeeder(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager,
        ApplicationDbContext dbContext, IConfiguration configuration, ILogger<TestAccountSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var testAccountPassword = _configuration[PasswordKey];
        if (string.IsNullOrWhiteSpace(testAccountPassword))
        {
            _logger.LogWarning(
                "Nie utworzono kont testowych: brakuje wartości {Key} (ustaw ją w user-secrets lub zmiennej środowiskowej).",
                PasswordKey);
            return;
        }

        try
        {
            if ((await _dbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                _logger.LogWarning(
                    "Nie utworzono kont testowych: schemat bazy nie jest aktualny (uruchom 'dotnet ef database update').");
                return;
            }

            foreach (var role in new[] { AdminRole, CustomerRole })
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    Check(await _roleManager.CreateAsync(new IdentityRole<int>(role)), $"rola {role}");
                }
            }

            await EnsureUserAsync(NewUser<User>(AdminLogin, "Administrator"), testAccountPassword, AdminRole);
            await EnsureUserAsync(NewUser<Customer>(CustomerLogin, "Klient"), testAccountPassword, CustomerRole);
        }
        catch (DbException ex)
        {
            _logger.LogWarning("Nie utworzono kont testowych: baza danych jest niedostępna ({Reason}).", ex.Message);
        }
    }

    private static T NewUser<T>(string login, string lastName) where T : User, new() => new()
    {
        UserName = login,
        Email = login,
        EmailConfirmed = true,
        FirstName = "Test",
        LastName = lastName,
        RegistrationDate = DateTime.UtcNow
    };

    private async Task EnsureUserAsync(User user, string password, string role)
    {
        if (await _userManager.FindByNameAsync(user.UserName!) is not null)
        {
            return;
        }

        Check(await _userManager.CreateAsync(user, password), $"konto {user.UserName}");
        Check(await _userManager.AddToRoleAsync(user, role), $"rola {role} dla {user.UserName}");
    }

    private void Check(IdentityResult result, string what)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Nie udało się utworzyć: {what} ({string.Join("; ", result.Errors.Select(e => e.Description))}).");
        }
    }
}
