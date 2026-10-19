using WebStore.DAL.Configuration;
using WebStore.Services.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebStoreDatabase(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddWebStoreServices();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Bez zbudowanego klienta (wwwroot/index.html) strona główna wyjaśnia, jak go zbudować.
if (app.Environment.WebRootFileProvider.GetFileInfo("index.html").Exists)
{
    app.MapFallbackToFile("index.html");
}
else
{
    app.MapGet("/", () => Results.Text(
        "Klient React nie jest zbudowany. Uruchom 'npm run build' w WebStore.Web/ClientApp, 'npm run dev' lub 'dotnet publish'."));
}

app.Run();
