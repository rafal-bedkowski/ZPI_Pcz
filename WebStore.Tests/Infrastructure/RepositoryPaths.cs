namespace WebStore.Tests.Infrastructure;

public static class RepositoryPaths
{
    public static string Root { get; } = FindRoot();

    // Pliki repozytorium przeglądane przez testy, bez katalogów generowanych i zależności klienta.
    public static IEnumerable<string> TrackedFiles()
    {
        var excluded = new[] { "/bin/", "/obj/", "/node_modules/", "/.git/", "/wwwroot/", "/sprinty/", "/wymagania/", "/.claude/" };
        return Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)
            .Where(f => !excluded.Any(e => f.Replace('\\', '/').Contains(e)));
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
