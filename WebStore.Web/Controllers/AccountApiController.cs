using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStore.ViewModels.VM;
using WebStore.Web.Services;

namespace WebStore.Web.Controllers;

public class AccountApiController : BaseApiController
{
    private readonly ILoginService _loginService;

    public AccountApiController(ILoginService loginService, ILogger logger, IMapper mapper) : base(logger, mapper)
    {
        _loginService = loginService;
    }

    // Przyjmuje JSON oraz formularz (application/x-www-form-urlencoded i multipart/form-data).
    [HttpPost("Login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResultVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Login([FromBody] LoginUserVm model) => ExecuteAsync(async () =>
    {
        var result = await _loginService.LoginAsync(model);
        return result is null
            ? Problem(
                detail: "Nieprawidłowy login lub hasło",
                statusCode: StatusCodes.Status401Unauthorized, title: "Logowanie nie powiodło się")
            : Ok(result);
    });
}
