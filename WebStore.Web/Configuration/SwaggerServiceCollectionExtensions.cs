using Microsoft.OpenApi;

namespace WebStore.Web.Configuration;

public static class SwaggerServiceCollectionExtensions
{
    public const string SchemeName = "Bearer";

    /// <summary>Dokumentacja OpenAPI z oknem „Authorize” dla tokenu JWT Bearer.</summary>
    public static IServiceCollection AddWebStoreSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "WebStore API", Version = "v1" });
            options.AddSecurityDefinition(SchemeName, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Token JWT otrzymany z POST /api/AccountApi/Login (sam token, bez słowa Bearer)."
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeName, document)] = new List<string>()
            });
        });
        return services;
    }
}
