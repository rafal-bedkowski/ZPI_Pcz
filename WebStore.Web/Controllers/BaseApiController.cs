using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebStore.Web.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
[Route("api/[controller]")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public abstract class BaseApiController : ControllerBase
{
    protected readonly ILogger Logger;
    protected readonly IMapper Mapper;

    protected BaseApiController(ILogger logger, IMapper mapper)
    {
        Logger = logger;
        Mapper = mapper;
    }

    // Wspólna obsługa błędów usług: kontroler tłumaczy wyjątki na kody HTTP, a logikę biznesową zostawia usługom.
    protected IActionResult Execute(Func<IActionResult> action, Func<Exception, IActionResult?>? onError = null,
        bool saving = false)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            return HandleException(ex, onError, saving);
        }
    }

    protected async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action,
        Func<Exception, IActionResult?>? onError = null)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            return HandleException(ex, onError, saving: false);
        }
    }

    protected IActionResult GetOne<TVm>(Func<TVm?> find) where TVm : class =>
        Execute(() => find() is { } vm ? Ok(vm) : NotFound());

    // POST: Id musi być puste, a odpowiedź 201 wskazuje nowy zasób w nagłówku Location.
    protected IActionResult Create<TVm>(int? id, Func<TVm> save, Func<TVm, IActionResult> created,
        Func<Exception, IActionResult?>? onError = null)
    {
        if (id is not (null or 0))
        {
            return Problem(
                detail: "Przy dodawaniu nowego rekordu pole Id musi być puste.",
                statusCode: StatusCodes.Status400BadRequest, title: "Błąd walidacji");
        }

        return Execute(() => created(save()), onError, saving: true);
    }

    // PUT: Id jest wymagane, a nieistniejący rekord daje 404 (osobno od nieistniejących powiązań, które dają 400).
    protected IActionResult Update<TVm>(int? id, Func<int, bool> exists, Func<TVm> save,
        Func<Exception, IActionResult?>? onError = null)
    {
        if (id is null or 0)
        {
            return Problem(
                detail: "Przy modyfikacji rekordu pole Id jest wymagane.",
                statusCode: StatusCodes.Status400BadRequest, title: "Błąd walidacji");
        }

        return Execute(() => exists(id.Value) ? Ok(save()) : NotFound(), onError, saving: true);
    }

    private IActionResult HandleException(Exception ex, Func<Exception, IActionResult?>? onError, bool saving)
    {
        if (onError?.Invoke(ex) is { } custom)
        {
            return custom;
        }

        switch (ex)
        {
            case ValidationException:
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Błąd walidacji");
            // Przy zapisie "nie znaleziono" dotyczy powiązanego rekordu (np. klienta), czyli błędu danych wejściowych.
            case KeyNotFoundException when saving:
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Błąd walidacji");
            case KeyNotFoundException:
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound, title: "Nie znaleziono zasobu");
            default:
                Logger.LogError(ex, "Nieoczekiwany błąd podczas obsługi żądania {Method} {Path}.",
                    Request.Method, Request.Path.Value);
                return Problem(
                    detail: "Wystąpił nieoczekiwany błąd serwera.",
                    statusCode: StatusCodes.Status500InternalServerError, title: "Błąd serwera");
        }
    }
}
