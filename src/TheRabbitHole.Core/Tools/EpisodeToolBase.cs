using Umbraco.AI.Core.Tools;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace TheRabbitHole.Core.Tools;

public abstract class EpisodeToolBase<TArgs> : AIToolBase<TArgs>
    where TArgs : class
{
    protected IContentService ContentService { get; }

    protected EpisodeToolBase(IContentService contentService)
    {
        ContentService = contentService;
    }

    protected (IContent? Episode, object? Error) GetEpisodeOrError(Guid episodeKey)
    {
        var episode = ContentService.GetById(episodeKey);
        if (episode is null)
            return (null, new { Success = false, Message = $"No content found for key {episodeKey}." });

        if (!episode.ContentType.Alias.Equals("podcastEpisode", StringComparison.OrdinalIgnoreCase))
            return (null, new { Success = false, Message = $"Content {episodeKey} is not a podcastEpisode." });

        return (episode, null);
    }
}
