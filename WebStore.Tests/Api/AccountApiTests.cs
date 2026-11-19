using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;
using WebStore.Web.Services;

namespace WebStore.Tests.Api;

public class AccountApiTests : ApiTestBase
{
    private const string LoginUrl = "/api/AccountApi/Login";

    private string Password => Factory.TestAccountPassword!;

    private static FormUrlEncodedContent UrlEncodedForm(string login, string password) =>
        new(new Dictionary<string, string> { ["Login"] = login, ["Password"] = password });

    private static MultipartFormDataContent MultipartForm(string login, string password) =>
        new() { { new StringContent(login), "Login" }, { new StringContent(password), "Password" } };

    private async Task AssertSuccessfulLoginAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadAsync<LoginResultVm>(response);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$", result.ExpiresIn);
    }

    [Fact]
    public async Task Login_AsJson_ReturnsTokenAndExpiry()
    {
        var response = await Anonymous.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials(ApiFactory.AdminLogin, Password), TestContext.Current.CancellationToken);

        await AssertSuccessfulLoginAsync(response);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.True(body.RootElement.TryGetProperty("access_token", out _));
        Assert.True(body.RootElement.TryGetProperty("expires_in", out _));
    }

    [Fact]
    public async Task Login_AsUrlEncodedForm_ReturnsToken()
    {
        await AssertSuccessfulLoginAsync(await Anonymous.PostAsync(LoginUrl, UrlEncodedForm(ApiFactory.AdminLogin, Password), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Login_AsMultipartForm_ReturnsToken()
    {
        await AssertSuccessfulLoginAsync(await Anonymous.PostAsync(LoginUrl, MultipartForm(ApiFactory.CustomerLogin, Password), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Login_Token_ContainsExpectedClaims()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);
        var response = await Anonymous.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials(ApiFactory.AdminLogin, Password), TestContext.Current.CancellationToken);
        var result = await ReadAsync<LoginResultVm>(response);

        var token = new JsonWebTokenHandler().ReadJsonWebToken(result.AccessToken);

        Assert.Equal(ApiFactory.AdminLogin, token.Claims.Single(c => c.Type == LoginService.NameClaim).Value);
        Assert.Equal(new[] { "Admin" }, token.Claims.Where(c => c.Type == LoginService.RoleClaim).Select(c => c.Value));
        Assert.Equal(Factory.Issuer, token.Issuer);
        Assert.Equal(new[] { Factory.Audience }, token.Audiences);
        Assert.Equal(TimeSpan.FromMinutes(20), token.ValidTo - token.ValidFrom);
        Assert.InRange(token.ValidFrom, before, DateTime.UtcNow.AddSeconds(2));
        Assert.Equal(token.ValidTo.ToString("yyyy-MM-ddTHH:mm:ss"), result.ExpiresIn);
        Assert.Equal(DateTimeKind.Utc, token.ValidFrom.Kind);
    }

    [Fact]
    public async Task Login_Token_WorksForBothRolesOnProtectedEndpoint()
    {
        foreach (var login in new[] { ApiFactory.AdminLogin, ApiFactory.CustomerLogin })
        {
            using var client = Factory.CreateApiClient();
            client.DefaultRequestHeaders.Authorization = new("Bearer", await Factory.LoginAsync(login, Password));

            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken)).StatusCode);
        }
    }

    [Theory]
    [InlineData("admin@webstore.local", "zle-haslo-123")]
    [InlineData("nie-ma-takiego@webstore.local", "dowolne-haslo")]
    public async Task Login_WrongCredentials_Returns401WithGenericMessageAndNoToken(string login, string password)
    {
        var response = await Anonymous.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials(login, password), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Nieprawidłowy login lub hasło", body);
        Assert.DoesNotContain("access_token", body);
    }

    [Fact]
    public async Task Login_ManyFailedAttempts_DoNotLockTheAccount()
    {
        for (var i = 0; i < 10; i++)
        {
            var failed = await Anonymous.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials(ApiFactory.AdminLogin, "zle-haslo-" + i), TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        await AssertSuccessfulLoginAsync(
            await Anonymous.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials(ApiFactory.AdminLogin, Password), TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("""{"login": "admin@webstore.local"}""")]
    [InlineData("""{"password": "dosc-dlugie-haslo"}""")]
    [InlineData("""{"login": "abc", "password": "dosc-dlugie-haslo"}""")]
    [InlineData("""{"login": "admin@webstore.local", "password": "abc"}""")]
    [InlineData("")]
    [InlineData("{niepoprawny json")]
    public async Task Login_InvalidInput_Returns400(string json)
    {
        var response = await Anonymous.PostAsync(LoginUrl, RawJson(json), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_TooLongPassword_Returns400()
    {
        var response = await Anonymous.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials(ApiFactory.AdminLogin, new string('a', 101)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidForm_Returns400()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Anonymous.PostAsync(LoginUrl, UrlEncodedForm("abc", Password), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Anonymous.PostAsync(LoginUrl, new FormUrlEncodedContent([]), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Login_InvalidInput_DoesNotAttemptToLogIn()
    {
        var spy = new CountingLoginService();
        using var factory = new ApiFactory { ConfigureServices = s => s.AddSingleton<ILoginService>(spy) };
        using var client = factory.CreateApiClient();

        await client.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials("abc", "abc"), TestContext.Current.CancellationToken);
        await client.PostAsync(LoginUrl, RawJson(""), TestContext.Current.CancellationToken);

        Assert.Equal(0, spy.Calls);
    }

    [Fact]
    public async Task Login_UnexpectedException_Returns500WithGenericMessageAndIsLogged()
    {
        using var factory = new ApiFactory { ConfigureServices = s => s.AddSingleton<ILoginService>(new ThrowingLoginService()) };
        using var client = factory.CreateApiClient();

        var response = await client.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials(ApiFactory.AdminLogin, Password), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain(ThrowingLoginService.Secret, body);
        Assert.Contains(factory.Logs.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Error && e.Exception is not null);
    }

    [Fact]
    public async Task Login_DoesNotWritePasswordOrTokenToLogs()
    {
        var wrongPassword = "zle-haslo-" + Guid.NewGuid().ToString("N");
        await Anonymous.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials(ApiFactory.AdminLogin, wrongPassword), TestContext.Current.CancellationToken);
        var token = await Factory.LoginAsync(ApiFactory.AdminLogin, Password);
        await Anonymous.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken);

        var messages = Factory.Logs.Entries.Select(e => e.Message + e.Exception).ToList();

        Assert.NotEmpty(messages);
        Assert.DoesNotContain(messages, m => m.Contains(Password) || m.Contains(wrongPassword) || m.Contains(token));
    }

    [Fact]
    public async Task Login_IsPublic_ButRestOfApiRequiresToken()
    {
        var login = await Anonymous.PostAsJsonAsync(LoginUrl, ApiFactory.Credentials(ApiFactory.AdminLogin, Password), TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.Unauthorized, login.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken)).StatusCode);
    }

    private sealed class CountingLoginService : ILoginService
    {
        public int Calls { get; private set; }

        public Task<LoginResultVm?> LoginAsync(LoginUserVm model)
        {
            Calls++;
            return Task.FromResult<LoginResultVm?>(null);
        }
    }

    private sealed class ThrowingLoginService : ILoginService
    {
        public const string Secret = "szczegol-wewnetrzny-logowania";

        public Task<LoginResultVm?> LoginAsync(LoginUserVm model) => throw new InvalidOperationException(Secret);
    }
}
