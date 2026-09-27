using Umbraco.Automate.Core.Triggers;

namespace TheRabbitHole.Core.Automate.Triggers;

// Custom Umbraco.Automate trigger that fires when the AI production pipeline finishes an episode.
//
// This is a moment no built-in trigger can express: "Content Saved" fires on every save — including
// each intermediate field the podcast-producer agent writes mid-run — while this fires exactly once,
// when transcript, summary and show notes are all done. That's the point of a custom trigger: it
// surfaces *your* domain events in the Automate UI so editors can wire up reactions without code.
//
// No registration needed — Automate discovers [Trigger] classes via the TypeLoader and, because we
// extend NotificationTriggerBase, automatically subscribes to PodcastEpisodeProducedNotification for us.
[Trigger("theRabbitHole.episodeProduced", "Podcast Episode Produced",
    Description = "Fires when the AI production pipeline finishes an episode — transcript, summary and show notes are ready.",
    Group = "Podcast",
    Icon = "icon-mic")]
public sealed class PodcastEpisodeProducedTrigger(TriggerInfrastructure infrastructure)
    : NotificationTriggerBase<PodcastEpisodeProducedSettings, PodcastEpisodeProducedOutput,
        PodcastEpisodeProducedNotification>(infrastructure)
{
    // Maps the domain notification to a trigger event. No idempotency key: the publish_episode_report
    // tool's contract is "call exactly once per production run", so there's no duplicate source to dedupe.
    public override IEnumerable<TriggerEvent> MapEvent(PodcastEpisodeProducedNotification notification)
    {
        yield return new TriggerEvent<PodcastEpisodeProducedOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = TriggerInitiatorType.System,
            InitiatorId = notification.EpisodeKey.ToString(),
            Output = new PodcastEpisodeProducedOutput
            {
                EpisodeKey = notification.EpisodeKey,
                EpisodeTitle = notification.EpisodeTitle,
                GuestCount = notification.GuestNames.Length
            },
        };
    }

    // Applies each automation's own settings to the event — the same event can fire one automation
    // and be filtered out by another.
    protected override bool CanHandle(PodcastEpisodeProducedOutput output, PodcastEpisodeProducedSettings? settings)
        => settings is not { OnlyWithGuests: true } || output.GuestCount > 0;
}
