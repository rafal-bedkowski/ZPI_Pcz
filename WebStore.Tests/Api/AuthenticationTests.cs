using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

public class AuthenticationTests : ApiTestBase
{
    private async Task<HttpStatusCode> GetProductsWithAsync(string? token)
    {
        using var client = Factory.CreateApiClient();
        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return (await client.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken)).StatusCode;
    }

    [Fact]
    public async Task ValidToken_IsAccepted()
    {
        Assert.Equal(HttpStatusCode.OK, await GetProductsWithAsync(Factory.CreateToken()));
    }

    [Fact]
    public async Task MissingToken_IsRejected()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, await GetProductsWithAsync(null));
    }

    [Fact]
    public async Task TokenWithWrongSignature_IsRejected()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, await GetProductsWithAsync(Factory.CreateToken(key: ApiFactory.NewSecret())));
    }

    [Fact]
    public async Task TokenWithWrongIssuerOrAudience_IsRejected()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, await GetProductsWithAsync(Factory.CreateToken(issuer: "inny-wystawca")));
        Assert.Equal(HttpStatusCode.Unauthorized, await GetProductsWithAsync(Factory.CreateToken(audience: "inna-publicznosc")));
    }

    [Fact]
    public async Task ExpiredToken_IsRejectedAfterClockSkew()
    {
        var expired = Factory.CreateToken(
            notBefore: DateTime.UtcNow.AddMinutes(-40), expires: DateTime.UtcNow.AddMinutes(-20));

        Assert.Equal(HttpStatusCode.Unauthorized, await GetProductsWithAsync(expired));
    }

    [Fact]
    public async Task TokenExpiredWithinClockSkew_IsStillAccepted()
    {
        var justExpired = Factory.CreateToken(
            notBefore: DateTime.UtcNow.AddMinutes(-30), expires: DateTime.UtcNow.AddMinutes(-2));

        Assert.Equal(HttpStatusCode.OK, await GetProductsWithAsync(justExpired));
    }

    [Theory]
    [InlineData("to-nie-jest-token")]
    [InlineData("aaa.bbb.ccc")]
    public async Task MalformedToken_IsRejected(string token)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, await GetProductsWithAsync(token));
    }

    [Fact]
    public async Task RolesFromToken_AreAvailableAsUserRoles()
    {
        var parameters = Factory.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(Factory.CreateToken(role: "Customer"), parameters);

        Assert.True(result.IsValid);
        var principal = new ClaimsPrincipal(result.ClaimsIdentity);
        Assert.True(principal.IsInRole("Customer"));
        Assert.False(principal.IsInRole("Admin"));
    }

    [Fact]
    public void JwtOptions_AreReadFromConfiguration()
    {
        var options = Factory.Services.GetRequiredService<IOptions<JwtOptionsVm>>().Value;

        Assert.Equal(Factory.Issuer, options.Issuer);
        Assert.Equal(Factory.Audience, options.Audience);
        Assert.Equal(Factory.SecretKey, options.SecretKey);
        Assert.Equal(20, options.TokenExpirationMinutes);
    }

    [Fact]
    public void IdentityPasswordPolicy_RequiresOnlyMinimumLength()
    {
        var policy = Factory.Services.GetRequiredService<IOptions<Microsoft.AspNetCore.Identity.IdentityOptions>>().Value;

        Assert.Equal(6, policy.Password.RequiredLength);
        Assert.False(policy.Password.RequireDigit);
        Assert.False(policy.Password.RequireUppercase);
        Assert.False(policy.Password.RequireLowercase);
        Assert.False(policy.Password.RequireNonAlphanumeric);
        Assert.False(policy.User.RequireUniqueEmail);
    }
}
