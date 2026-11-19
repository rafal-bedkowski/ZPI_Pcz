using WebStore.DAL.Configuration;
using WebStore.Services.Configuration;
using WebStore.Web.Configuration;
using WebStore.Web.Infrastructure;
using WebStore.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebStoreDatabase(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddWebStoreServices();
builder.Services.AddWebStoreAuthentication();
builder.Services.AddControllers(options => options.InputFormatters.Add(new FormLoginInputFormatter()));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddWebStoreSwagger();
}

var app = builder.Build();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebStore API v1"));
}

// Bez katalogu wwwroot (klient nie jest zbudowany) middleware plików statycznych logowałby ostrzeżenie o braku WebRootPath,
// dlatego wtedy strona główna tylko wyjaśnia, jak zbudować klienta.
var clientBuilt = File.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html"));
if (clientBuilt)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (clientBuilt)
{
    // Ścieżki /api i /swagger nie trafiają do routingu klienta: nieznany adres API ma dać 404, a nie index.html.
    app.MapFallbackToFile("{*path:nonfile:regex(^(?!(api|swagger)(/|$)).*$)}", "index.html");
}
else
{
    app.MapGet("/", () => Results.Text(
        "Klient React nie jest zbudowany. Uruchom 'npm run build' w WebStore.Web/ClientApp, 'npm run dev' lub 'dotnet publish'."));
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<TestAccountSeeder>().SeedAsync();
}

app.Run();

// Udostępnia punkt wejścia testom integracyjnym (WebApplicationFactory).
public partial class Program;
