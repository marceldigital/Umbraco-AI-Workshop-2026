
namespace TheRabbitHole.Core.Automate.Triggers;

public sealed class PodcastEpisodeProducedOutput
{
    public Guid EpisodeKey { get; init; }
    public string EpisodeTitle { get; init; } = string.Empty;

    public int GuestCount { get; init; } = 0;
}
