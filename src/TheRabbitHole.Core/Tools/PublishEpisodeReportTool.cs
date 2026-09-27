using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Umbraco.AI.Core.Tools;
using Umbraco.Cms.Core.Events;
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
// and deliver the structured report data. Rather than pushing a SignalR message itself (Lesson 5), it now
// announces a domain event. The PodcastEpisodeProducedTrigger surfaces that event in Umbraco Automate, so the
// reactions (toasts, emails, Slack…) become automations that editors compose in the UI.
[AITool("publish_episode_report", "Publish Episode Report", ScopeId = "podcast")]
public sealed class PublishEpisodeReportTool : EpisodeToolBase<PublishEpisodeReportArgs>
{
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<PublishEpisodeReportTool> _logger;

    public PublishEpisodeReportTool(
        IContentService contentService,
        IEventAggregator eventAggregator,
        ILogger<PublishEpisodeReportTool> logger) : base(contentService)
    {
        _eventAggregator = eventAggregator;
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

        // Announce the domain event. This code no longer knows or cares what the reactions are —
        // that's configured in the Automation section.
        await _eventAggregator.PublishAsync(
            new PodcastEpisodeProducedNotification(
                args.EpisodeKey,
                args.EpisodeTitle,
                args.Summary,
                args.GuestNames,
                args.RelatedEpisodeTitle),
            cancellationToken);

        return new { Success = true };
    }
}
