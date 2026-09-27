using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace TheRabbitHole.Core.Tools;

public abstract class SaveEpisodePropertyToolBase<TArgs> : EpisodeToolBase<TArgs>
    where TArgs : class
{
    private readonly IBackOfficeSecurityAccessor _securityAccessor;
    protected SaveEpisodePropertyToolBase(IContentService contentService,
        IBackOfficeSecurityAccessor securityAccessor) : base(contentService)
    {
        _securityAccessor = securityAccessor;
    }

    protected abstract string PropertyAlias { get; }

    protected abstract (Guid EpisodeKey, object? Value) Extract(TArgs args);

    protected override Task<object> ExecuteAsync(TArgs args, CancellationToken cancellationToken = default)
    {
        var (episodeKey, value) = Extract(args);
        var (episode, error) = GetEpisodeOrError(episodeKey);
        if (episode is null)
            return Task.FromResult(error!);

        var currentUser = _securityAccessor.BackOfficeSecurity?.CurrentUser;
        
        episode.SetValue(PropertyAlias, value);
        ContentService.Save(episode, currentUser?.Id);
        return Task.FromResult<object>(new { Success = true });
    }
}
