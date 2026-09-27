using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace TheRabbitHole.Web.AutoLogin;

// Workshop convenience, development only: see BackofficeAutologinExtensions.
// A "remote" login provider that never leaves the site: the challenge redirects straight to our own callback,
// and the callback signs in the configured user without asking for a password.
internal sealed class BackofficeAutologinAuthenticationHandler : RemoteAuthenticationHandler<AutoLoginOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;

    public BackofficeAutologinAuthenticationHandler(IOptionsMonitor<AutoLoginOptions> options, ILoggerFactory logger, UrlEncoder encoder, IHttpContextAccessor httpContextAccessor, IBackOfficeUserManager backOfficeUserManager, IBackOfficeSignInManager backOfficeSignInManager)
        : base(options, logger, encoder)
    {
        _httpContextAccessor = httpContextAccessor;
        _backOfficeUserManager = backOfficeUserManager;
        _backOfficeSignInManager = backOfficeSignInManager;
    }

    // The challenge is to redirect to our authentication handler.
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        HttpContext httpContext = _httpContextAccessor.GetRequiredHttpContext();
        httpContext.Response.Redirect(Options.CallbackPath);

        return Task.CompletedTask;
    }

    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        const string AuthenticationScheme = "Umbraco." + AutoLoginOptions.AuthenticationScheme;
        HttpContext httpContext = _httpContextAccessor.GetRequiredHttpContext();

        string redirectUrl = httpContext.Request.Query["returnUrl"].FirstOrDefault() ?? "/umbraco";
        if (!redirectUrl.StartsWith("/umbraco", StringComparison.OrdinalIgnoreCase))
        {
            redirectUrl = "/umbraco";
        }

        if (string.IsNullOrWhiteSpace(Options.UserEmail))
        {
            throw new InvalidOperationException("Unable to log in with auto login, because no user email has been specified in config");
        }

        BackOfficeIdentityUser identityUser = await _backOfficeUserManager.FindByEmailAsync(Options.UserEmail)
            ?? throw new InvalidOperationException("The user with the configured email address could not be found");

        AuthenticationProperties properties = _backOfficeSignInManager.ConfigureExternalAuthenticationProperties(AuthenticationScheme, redirectUrl, identityUser.Id);
        ClaimsPrincipal principal = await _backOfficeSignInManager.CreateUserPrincipalAsync(identityUser);

        AuthenticationTicket ticket = new(principal, properties, Constants.Security.BackOfficeExternalAuthenticationType);

        return HandleRequestResult.Success(ticket);
    }
}
