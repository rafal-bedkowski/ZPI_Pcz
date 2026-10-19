using Microsoft.AspNetCore.Identity;

namespace WebStore.Model.DataModels;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
}
