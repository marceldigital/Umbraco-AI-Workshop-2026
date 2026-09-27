using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace TheRabbitHole.Core;

// Notification handler that listens for when podcast episodes are saved, and enqueues them for processing
// if they have an audio file.
public class PodcastEpisodeSavedHandler(PodcastEpisodeQueue queue)
    : INotificationAsyncHandler<ContentSavedNotification>
{
    public async Task HandleAsync(ContentSavedNotification notification, CancellationToken ct)
    {
        foreach (var content in notification.SavedEntities)
        {
            // Only process content of the "podcastEpisode" document type
            if (!content.ContentType.Alias.Equals("podcastEpisode", StringComparison.OrdinalIgnoreCase))
                continue;

            // Only enqueue if there's an audio file to process
            if (string.IsNullOrWhiteSpace(content.GetValue<string>("audioFile")))
                continue;

            // Skip if this episode is already being processed. Saving the results back onto the
            // episode fires this same notification, so without this check we'd re-enqueue it
            // every time we write to it.
            if (queue.IsInFlight(content.Key))
                continue;

            // Enqueue the content key for processing by the background service
            await queue.Writer.WriteAsync(content.Key, ct);
        }
    }
}