using System.Text.Json.Serialization;

namespace WebStore.ViewModels.VM;

public class LoginResultVm
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    // Czas wygaśnięcia tokenu w UTC, format yyyy-MM-ddTHH:mm:ss.
    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; set; } = string.Empty;
}
