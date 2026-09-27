using System.ComponentModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Umbraco.AI.Core.Tools;
using Umbraco.Cms.Core.Services;

namespace TheRabbitHole.Core.Tools;

public sealed record PublishEpisodeReportArgs(
    [property: Description("The Umbraco content key (GUID) of the episode you have just finished.")]
    Guid EpisodeKey,
    [property: Description("The final episode title.")]
    string EpisodeTitle,
    [property: Description("The 1–2 sentence plain-text summary you wrote for this episode.")]
    string Summary,
    [property: Description("Canonical guest names credited in the show notes. Empty array if no guests were mentioned.")]
    string[] GuestNames,
    [property: Description("Title of another episode of the show you linked to from the show notes, or null if none.")]
    string? RelatedEpisodeTitle);

// This tool exists as the agent's completion signal: calling it is the only way to report "episode ready"
// and deliver the structured report data to connected backoffice clients. It's hand-rolled plumbing — the
// kind of thing Umbraco.Automate would replace with a content-state workflow, leaving just the AI work here.
[AITool("publish_episode_report", "Publish Episode Report", ScopeId = "podcast")]
public sealed class PublishEpisodeReportTool : EpisodeToolBase<PublishEpisodeReportArgs>
{
    private readonly IHubContext<PodcastHub> _hub;
    private readonly ILogger<PublishEpisodeReportTool> _logger;

    public PublishEpisodeReportTool(
        IContentService contentService,
        IHubContext<PodcastHub> hub,
        ILogger<PublishEpisodeReportTool> logger) : base(contentService)
    {
        _hub = hub;
        _logger = logger;
    }

    public override string Description =>
        "File your production report once the episode is ready for publication. " +
        "Notifies connected backoffice clients that the episode has been processed and summarises what was produced. " +
        "Call exactly once, as the last thing you do, after transcript, summary, and show notes have all been saved.";

    protected override async Task<object> ExecuteAsync(
        PublishEpisodeReportArgs args,
        CancellationToken cancellationToken = default)
    {
        var (episode, error) = GetEpisodeOrError(args.EpisodeKey);
        if (episode is null)
            return error!;

        _logger.LogInformation(
            "Publishing episode report for {Id}: {Title} (guests: {GuestCount}, related: {Related})",
            args.EpisodeKey, args.EpisodeTitle, args.GuestNames.Length, args.RelatedEpisodeTitle ?? "none");

        await _hub.Clients.All.SendAsync(
            "episodeProcessed",
            args.EpisodeKey,
            args.EpisodeTitle,
            cancellationToken);

        return new { Success = true };
    }
}
