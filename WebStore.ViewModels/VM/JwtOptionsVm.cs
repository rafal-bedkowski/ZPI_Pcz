namespace WebStore.ViewModels.VM;

public class JwtOptionsVm
{
    public const string SectionName = "JwtOptions";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int TokenExpirationMinutes { get; set; } = 20;
}
