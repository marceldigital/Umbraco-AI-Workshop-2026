using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Security;

namespace TheRabbitHole.Web.AutoLogin;

// Workshop convenience, development only: see BackofficeAutologinExtensions.
// Auto-linking connects the "Developer login" to the existing backoffice user on first use.
public class BackofficeAutologinProviderOptions : IConfigureNamedOptions<BackOfficeExternalLoginProviderOptions>
{
    public void Configure(string? name, BackOfficeExternalLoginProviderOptions options)
    {
        if (!string.Equals(name, "Umbraco." + AutoLoginOptions.AuthenticationScheme, StringComparison.Ordinal))
        {
            return;
        }

        Configure(options);
    }

    public void Configure(BackOfficeExternalLoginProviderOptions options)
        => options.AutoLinkOptions = new ExternalSignInAutoLinkOptions(true);
}
