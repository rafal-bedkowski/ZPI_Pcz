namespace WebStore.Tests.Infrastructure;

public static class RepositoryPaths
{
    public static string Root { get; } = FindRoot();

    // Pliki w katalogu repozytorium (także nieśledzone przez git) przeglądane przez testy,
    // bez katalogów generowanych, zależności klienta, wymagań i definicji agentów.
    public static IEnumerable<string> RepositoryFiles()
    {
        var excluded = new[] { "/bin/", "/obj/", "/node_modules/", "/.git/", "/wwwroot/", "/wymagania/", "/.claude/" };
        return Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)
            .Where(f => !excluded.Any(e => (OperatingSystem.IsWindows() ? f.Replace('\\', '/') : f).Contains(e)));
    }

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "WebStore.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Nie znaleziono katalogu głównego solucji.");
    }
}
