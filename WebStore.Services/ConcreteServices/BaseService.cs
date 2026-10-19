using AutoMapper;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;

namespace WebStore.Services.ConcreteServices;

public abstract class BaseService
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly IMapper Mapper;
    protected readonly ILogger Logger;

    protected BaseService(ApplicationDbContext dbContext, IMapper mapper, ILogger logger)
    {
        DbContext = dbContext;
        Mapper = mapper;
        Logger = logger;
    }

    // Każdy błąd usługi trafia do loga i jest rzucany dalej, aby wywołujący mógł go obsłużyć.
    protected T Execute<T>(string operation, Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Operacja {Operation} zakończyła się błędem.", operation);
            throw;
        }
    }

    protected static KeyNotFoundException NotFound(string entity, object id) =>
        new($"Nie znaleziono rekordu '{entity}' o identyfikatorze {id}.");
}
