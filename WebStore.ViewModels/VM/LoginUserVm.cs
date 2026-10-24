using System.ComponentModel.DataAnnotations;

namespace WebStore.ViewModels.VM;

public class LoginUserVm
{
    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string Login { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
