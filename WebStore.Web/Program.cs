using WebStore.DAL.Configuration;
using WebStore.Services.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebStoreDatabase(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddWebStoreServices();

var app = builder.Build();

// Bez katalogu wwwroot (klient nie jest zbudowany) middleware plików statycznych logowałby ostrzeżenie o braku WebRootPath,
// dlatego wtedy strona główna tylko wyjaśnia, jak zbudować klienta.
var clientBuilt = File.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html"));
if (clientBuilt)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");
}
else
{
    app.MapGet("/", () => Results.Text(
        "Klient React nie jest zbudowany. Uruchom 'npm run build' w WebStore.Web/ClientApp, 'npm run dev' lub 'dotnet publish'."));
}

app.Run();
