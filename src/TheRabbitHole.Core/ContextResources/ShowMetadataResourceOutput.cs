namespace TheRabbitHole.Core.ContextResources;

public sealed class ShowMetadataResourceOutput
{
    public string? ShowName { get; set; }

    public string? ShowDescription { get; set; }

    public IReadOnlyList<EpisodeSummary> LatestEpisodes { get; set; } = Array.Empty<EpisodeSummary>();
}

public sealed record EpisodeSummary(int Number, string Name, string Summary, string Url);
