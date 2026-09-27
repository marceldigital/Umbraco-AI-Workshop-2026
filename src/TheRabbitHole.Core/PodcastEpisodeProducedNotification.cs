using Umbraco.Cms.Core.Notifications;

namespace TheRabbitHole.Core;

public sealed class PodcastEpisodeProducedNotification(
    Guid episodeKey,
    string episodeTitle,
    string summary,
    string[] guestNames,
    string? relatedEpisodeTitle) : INotification
{
    public Guid EpisodeKey { get; } = episodeKey;
    public string EpisodeTitle { get; } = episodeTitle;
    public string Summary { get; } = summary;
    public string[] GuestNames { get; } = guestNames;
    public string? RelatedEpisodeTitle { get; } = relatedEpisodeTitle;
}
