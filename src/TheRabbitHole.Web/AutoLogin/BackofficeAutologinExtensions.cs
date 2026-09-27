using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Infrastructure.Manifest;

namespace TheRabbitHole.Web.AutoLogin;

// Workshop convenience: signs the backoffice in as the configured user, with no password, so nobody has to type
// credentials after a timeout or a branch switch. Development only. Never enable this on a real site: anyone who can
// reach the backoffice would be signed in as that user.
internal static class BackofficeAutologinExtensions
{
    public static IUmbracoBuilder AddAutoLogin(this IUmbracoBuilder builder, IHostEnvironment environment)
    {
        // Development only, whatever the configuration says.
        if (!environment.IsDevelopment())
        {
            return builder;
        }

        // The configured user email is required — if it's missing there's
        // nothing to sign in as, so don't register the auth scheme.
        string? userEmail = builder.Config.GetValue<string>("Autologin:Backoffice:Email");

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return builder;
        }

        builder.Services.ConfigureOptions<BackofficeAutologinProviderOptions>();
        builder.Services.AddSingleton<IPackageManifestReader, AutoLoginManifestReader>();

        builder.AddBackOfficeExternalLogins(logins =>
        {
            logins.AddBackOfficeLogin(authBuilder =>
            {
                authBuilder.AddRemoteScheme<AutoLoginOptions, BackofficeAutologinAuthenticationHandler>(BackOfficeAuthenticationBuilder.SchemeForBackOffice(AutoLoginOptions.AuthenticationScheme)!, "Developer login", alOptions =>
                {
                    alOptions.CallbackPath = new PathString("/umbraco-auto-login");
                    alOptions.UserEmail = userEmail;
                });
            });
        });

        return builder;
    }
}
