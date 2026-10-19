using System.Reflection;
using System.Text.RegularExpressions;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.ConcreteServices;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class ArchitectureTests
{
    private static readonly Assembly Model = typeof(User).Assembly;
    private static readonly Assembly Dal = typeof(ApplicationDbContext).Assembly;
    private static readonly Assembly Services = typeof(BaseService).Assembly;
    private static readonly Assembly ViewModels = typeof(ProductVm).Assembly;

    private static IEnumerable<string> WebStoreReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name!).Where(n => n.StartsWith("WebStore.")).OrderBy(n => n);

    [Fact]
    public void US_01_01_ProjectReferences_MatchLayering()
    {
        Assert.Empty(WebStoreReferences(Model));
        Assert.Equal(new[] { "WebStore.Model" }, WebStoreReferences(Dal));
        Assert.Equal(new[] { "WebStore.DAL", "WebStore.Model", "WebStore.ViewModels" }, WebStoreReferences(Services));
    }

    [Fact]
    public void US_01_01_Solution_ContainsAllProjects()
    {
        var solution = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "WebStore.sln"));

        foreach (var name in new[] { "Model", "DAL", "Services", "ViewModels", "Web", "Tests" })
        {
            Assert.Contains($"WebStore.{name}\\WebStore.{name}.csproj", solution.Replace('/', '\\'));
        }
    }

    [Fact]
    public void US_01_01_CsprojFiles_ReferenceExpectedProjects()
    {
        Assert.Equal(new[] { "WebStore.Model" }, ProjectReferences("WebStore.DAL"));
        Assert.Equal(new[] { "WebStore.Model" }, ProjectReferences("WebStore.ViewModels"));
        Assert.Equal(new[] { "WebStore.DAL", "WebStore.Model", "WebStore.ViewModels" }, ProjectReferences("WebStore.Services"));
        Assert.Equal(new[] { "WebStore.DAL", "WebStore.Model", "WebStore.Services", "WebStore.ViewModels" }, ProjectReferences("WebStore.Web"));
        Assert.Equal(new[] { "WebStore.DAL", "WebStore.Model", "WebStore.Services" }, ProjectReferences("WebStore.Tests"));
        Assert.Empty(ProjectReferences("WebStore.Model"));
    }

    [Fact]
    public void US_01_01_AllProjects_ShareTheSameTargetFramework()
    {
        var frameworks = AllProjects()
            .Select(p => Regex.Match(File.ReadAllText(p), "<TargetFramework>(.+?)</TargetFramework>").Groups[1].Value)
            .Distinct()
            .ToList();

        Assert.Equal(new[] { "net10.0" }, frameworks);
    }

    [Fact]
    public void US_01_01_GlobalJson_PinsSdkMatchingTargetFramework()
    {
        var globalJson = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "global.json"));

        Assert.Matches("\"version\":\\s*\"10\\.", globalJson);
    }

    [Fact]
    public void US_01_01_WebProject_HasNoApiControllersNorSwagger()
    {
        var web = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "WebStore.Web", "WebStore.Web.csproj"));
        var program = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "WebStore.Web", "Program.cs"));

        Assert.DoesNotContain("Swashbuckle", web);
        Assert.DoesNotContain("Swagger", program, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MapControllers", program);
        Assert.DoesNotContain(
            Directory.EnumerateFiles(Path.Combine(RepositoryPaths.Root, "WebStore.Web"), "*Controller.cs", SearchOption.AllDirectories),
            f => !f.Contains("node_modules"));
    }

    [Fact]
    public void US_01_06_ModelServicesViewModels_DoNotReferenceDatabaseProviders()
    {
        foreach (var assembly in new[] { Model, Services, ViewModels })
        {
            var references = assembly.GetReferencedAssemblies().Select(a => a.Name!);
            Assert.DoesNotContain(references, n => n.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
                                                  || n.Contains("SqlServer", StringComparison.OrdinalIgnoreCase)
                                                  || n.Contains("SqlClient", StringComparison.OrdinalIgnoreCase));
        }

        foreach (var project in new[] { "WebStore.Model", "WebStore.Services", "WebStore.ViewModels" })
        {
            var csproj = File.ReadAllText(Path.Combine(RepositoryPaths.Root, project, $"{project}.csproj"));
            Assert.DoesNotContain("Sqlite", csproj);
            Assert.DoesNotContain("SqlServer", csproj);
        }
    }

    [Fact]
    public void US_01_07_Repository_ContainsNoSqlServerConnectionStringOrPassword()
    {
        var pattern = new Regex(@"(Password|Pwd)\s*=\s*[^;""'\s]+|User\s*Id\s*=", RegexOptions.IgnoreCase);
        var offenders = RepositoryPaths.TrackedFiles()
            .Where(f => f.EndsWith(".cs") || f.EndsWith(".json") || f.EndsWith(".csproj") || f.EndsWith(".md"))
            .Where(f => !f.Contains("ArchitectureTests.cs") && !f.Contains("package-lock.json") && !f.Contains("/Migrations/"))
            .Where(f => pattern.IsMatch(File.ReadAllText(f)))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void US_01_07_GitIgnore_ExcludesSqliteDatabaseFile()
    {
        var gitignore = File.ReadAllLines(Path.Combine(RepositoryPaths.Root, ".gitignore")).Select(l => l.Trim());

        Assert.Contains("*.db", gitignore);
    }

    [Fact]
    public void US_01_10_TestProject_UsesXunitV3()
    {
        var csproj = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "WebStore.Tests", "WebStore.Tests.csproj"));

        Assert.Contains("Include=\"xunit.v3\"", csproj);
        Assert.DoesNotContain("Include=\"xunit\"", csproj);
        Assert.Contains("Xunit.DependencyInjection", csproj);
    }

    private static IEnumerable<string> AllProjects() =>
        Directory.EnumerateFiles(RepositoryPaths.Root, "WebStore.*.csproj", SearchOption.AllDirectories)
            .Where(f => !f.Contains("/node_modules/"));

    private static IEnumerable<string> ProjectReferences(string project)
    {
        var csproj = File.ReadAllText(Path.Combine(RepositoryPaths.Root, project, $"{project}.csproj"));
        return Regex.Matches(csproj, @"ProjectReference Include=""[^""]*?(WebStore\.\w+)\.csproj""")
            .Select(m => m.Groups[1].Value)
            .OrderBy(n => n)
            .ToList();
    }
}
