using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Web.Services;

public class LoginService : ILoginService
{
    // Nazwy claimów zgodne z TokenValidationParameters ustawionymi w konfiguracji uwierzytelniania.
    public const string NameClaim = JwtRegisteredClaimNames.UniqueName;
    public const string RoleClaim = "role";

    private readonly UserManager<User> _userManager;
    private readonly JwtOptionsVm _jwtOptions;

    public LoginService(UserManager<User> userManager, IOptions<JwtOptionsVm> jwtOptions)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<LoginResultVm?> LoginAsync(LoginUserVm model)
    {
        ArgumentNullException.ThrowIfNull(model);

        // Sprawdzenie hasła przez UserManager nie zlicza nieudanych prób, więc konto nie jest blokowane.
        var user = await _userManager.FindByNameAsync(model.Login);
        if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return null;
        }

        var claims = new List<Claim> { new(NameClaim, user.UserName!) };
        foreach (var role in await _userManager.GetRolesAsync(user))
        {
            claims.Add(new Claim(RoleClaim, role));
        }

        var issuedAt = DateTime.UtcNow;
        var expires = issuedAt.AddMinutes(_jwtOptions.TokenExpirationMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            NotBefore = issuedAt,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                SecurityAlgorithms.HmacSha256)
        };

        return new LoginResultVm
        {
            AccessToken = new JsonWebTokenHandler().CreateToken(descriptor),
            ExpiresIn = expires.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
        };
    }
}
