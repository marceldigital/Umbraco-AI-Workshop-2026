using TheRabbitHole.Core.Models;
using TheRabbitHole.Core.Prompts;
using Umbraco.AI.Core.Contexts.ResourceTypes;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace TheRabbitHole.Core.ContextResources;

[AIContextResourceType("show-metadata", "Show Metadata",
    Description = "Podcast show metadata including the last 3 published episodes",
    Icon = "icon-mic")]
public sealed class ShowMetadataResourceType
    : AIContextResourceTypeBase<ShowMetadataResourceSettings, ShowMetadataResourceOutput>
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly PromptTemplates _promptTemplates;

    public ShowMetadataResourceType(
        IAIContextResourceTypeInfrastructure infrastructure,
        IUmbracoContextFactory umbracoContextFactory,
        PromptTemplates promptTemplates)
        : base(infrastructure)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _promptTemplates = promptTemplates;
    }

    public override Task<ShowMetadataResourceOutput?> ResolveDataAsync(
        ShowMetadataResourceSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (settings.SiteRootId is null || settings.SiteRootId == Guid.Empty)
            return Task.FromResult<ShowMetadataResourceOutput?>(null);

        using var umbracoContext = _umbracoContextFactory.EnsureUmbracoContext();

        var root = umbracoContext.UmbracoContext?.Content.GetById(settings.SiteRootId.Value);
        if (root is null)
            return Task.FromResult<ShowMetadataResourceOutput?>(null);

        var home = root as Home;
        var episodes = root.Children<PodcastEpisodes>().First();
        var latestEpisodes = episodes
            .Children<PodcastEpisode>()
            .OrderByDescending(e => e.PublishDate ?? DateTimeOffset.MinValue)
            .Take(3)
            .Select(e => new EpisodeSummary(
                e.EpisodeNumber,
                e.Name,
                e.Summary ?? string.Empty,
                e.Url(mode: UrlMode.Absolute)))
            .ToList();

        var data = new ShowMetadataResourceOutput
        {
            ShowName = home?.SiteName ?? root.Name,
            ShowDescription = home?.SiteDescription,
            LatestEpisodes = latestEpisodes,
        };

        return Task.FromResult<ShowMetadataResourceOutput?>(data);
    }

    protected override string FormatDataForLlm(ShowMetadataResourceOutput data)
        => _promptTemplates.Render("show-metadata", data);
}
