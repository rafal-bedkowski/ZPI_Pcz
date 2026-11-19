using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;
using WebStore.Web.Services;

namespace WebStore.Web.Configuration;

public static class AuthenticationServiceCollectionExtensions
{
    public const int MinimumKeyBytes = 32;

    /// <summary>
    /// Rejestruje Identity (<see cref="User"/>, rola <see cref="IdentityRole{TKey}"/>) oraz uwierzytelnianie tokenem JWT Bearer
    /// z parametrami z sekcji JwtOptions. Opcje są odczytywane przy pierwszym użyciu, a ich poprawność (w tym klucz
    /// podpisujący) jest sprawdzana przy starcie aplikacji.
    /// </summary>
    public static IServiceCollection AddWebStoreAuthentication(this IServiceCollection services)
    {
        services.AddOptions<JwtOptionsVm>()
            .BindConfiguration(JwtOptionsVm.SectionName)
            .PostConfigure<IHostEnvironment>((options, environment) =>
            {
                // W Development klucz jest losowy przy każdym starcie, więc tokeny nie przeżywają restartu i nie ma sekretu w repozytorium.
                if (string.IsNullOrWhiteSpace(options.SecretKey) && environment.IsDevelopment())
                {
                    options.SecretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                }
            })
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtOptionsVm>, JwtOptionsValidator>();

        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptionsVm>>((bearer, jwt) =>
            {
                var settings = jwt.Value;
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ClockSkew = TimeSpan.FromMinutes(5),
                    NameClaimType = LoginService.NameClaim,
                    RoleClaimType = LoginService.RoleClaim
                };
            });

        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<TestAccountSeeder>();
        return services;
    }

    private sealed class JwtOptionsValidator : IValidateOptions<JwtOptionsVm>
    {
        public ValidateOptionsResult Validate(string? name, JwtOptionsVm options)
        {
            var failures = new List<string>();
            var key = $"{JwtOptionsVm.SectionName}:{nameof(JwtOptionsVm.SecretKey)}";

            if (Encoding.UTF8.GetByteCount(options.SecretKey) < MinimumKeyBytes)
            {
                failures.Add(
                    $"Brak poprawnego klucza podpisującego tokeny JWT: ustawienie {key} musi mieć co najmniej " +
                    $"{MinimumKeyBytes} bajty. Ustaw je w user-secrets (dotnet user-secrets set \"{key}\" <klucz> --project WebStore.Web) " +
                    $"lub w zmiennej środowiskowej {key.Replace(":", "__")}.");
            }

            if (string.IsNullOrWhiteSpace(options.Issuer))
            {
                failures.Add($"Brakuje ustawienia {JwtOptionsVm.SectionName}:{nameof(JwtOptionsVm.Issuer)}.");
            }

            if (string.IsNullOrWhiteSpace(options.Audience))
            {
                failures.Add($"Brakuje ustawienia {JwtOptionsVm.SectionName}:{nameof(JwtOptionsVm.Audience)}.");
            }

            if (options.TokenExpirationMinutes <= 0)
            {
                failures.Add($"Ustawienie {JwtOptionsVm.SectionName}:{nameof(JwtOptionsVm.TokenExpirationMinutes)} musi być większe od zera.");
            }

            return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
        }
    }
}
