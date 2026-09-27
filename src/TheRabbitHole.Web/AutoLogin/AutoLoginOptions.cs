using Microsoft.AspNetCore.Authentication;

namespace TheRabbitHole.Web.AutoLogin;

// Workshop convenience, development only: see BackofficeAutologinExtensions.
internal sealed class AutoLoginOptions : RemoteAuthenticationOptions
{
    public const string AuthenticationScheme = "AutoLogin";

    public string? UserEmail { get; set; }
}
