using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebStore.Tests.Infrastructure;

namespace WebStore.Tests;

public class SetupScriptTests
{
    [Fact]
    public void US_01_07_AppSettings_DefaultProviderIsSqlite()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(RepositoryPaths.Root, "WebStore.Web", "appsettings.json")));

        Assert.Equal("Sqlite", document.RootElement.GetProperty("Database").GetProperty("Provider").GetString());
    }

    [Fact]
    public void US_01_15_CheckOnly_DoesNotAskAndDoesNotChangeUserSecrets()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Skrypt setup.ps1 dotyczy Windowsa.");

        var secretsFile = UserSecretsFile();
        var before = File.Exists(secretsFile) ? File.ReadAllBytes(secretsFile) : null;

        var startInfo = new ProcessStartInfo("powershell.exe")
        {
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            WorkingDirectory = RepositoryPaths.Root
        };
        foreach (var argument in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "setup.ps1", "-CheckOnly" })
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        process.StandardInput.Close();
        var output = process.StandardOutput.ReadToEnd();
        Assert.True(process.WaitForExit(TimeSpan.FromMinutes(2)), "setup.ps1 -CheckOnly nie zakończył się w limicie czasu.");

        Assert.DoesNotContain("[t/N]", output);
        var after = File.Exists(secretsFile) ? File.ReadAllBytes(secretsFile) : null;
        Assert.Equal(before, after);
    }

    [Fact]
    public void US_01_15_Readme_DescribesWindowsSqlServerSetupAndManualConfiguration()
    {
        var readme = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "README.md"));

        foreach (var expected in new[]
                 {
                     "setup.ps1", "Database:Provider", "ConnectionStrings:SqlServer", "dotnet user-secrets set",
                     "dotnet ef database update --context SqlServerDbContext", "WEBSTORE_TEST_SQLSERVER"
                 })
        {
            Assert.Contains(expected, readme);
        }
    }

    private static string UserSecretsFile()
    {
        var csproj = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "WebStore.Web", "WebStore.Web.csproj"));
        var id = Regex.Match(csproj, "<UserSecretsId>([^<]+)</UserSecretsId>").Groups[1].Value;
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", id, "secrets.json");
    }
}
