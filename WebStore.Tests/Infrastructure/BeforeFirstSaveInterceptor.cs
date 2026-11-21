using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WebStore.Tests.Infrastructure;

// Wykonuje akcję tuż przed pierwszym zapisem kontekstu, co pozwala deterministycznie odtworzyć wyścig:
// konkurencyjny zapis trafia do bazy po sprawdzeniach usługi, ale przed jej własnym zapisem.
public sealed class BeforeFirstSaveInterceptor : SaveChangesInterceptor
{
    private readonly Action _action;
    private bool _done;

    public BeforeFirstSaveInterceptor(Action action) => _action = action;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (!_done)
        {
            _done = true;
            _action();
        }

        return result;
    }
}
