using WebStore.ViewModels.VM;

namespace WebStore.Web.Services;

public interface ILoginService
{
    /// <summary>Zwraca token JWT dla poprawnych danych logowania albo null, gdy login lub hasło są nieprawidłowe.</summary>
    Task<LoginResultVm?> LoginAsync(LoginUserVm model);
}
