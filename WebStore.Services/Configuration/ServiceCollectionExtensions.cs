using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebStore.Services.Configuration.Profiles;
using WebStore.Services.ConcreteServices;
using WebStore.Services.Interfaces;

namespace WebStore.Services.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>Rejestruje AutoMapper z <see cref="MainProfile"/>, nieogólny <see cref="ILogger"/> i wszystkie usługi aplikacji.</summary>
    public static IServiceCollection AddWebStoreServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MainProfile>());
        services.AddScoped<ILogger>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger("WebStore"));

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IAddressService, AddressService>();
        return services;
    }
}
