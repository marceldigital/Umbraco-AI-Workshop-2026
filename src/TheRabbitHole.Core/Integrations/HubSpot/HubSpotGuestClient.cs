using Microsoft.Extensions.Options;

namespace TheRabbitHole.Core.Integrations.HubSpot;

// Client for looking up podcast guests in the HubSpot CRM by their email address.
//
// WORKSHOP NOTE: this is a stand-in for the real integration. Instead of calling HubSpot's CRM search
// API (POST https://api.hubapi.com/crm/v3/objects/contacts/search with the access token from
// HubSpotOptions), it answers from a small in-memory contact list, so the workshop runs without a
// HubSpot account. The public surface matches the real client, so the code that uses it can't tell
// the difference.
public sealed class HubSpotGuestClient
{
    // The "CRM". Note the canonical spellings: transcripts regularly get these names wrong.
    private static readonly IReadOnlyList<GuestResult> Contacts =
    [
        new(Email: "sebastiaan.janssen@example.com",
            FirstName: "Sebastiaan",
            LastName: "Janssen",
            Bio: "Sebastiaan is part of the Developer Relations team at Umbraco HQ and a long-time member of the Umbraco community.",
            TwitterUrl: null,
            LinkedInUrl: null,
            BlueskyUrl: null,
            MastodonUrl: null,
            WebsiteUrl: "https://example.com/guests/sebastiaan-janssen"),
        new(Email: "lotte.pitcher@example.com",
            FirstName: "Lotte",
            LastName: "Pitcher",
            Bio: "Lotte is part of the Developer Relations team at Umbraco HQ, working with the community and its contributors.",
            TwitterUrl: null,
            LinkedInUrl: null,
            BlueskyUrl: null,
            MastodonUrl: null,
            WebsiteUrl: "https://example.com/guests/lotte-pitcher"),
        new(Email: "matt@example.com",
            FirstName: "Matt",
            LastName: "Brailsford",
            Bio: "Matt is the lead developer on the Umbraco AI project at Umbraco HQ.",
            TwitterUrl: null,
            LinkedInUrl: null,
            BlueskyUrl: null,
            MastodonUrl: null,
            WebsiteUrl: "https://example.com/guests/matt-brailsford"),
    ];

    private readonly HttpClient _http;
    private readonly HubSpotOptions _options;

    public HubSpotGuestClient(HttpClient http, IOptions<HubSpotOptions> options)
    {
        // Kept for parity with the real client, which uses these to call the HubSpot API.
        _http = http;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<GuestResult>> SearchByEmailsAsync(
        IReadOnlyList<string> emails,
        CancellationToken ct)
    {
        if (emails.Count == 0)
            return [];

        // Simulate the round-trip to HubSpot.
        await Task.Delay(TimeSpan.FromMilliseconds(250), ct);

        return Contacts
            .Where(c => emails.Contains(c.Email, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }
}

public sealed record GuestResult(
    string? Email,
    string? FirstName,
    string? LastName,
    string? Bio,
    string? TwitterUrl,
    string? LinkedInUrl,
    string? BlueskyUrl,
    string? MastodonUrl,
    string? WebsiteUrl);
