using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace TheRabbitHole.Web.AutoLogin;

// Workshop convenience, development only: see BackofficeAutologinExtensions.
// Tells the backoffice login page about the "Developer login" provider and to redirect to it automatically.
// It's registered from code (not an umbraco-package.json file) so that it only exists when auto-login is enabled.
// Otherwise the login page would try to redirect to a provider that isn't there.
internal sealed class AutoLoginManifestReader : IPackageManifestReader
{
    public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
    {
        PackageManifest manifest = new()
        {
            Name = "Workshop Auto Login",
            AllowPublicAccess = true,
            Extensions =
            [
                new
                {
                    type = "authProvider",
                    alias = "TheRabbitHole.AuthProvider.AutoLogin",
                    name = "Developer login",
                    forProviderName = "Umbraco." + AutoLoginOptions.AuthenticationScheme,
                    meta = new
                    {
                        label = "Developer login",
                        defaultView = new { icon = "icon-user" },
                        behavior = new { autoRedirect = true },
                        linking = new { allowManualLinking = false },
                    },
                },
            ],
        };

        return Task.FromResult<IEnumerable<PackageManifest>>([manifest]);
    }
}
