using System.ComponentModel;
using TheRabbitHole.Core.Integrations.HubSpot;
using Umbraco.AI.Core.Tools;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace TheRabbitHole.Core.Tools;

public sealed record GetEpisodeGuestsArgs(
    [property: Description("The Umbraco content key (GUID) of the podcast episode.")]
    Guid EpisodeKey);

[AITool("get_episode_guests", "Get Episode Guests", ScopeId = "podcast")]
public sealed class GetEpisodeGuestsTool : AIToolBase<GetEpisodeGuestsArgs>
{
    private readonly IContentService _contentService;
    private readonly HubSpotGuestClient _hubspot;

    public GetEpisodeGuestsTool(IContentService contentService, HubSpotGuestClient hubspot)
    {
        _contentService = contentService;
        _hubspot = hubspot;
    }

    public override string Description =>
        "Returns the authoritative guest details (canonical name spelling, bio, and social links including " +
        "Twitter/X, LinkedIn, Bluesky, Mastodon, website) for a podcast episode. " +
        "Call this whenever you need to refer to a guest by name or reference their socials — " +
        "episode transcripts often misspell names, so always resolve via this tool using the episode's content key.";

    protected override async Task<object> ExecuteAsync(
        GetEpisodeGuestsArgs args,
        CancellationToken cancellationToken = default)
    {
        var episode = _contentService.GetById(args.EpisodeKey);
        if (episode is null)
            return new { Found = false, Message = $"No content found for key {args.EpisodeKey}." };

        if (!episode.ContentType.Alias.Equals("podcastEpisode", StringComparison.OrdinalIgnoreCase))
            return new { Found = false, Message = $"Content {args.EpisodeKey} is not a podcastEpisode." };

        // The episode stores guest email addresses in a repeatable text string, one per line.
        var emails = (episode.GetValue<string>("guests")?.Split('\n') ?? [])
            .Select(e => e.Trim())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        if (emails.Count == 0)
            return new { Found = true, EpisodeName = episode.Name, Guests = Array.Empty<GuestResult>() };

        var guests = await _hubspot.SearchByEmailsAsync(emails, cancellationToken);

        var missing = emails
            .Where(e => !guests.Any(g => string.Equals(g.Email, e, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return new
        {
            Found = true,
            EpisodeName = episode.Name,
            Guests = guests,
            MissingEmails = missing,
        };
    }
}
