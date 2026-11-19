using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Tests.Infrastructure;
using WebStore.Web.Controllers;

namespace WebStore.Tests;

public class ControllerConventionTests
{
    private static readonly Assembly WebAssembly = typeof(BaseApiController).Assembly;

    private static readonly Type[] Controllers = WebAssembly.GetTypes()
        .Where(t => t.Namespace == "WebStore.Web.Controllers" && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
        .ToArray();

    private static IEnumerable<string> ControllerSources() =>
        Directory.EnumerateFiles(Path.Combine(RepositoryPaths.Root, "WebStore.Web", "Controllers"), "*.cs");

    [Fact]
    public void EveryController_InheritsBaseApiControllerAndEndsWithApiController()
    {
        Assert.Equal(6, Controllers.Length);
        Assert.All(Controllers, t =>
        {
            Assert.True(t.IsSubclassOf(typeof(BaseApiController)));
            Assert.EndsWith("ApiController", t.Name);
        });
    }

    [Fact]
    public void BaseApiController_IsAbstractApiControllerProducingJsonWithApiRoute()
    {
        var type = typeof(BaseApiController);

        Assert.True(type.IsAbstract);
        Assert.NotNull(type.GetCustomAttribute<ApiControllerAttribute>());
        Assert.Equal(new[] { "application/json" }, type.GetCustomAttribute<ProducesAttribute>()!.ContentTypes);
        Assert.Equal("api/[controller]", type.GetCustomAttribute<RouteAttribute>()!.Template);
        Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void BaseApiController_HasProtectedReadonlyLoggerAndMapperPassedThroughConstructor()
    {
        var type = typeof(BaseApiController);
        var constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Single();

        Assert.Equal(new[] { typeof(ILogger), typeof(IMapper) }, constructor.GetParameters().Select(p => p.ParameterType));
        foreach (var (name, fieldType) in new[] { ("Logger", typeof(ILogger)), ("Mapper", typeof(IMapper)) })
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
            Assert.Equal(fieldType, field.FieldType);
            Assert.True(field.IsFamily);
            Assert.True(field.IsInitOnly);
        }
    }

    [Fact]
    public void Controllers_DoNotDependOnDbContextOrEntityFramework()
    {
        var members = Controllers.Append(typeof(BaseApiController)).SelectMany(t => t
            .GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .SelectMany(MemberTypes));

        Assert.DoesNotContain(members, t => typeof(ApplicationDbContext).IsAssignableFrom(t)
                                            || t.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true);
        Assert.All(ControllerSources(), f =>
        {
            var text = File.ReadAllText(f);
            Assert.DoesNotContain("ApplicationDbContext", text);
            Assert.DoesNotContain("Microsoft.EntityFrameworkCore", text);
        });
    }

    [Fact]
    public void OnlyLoginIsPublic_AllOtherEndpointsRequireAuthorization()
    {
        foreach (var controller in Controllers)
        {
            var actions = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes<HttpMethodAttribute>().Any());

            foreach (var action in actions)
            {
                var anonymous = action.GetCustomAttribute<AllowAnonymousAttribute>() is not null;
                Assert.Equal(controller.Name == "AccountApiController" && action.Name == "Login", anonymous);
            }
        }
    }

    [Fact]
    public void OnlyProductApi_ExposesDelete()
    {
        var withDelete = Controllers
            .Where(c => c.GetMethods().Any(m => m.GetCustomAttribute<HttpDeleteAttribute>() is not null))
            .Select(c => c.Name);

        Assert.Equal(new[] { "ProductApiController" }, withDelete);
    }

    [Fact]
    public void AppSettings_ContainJwtSettingsWithoutSecretsAndTestAccountPassword()
    {
        var files = Directory.EnumerateFiles(Path.Combine(RepositoryPaths.Root, "WebStore.Web"), "appsettings*.json").ToList();
        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("SecretKey", text);
            Assert.DoesNotContain("TestAccountPassword", text);
        }

        using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryPaths.Root, "WebStore.Web", "appsettings.json")));
        var jwt = settings.RootElement.GetProperty("JwtOptions");
        Assert.False(string.IsNullOrWhiteSpace(jwt.GetProperty("Issuer").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(jwt.GetProperty("Audience").GetString()));
        Assert.Equal(20, jwt.GetProperty("TokenExpirationMinutes").GetInt32());
    }

    [Fact]
    public void LaunchSettings_ContainHttpsProfile()
    {
        var text = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "WebStore.Web", "Properties", "launchSettings.json"));

        Assert.Contains("https://localhost", text);
    }

    private static IEnumerable<Type> MemberTypes(MemberInfo member) => member switch
    {
        FieldInfo f => new[] { f.FieldType },
        PropertyInfo p => new[] { p.PropertyType },
        MethodBase m => m.GetParameters().Select(p => p.ParameterType)
            .Concat(m is MethodInfo mi ? new[] { mi.ReturnType } : Array.Empty<Type>()),
        _ => Array.Empty<Type>()
    };
}
